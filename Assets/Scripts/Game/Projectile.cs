using UnityEngine;
using System;
using System.Runtime.CompilerServices;


[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private Rigidbody _rb;
    private ProjectilePool _pool;
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

    // ProjectilePool에서 호출
    public void Initialize(Entity owner, float damage, float coreDamage, Transform target)
    {
        this.damage = damage;
        this.target = target;
        this.coreDamage = coreDamage;
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

        
        // 1) 단일 대상 즉시 피격 (거리 기반)
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= data.detectionRadius)
            {
                if (Vector3.Distance(transform.position, target.position) <= data.detectionRadius)
                {
                    // 즉시 데미지 & 반환
                    var hp = target.GetComponent<HealthComponent>();
                    if (hp != null) hp.TakeDamage(coreDamage != 0 ? coreDamage : damage);
                }

                // 2) 폭발 반경이 있으면 주변 데미지
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
                            otherHp.TakeDamage(coreDamage != 0 ? coreDamage : damage);
                    }
                }

                ReturnFlightEffect();
                ReturnToPool();
                return;
            }
        }
    }
    public void SetPool(ProjectilePool pool)
    {
        _pool = pool;
    }

    private void ReturnToPool()
    {
        // **이펙트 먼저 반드시 반환**
        ReturnFlightEffect();

        // 본인 풀로 반환 (없으면 Destroy)
        if (_pool != null)
        {
            _pool.ReturnProjectile(this);
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// FlightEffect를 EffectPool에서 받아와 부착
    /// </summary>
    private void AttachFlightEffect()
    {
        // 기존 이펙트 반환
        ReturnFlightEffect();

        if (data.FlightEffectPrefab == null) return;

        // EffectPoolManager에서 풀 획득
        var pool = EffectPoolManager.Instance.GetPool(data.FlightEffectPrefab.name);
        if (pool != null)
        {
            flightEffect = pool.GetEffect(transform.position, Quaternion.identity);
            flightEffect.transform.SetParent(transform, false);
            flightEffect.transform.localPosition = Vector3.zero;
        }
        else
        {
            // 풀을 못 찾으면 비상용으로 Instantiate (원치 않으면 삭제)
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

        var pool = EffectPoolManager.Instance.GetPool(data.FlightEffectPrefab.name);
        if (pool != null)
        {
            flightEffect.transform.SetParent(pool.transform, false);
            pool.ReturnEffect(flightEffect);
        }
        else
        {
            Destroy(flightEffect);
        }
            

        flightEffect = null;
    }

    // Pool에서 꺼낼 때 반드시 Initialize를 호출해야 함

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
