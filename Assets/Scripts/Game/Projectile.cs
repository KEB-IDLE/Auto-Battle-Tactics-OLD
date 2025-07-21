using TMPro.Examples;
using UnityEngine;
using System;


[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour, IEffectNotifier
{
    private Rigidbody _rb;
    private ProjectilePool _pool;


    [Header("Projectile Scriptable Object�� �Ҵ��ϼ���.")]
    [Tooltip("Asset > Scripts > Game > ScriptableObject > Unit ���� �� ���ϴ� ������ ����")]
    [SerializeField] private ProjectileData data;

    //private Entity owner;
    private Team team; // ��ȯ���� ��(���� ����)
    private float timer;
    private float damage;
    private float coreDamage;

    private Transform target;

#pragma warning disable 67 // '�̺�Ʈ ��� ����' ��� ����
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

        // �����ϰ� Team ���� �ޱ�
        var teamComponent = owner.GetComponent<TeamComponent>();
        if (teamComponent != null)
            this.team = teamComponent.Team;
        else
            Debug.LogWarning("[Projectile] Owner�� TeamComponent�� �����ϴ�!");

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
            // ���� �Ʒ��� ����
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
            //���⿡ Ÿ�� ����Ʈ �߰�
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
