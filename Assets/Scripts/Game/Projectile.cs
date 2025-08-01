using UnityEngine;
using System;
using System.Runtime.CompilerServices;


[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private Rigidbody _rb;
    private string poolName; // 어떤 풀에 속한 오브젝트인지
    private GameObject flightEffect;

    [Header("Projectile Scriptable Object를 할당하세요.")]
    [SerializeField] private ProjectileData data;

    private Team team;
    private float timer;
    private float lifeTime;
    private float damage;
    private float coreDamage;
    private Transform targetEntity;
    private Transform hitPoint;
    private float disengageRange = 100f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
    }

    // ObjectPoolManager를 통한 초기화
    public void Initialize(Entity owner, float damage, float coreDamage, Transform targetEntity, Transform hitPoint, string poolName, float disengageRange)
    {
        this.damage = damage;
        this.targetEntity = targetEntity;
        this.hitPoint = hitPoint;
        this.coreDamage = coreDamage;
        this.poolName = poolName;
        this.disengageRange = disengageRange;
        timer = 0f;
        lifeTime = 5f;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        
        SetTeam(owner);
        AttachFlightEffect();

        if (this.targetEntity != null)
        {
            float distance = Vector3.Distance(transform.position, this.targetEntity.position);

            // 가중치(Weight)에 따라 자동 보정
            float autoSpeed = Mathf.Max(distance * data.speedWeight, 10f);
            float autoVerticalSpeed = Math.Max(distance * data.verticalSpeedWeight, 4f);

            Vector3 direction = (this.targetEntity.position - transform.position).normalized;
            _rb.linearVelocity = direction * autoSpeed + Vector3.up * autoVerticalSpeed;
        }
    }

    private void SetTeam(Entity owner)
    {
        var teamComponent = owner.GetComponent<TeamComponent>();
        if (teamComponent != null)
            this.team = teamComponent.Team;
        else
            Debug.LogWarning("[Projectile] Owner에 TeamComponent가 없습니다!");
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (ShouldReturnToPool())
            return;

        Move();
        CheckHit();
    }

    private bool ShouldReturnToPool()
    {
        if (targetEntity == null || timer >= lifeTime)
            return ReturnProjectile();

        var health = targetEntity.GetComponent<HealthComponent>();
        if (health != null && !health.IsTargetable())
            return ReturnProjectile();

        float dist = Vector3.Distance(transform.position, targetEntity.position);
        if (dist > disengageRange)
            return ReturnProjectile();

        return false;
    }

    private bool ReturnProjectile()
    {
        ReturnFlightEffect();
        ReturnToPool();
        return true;
    }

    private void Move()
    {
        //if (_rb == null || target == null) return;
        //Vector3 toTarget = (target.position - transform.position).normalized;
        //float currentSpeed = _rb.linearVelocity.magnitude;

        //// 살짝 유도 (0.08~0.18 사이에서 실험 추천)
        //_rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, toTarget * currentSpeed, 0.12f);
        //if (_rb.linearVelocity.sqrMagnitude > 0.01f)
        //    transform.forward = _rb.linearVelocity.normalized;

        if (_rb == null || hitPoint == null) return;
        Vector3 toTarget = (hitPoint.position - transform.position).normalized;
        float currentSpeed = _rb.linearVelocity.magnitude;
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, toTarget * currentSpeed, 0.12f);
        if (_rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.forward = _rb.linearVelocity.normalized;
    }

    protected virtual void CheckHit()
    {
        if (targetEntity != null && hitPoint != null)
        {
            float dist = Vector3.Distance(transform.position, hitPoint.position);
            if (dist <= data.detectionRadius)
            {
                var hp = targetEntity.GetComponent<HealthComponent>();
                var core = targetEntity.GetComponent<Core>();

                float damageToApply = (core != null) ? coreDamage : damage;

                if (hp != null) hp.RequestDamage(damageToApply);
                hp?.ApplyImmediateDamage();
                if (data.explosionRadius > 0f)
                {
                    Collider[] explosionHits = Physics.OverlapSphere(
                        transform.position,
                        data.explosionRadius,
                        LayerMask.GetMask("Agent", "Tower", "Core"));
                    foreach (var ex in explosionHits)
                    {
                        var enemy = ex.GetComponent<Entity>();
                        if (enemy == null) continue;
                        var teamComp = enemy.GetComponent<TeamComponent>();
                        if (teamComp == null || teamComp.Team == team) continue;

                        var otherHp = enemy.GetComponent<HealthComponent>();
                        var otherCore = enemy.GetComponent<Core>();
                        float explosionDamage = (otherCore != null) ? coreDamage : damage;
                        if (otherHp != null)
                        {
                            otherHp.RequestDamage(explosionDamage);
                            otherHp?.ApplyImmediateDamage();
                        }
                    }
                }
                ReturnFlightEffect();
                ReturnToPool();
                return;
            }
        }
    }

    /// <summary>
    /// 풀 이름을 직접 지정해줌
    /// </summary>
    public void SetPoolName(string poolName) => this.poolName = poolName;

    private void ReturnToPool()
    {
        ReturnFlightEffect();

        if (!string.IsNullOrEmpty(poolName))
        {
            ObjectPoolManager.Instance.Return(poolName, this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// FlightEffect를 EffectPool에서 받아와 부착
    /// </summary>
    private void AttachFlightEffect()
    {
        ReturnFlightEffect();

        if (data.FlightEffectPrefab == null) return;

        var pool = ObjectPoolManager.Instance.GetPool(data.FlightEffectPrefab.name);
        if (pool != null)
        {
            flightEffect = pool.Get(transform.position, Quaternion.identity);
            flightEffect.transform.SetParent(transform, false);
            flightEffect.transform.localPosition = Vector3.zero;
        }
        else
        {
            flightEffect = Instantiate(data.FlightEffectPrefab, transform);
            flightEffect.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// 현재 FlightEffect를 반드시 풀로 반환/제거
    /// </summary>
    private void ReturnFlightEffect()
    {
        if (flightEffect == null) return;
        flightEffect.transform.SetParent(null);

        var pool = ObjectPoolManager.Instance.GetPool(data.FlightEffectPrefab.name);
        if (pool != null)
        {
            if(pool is MonoBehaviour poolObj)
                flightEffect.transform.SetParent(poolObj.transform, false);
            pool.Return(flightEffect);
        }
        else
        {
            Destroy(flightEffect);
        }
        flightEffect = null;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRadius);

        Gizmos.color = Color.brown;
        Gizmos.DrawWireSphere(transform.position, data.explosionRadius);
    }
#endif
}
