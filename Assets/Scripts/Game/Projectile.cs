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
    private float damage;
    private float coreDamage;
    private Transform target;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
    }

    // ObjectPoolManager를 통한 초기화
    public void Initialize(Entity owner, float damage, float coreDamage, Transform target, string poolName)
    {
        this.damage = damage;
        this.target = target;
        this.coreDamage = coreDamage;
        this.poolName = poolName; // 어떤 풀에 반환해야 할지 기억
        timer = 0f;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        SetTeam(owner);
        AttachFlightEffect();

        // 발사 방향 초기화
        if (target != null)
        {
            _rb.linearVelocity = (target.position - transform.position)
                                .normalized * data.speed + Vector3.up * data.verticalSpeed;
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
        if (timer > data.lifeTime || target == null)
        {
            ReturnFlightEffect();
            ReturnToPool();
            return;
        }
        Move();
        CheckHit();
    }

    private void Move()
    {
        if (target == null) return;
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * data.speed * Time.deltaTime;
        transform.forward = dir;
    }

    protected virtual void CheckHit()
    {
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= data.detectionRadius)
            {
                var hp = target.GetComponent<HealthComponent>();
                if (hp != null) hp.RequestDamage(coreDamage != 0 ? coreDamage : damage);

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
                        if (otherHp != null)
                            otherHp.RequestDamage(coreDamage != 0 ? coreDamage : damage);
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
    public void SetPoolName(string poolName)
    {
        this.poolName = poolName;
    }

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
