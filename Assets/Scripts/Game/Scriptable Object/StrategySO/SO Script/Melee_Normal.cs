using UnityEngine;

[CreateAssetMenu(menuName = "AttackStrategy/Melee/Normal")]
public class Melee_Normal : AttackStrategyBaseSO
{
    public override void Attack(AttackComponent context, IDamageable target)
    {
        if (context == null || context.teamProvider == null)
            return;

        MonoBehaviour mb = target as MonoBehaviour;
        Transform aim = context.transform; // 기본값: 자기 자신(타겟 없을 때 대비)

        if (target != null && target.IsAlive() && mb != null && mb.gameObject != context.gameObject)
        {
            // 아군/적군 판정
            var provider = mb.GetComponent<ITeamProvider>();
            bool isEnemy = provider == null || provider.Team != context.teamProvider.Team;

            // 실피해는 disengageRange 기준
            float sqrDist = (mb.transform.position - context.transform.position).sqrMagnitude;
            bool inDisengage = sqrDist <= (context.disengageRange * context.disengageRange);

            if (isEnemy && inDisengage)
            {
                context.lockedTarget = target;

                var core = mb.GetComponent<Core>();
                context.RequestAttackSound();
                target.RequestDamage(core != null ? context.attackCoreDamage : context.attackDamage);
                (target as HealthComponent)?.ApplyImmediateDamage();
            }

            // Magic_Fire처럼 타겟 Transform을 이펙트 에임 기준으로 넘김
            //aim = mb.transform;
            aim = context.firePoint;
        }

        // 명중/빗맞음 무관하게 한 번만 이벤트 발신
        context.EventSender(aim);
    }

    public override void DrawGizmos(AttackComponent context, EntityData data)
    {
#if UNITY_EDITOR
        if (context == null) return;

        Vector3 origin = context.transform.position;

        // 공격 진입 반경(애니메이션 트리거)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, data.attackRange);

        // 실피해 반경(disengageRange)
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.15f);
        Gizmos.DrawWireSphere(origin, data.disengageRange);
#endif
    }
}
