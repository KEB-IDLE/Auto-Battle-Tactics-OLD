using TMPro.Examples;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField] private ProjectileData data;

    private Entity owner;
    private Team team; // ��ȯ���� ��(���� ����)
    private float timer;
    private float damage;
    private float coreDamage;

    private Transform target;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
    }

    public void Initialize(Entity owner, float damage, float coreDamage, Transform target)
    {
        this.owner = owner;
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
            Destroy(gameObject);
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
                if (enemy == null || enemy.GetComponent<TeamComponent>().Team == team)
                    continue;

                var hp = enemy.GetComponent<HealthComponent>();
                if (hp != null)
                    hp.TakeDamage(damage); // �ʿ��ϴٸ� falloff ���� �� �߰� ����
            }


            Destroy(gameObject);
            break;
        }
    }
}
