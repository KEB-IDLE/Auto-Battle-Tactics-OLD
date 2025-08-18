using UnityEngine;

[CreateAssetMenu(fileName = "CornMeleeAttack", menuName = "AttackStrategy/Melee/Cone")]
public class Melee_Cone : AttackStrategyBaseSO
{
    [Header("범위 피해(부채꼴) 세팅")]
    private float coneAngle = 55f;

    public override void Attack(AttackComponent context, IDamageable target)
    {
        if (context == null || context.teamProvider == null) return;

        //float radius = context.attackRange;
        float radius = context.disengageRange;
        float angle = coneAngle;

        // 범위 내 적 탐색
        Collider[] hits = Physics.OverlapSphere(context.transform.position, radius, context.targetLayer);

        foreach (var col in hits)
        {

            if (col.gameObject == context.gameObject) continue;
            var dmg = col.GetComponent<IDamageable>();
            if (dmg == null || !dmg.IsAlive()) continue;

            var provider = col.GetComponent<ITeamProvider>();
            if (provider == null || provider.Team == context.teamProvider.Team) continue;


            Vector3 origin = context.transform.position;
            Vector3 forward = context.transform.forward; forward.y = 0;
            Vector3 toTarget = col.transform.position - origin; toTarget.y = 0;

            float angleToTarget = Vector3.Angle(forward, toTarget);
            if (angleToTarget > coneAngle * 0.5f) continue;

            var core = col.GetComponent<Core>();
            context.RequestAttackSound();
            dmg.RequestDamage(core != null ? context.attackCoreDamage : context.attackDamage);
            (dmg as HealthComponent)?.ApplyImmediateDamage();
        }
        context.EventSender(context.transform);
    }
    public override void DrawGizmos(AttackComponent context, EntityData data)
    {
#if UNITY_EDITOR
        if (context == null) return;

        float angle = coneAngle;
        float attackRadius = data.attackRange;
        float disengageRadius = data.disengageRange;
        Vector3 origin = context.transform.position;
        Vector3 forward = context.transform.forward; forward.y = 0;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, attackRadius);

        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.15f);
        DrawWireCone(origin, forward, angle, disengageRadius);

        Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.25f);
        DrawWireCone(origin, forward, angle, attackRadius);
#endif
    }

    private void DrawWireCone(Vector3 origin, Vector3 forward, float angle, float radius)
    {
#if UNITY_EDITOR
        int segments = 60;
        float halfAngle = angle * 0.5f;
        Quaternion leftRot = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(halfAngle, Vector3.up);

        Vector3 prev = origin + (leftRot * forward) * radius;
        for (int i = 1; i <= segments; ++i)
        {
            float lerp = (float)i / segments;
            Quaternion rot = Quaternion.Slerp(leftRot, rightRot, lerp);
            Vector3 next = origin + (rot * forward) * radius;
            Gizmos.DrawLine(prev, next);
            Gizmos.DrawLine(origin, next);
            prev = next;
        }
#endif
    }
}
