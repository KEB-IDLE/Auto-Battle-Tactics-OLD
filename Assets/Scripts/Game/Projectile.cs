using TMPro.Examples;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField] private ProjectileData data;

    private Entity owner;
    private Team team; // 소환자의 팀(직접 저장)
    private float timer;
    private float damage;

    private Transform target;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
    }

    public void Initialize(/*ProjectileData data, */ Entity owner, float damage, Transform target)
    {
        this.owner = owner;
        this.damage = damage;
        this.target = target;
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

    // Update is called once per frame
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
            // 위를 아래로 변경
            var e = hit.GetComponent<Entity>();
            if (e == null || e.GetComponent<TeamComponent>().Team == team)
                continue;

            var hp = e.GetComponent<HealthComponent>();
            if (hp != null)
                hp.TakeDamage(damage);

            Destroy(gameObject);
            break;

        }
    }
}
