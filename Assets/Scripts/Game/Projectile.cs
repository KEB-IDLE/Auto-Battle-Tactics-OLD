using TMPro.Examples;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public ProjectileData data;
    private Entity owner;
    private Team team; // ��ȯ���� ��(���� ����)
    private float lifeTime;
    private float timer;
    private float damage;
    private float detectionRadius;
    private float explosionRadius;

    public void Initialize(ProjectileData data, Entity owner, float damage)
    {
        this.data = data;
        this.owner = owner;
        this.lifeTime = data.lifeTime;
        this.detectionRadius = data.detectionRadius;
        this.damage = damage;
        timer = 0f;

        // �����ϰ� Team ���� �ޱ�
        var teamComponent = owner.GetComponent<TeamComponent>();
        if (teamComponent != null)
            this.team = teamComponent.Team;
        else
            Debug.LogWarning("[Projectile] Owner�� TeamComponent�� �����ϴ�!");
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, data.detectionRadius, LayerMask.GetMask("Agent", "Tower", "Core"));

        foreach (var hit in hits)
        {
            // 1. �� ���� ������ ����
            Entity targetEntity = hit.GetComponent<Entity>();
            if (targetEntity == null)
                continue;

            TeamComponent targetTeamComp = targetEntity.GetComponent<TeamComponent>();
            if (targetTeamComp == null)
                continue;

            if (targetTeamComp.Team == this.team)
                continue;

            HealthComponent health = targetEntity.GetComponent<HealthComponent>();


            if (health != null)
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
                break;
            }
        }


    }
}
