using TMPro.Examples;
using UnityEngine;
using System;


[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour, IEffectNotifier
{
    private Rigidbody _rb;
    private ProjectilePool _pool;


    [Header("Projectile Scriptable Object를 할당하세요.")]
    [Tooltip("Asset > Scripts > Game > ScriptableObject > Unit 선택 후 원하는 데이터 삽입")]
    [SerializeField] private ProjectileData data;

    //private Entity owner;
    private Team team; // 소환자의 팀(직접 저장)
    private float timer;
    private float damage;
    private float coreDamage;

    private Transform target;

#pragma warning disable 67 // '이벤트 사용 안함' 경고 억제
    public event Action<Transform> OnAttackEffect;
    public event Action<Transform> OnTakeDamageEffect;
    public event Action<Transform> OnDeathEffect;
#pragma warning restore 67
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
    }

    public void Initialize(Entity owner, float damage, float coreDamage, Transform target)
    {
        //this.owner = owner;
        this.damage = damage;
        this.target = target;
        this.coreDamage = coreDamage;
        timer = 0f;

        // 안전하게 Team 정보 받기
        var teamComponent = owner.GetComponent<TeamComponent>();
        if (teamComponent != null)
            this.team = teamComponent.Team;
        else
            Debug.LogWarning("[Projectile] Owner에 TeamComponent가 없습니다!");

        _rb.linearVelocity = (target.position - transform.position).normalized
                       * data.speed + Vector3.up * data.verticalSpeed;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(timer > data.lifeTime || target == null /*lifeTime*/)
        {
            ReturnToPool();
            return;
        }
        Move();
        CheckHit();
    }

    protected virtual void Move()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * data.speed * Time.deltaTime;
        transform.forward = dir;
    }

    protected virtual void CheckHit()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            data.detectionRadius,
            LayerMask.GetMask("Agent", "Tower", "Core"));

        foreach (var hit in hits)
        {
            // 위를 아래로 변경
            var e = hit.GetComponent<Entity>();
            if (e == null || e.GetComponent<TeamComponent>().Team == team)
                continue;

            Collider[] explosionHits = Physics.OverlapSphere(
                transform.position,
                data.explosionRadius,
                LayerMask.GetMask("Agent", "Tower", "Core"));

            foreach (var ex in explosionHits)
            {
                var enemy = ex.GetComponent<Entity>();
                if (enemy == null)
                    continue;

                var teamComp = enemy.GetComponent<TeamComponent>();
                if (teamComp == null || teamComp.Team == team)
                    continue;

                var hp = enemy.GetComponent<HealthComponent>();
                var coreComp = enemy.GetComponent<Core>();

                if (hp == null)
                    continue;

                hp.TakeDamage(coreComp != null ? coreDamage : damage);
            }
            //여기에 타격 이펙트 추가
            OnAttackEffect?.Invoke(this.transform);
            ReturnToPool();
            break;
        }
    }
    public void SetPool(ProjectilePool pool)
    {
        _pool = pool;
    }

    private void ReturnToPool()
    {
        if (_pool != null)
            _pool.ReturnProjectile(this);
        else
            Destroy(gameObject);
    }



#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (data == null) return;

        // detectionRadius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRadius);

        // attackRange
        Gizmos.color = Color.brown;
        Gizmos.DrawWireSphere(transform.position, data.explosionRadius);
    }
#endif
}
