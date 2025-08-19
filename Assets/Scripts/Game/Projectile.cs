using UnityEngine;
using System;
using System.Runtime.CompilerServices;


[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private Rigidbody _rb;
    private string poolName;
    private GameObject flightEffect;

    [Header("Projectile Scriptable Object.")]
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
            Debug.LogWarning("[Projectile] Owner�� TeamComponent�� �����ϴ�!");
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

        if (_rb == null || hitPoint == null) return;
        Vector3 toTarget = (hitPoint.position - transform.position).normalized;
        float currentSpeed = _rb.linearVelocity.magnitude;
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, toTarget * currentSpeed, 0.12f);
        if (_rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion forward = Quaternion.LookRotation(_rb.linearVelocity.normalized);
            transform.rotation = forward * Quaternion.Euler(data.localRotationOffset);
        }
    }

    protected virtual void CheckHit()
    {
        if (targetEntity != null && hitPoint != null)
        {
            float dist = Vector3.Distance(transform.position, hitPoint.position);
            if (dist <= data.detectionRadius)
            {
                var hp = targetEntity.GetComponent<HealthComponent>();
                //var core = targetEntity.GetComponent<Core>();

                float damageToApply = damage;
                //float damageToApply = (core != null) ? coreDamage : damage;

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
                        //var otherCore = enemy.GetComponent<Core>();
                        float explosionDamage = damage;
                        //float explosionDamage = (otherCore != null) ? coreDamage : damage;
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
    /// FlightEffect�� EffectPool���� �޾ƿ� ����
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
    /// ���� FlightEffect�� �ݵ�� Ǯ�� ��ȯ/����
    /// </summary>
    private void ReturnFlightEffect()
    {
        if (flightEffect == null) return;
        flightEffect.transform.SetParent(null);

        var pool = ObjectPoolManager.Instance.GetPool(data.FlightEffectPrefab.name);
        if (pool != null)
        {
            if (pool is MonoBehaviour poolObj)
                flightEffect.transform.SetParent(poolObj.transform, false);
            pool.Return(flightEffect);
        }
        else
        {
            Destroy(flightEffect);
        }
        flightEffect = null;
    }

    public void Init(GameObject _target)
    {
        //target = _target;

        //isMoving = true;
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
