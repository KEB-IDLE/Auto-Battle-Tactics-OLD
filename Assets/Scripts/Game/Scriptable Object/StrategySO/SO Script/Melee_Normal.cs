//using UnityEngine;

//[CreateAssetMenu(menuName = "AttackStrategy/Melee/Normal")]
//public class Melee_Normal : AttackStrategyBaseSO
//{
//    public override void Attack(AttackComponent context, IDamageable target)
//    {
//        if (target == null || !target.IsAlive())
//            return;

//        context.lockedTarget = target;
//        context.EventSender(context.transform);

//        var coreComp = (target as MonoBehaviour).GetComponent<Core>();
//        target.RequestDamage(coreComp != null ? context.attackCoreDamage : context.attackDamage);
//        (target as HealthComponent)?.ApplyImmediateDamage();
//    }

//    public override void DrawGizmos(AttackComponent context, EntityData data)
//    {
//#if UNITY_EDITOR
//        if (context == null) return;

//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(context.transform.position, data.attackRange);
//#endif
//    }
//}
//////////////////////////////////////////////////////////////////////////////////////////////
//using UnityEngine;

//[CreateAssetMenu(menuName = "AttackStrategy/Melee/Normal")]
//public class Melee_Normal : AttackStrategyBaseSO
//{
//    public override void Attack(AttackComponent context, IDamageable target)
//    {
//        if (context == null || context.teamProvider == null)
//            return;

//        bool canHit = false;

//        if (target != null && target.IsAlive())
//        {
//            var mb = target as MonoBehaviour;
//            if (mb != null && mb.gameObject != context.gameObject)
//            {
//                // �Ʊ� ����
//                var provider = mb.GetComponent<ITeamProvider>();
//                bool isEnemy = (provider == null) || (provider.Team != context.teamProvider.Team);

//                // �����ش� disengageRange ����
//                float sqrDist = (mb.transform.position - context.transform.position).sqrMagnitude;
//                bool inDisengage = sqrDist <= (context.disengageRange * context.disengageRange);

//                canHit = isEnemy && inDisengage;
//                if (canHit)
//                {
//                    context.lockedTarget = target;

//                    var core = mb.GetComponent<Core>();
//                    target.RequestDamage(core != null ? context.attackCoreDamage : context.attackDamage);
//                    (target as HealthComponent)?.ApplyImmediateDamage();
//                }
//            }
//        }

//        // ����/������ ������� ����Ʈ�� �� ���� �߽� (Melee_Cone�� ������ Ÿ�̹�)
//        context.EventSender(context.transform);
//    }

//    public override void DrawGizmos(AttackComponent context, EntityData data)
//    {
//#if UNITY_EDITOR
//        if (context == null) return;

//        Vector3 origin = context.transform.position;

//        // ���� ���� �ݰ�(�ִϸ��̼� Ʈ����)
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(origin, data.attackRange);

//        // ������ �ݰ�(disengageRange)
//        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.15f);
//        Gizmos.DrawWireSphere(origin, data.disengageRange);
//#endif
//    }
//}
using UnityEngine;

[CreateAssetMenu(menuName = "AttackStrategy/Melee/Normal")]
public class Melee_Normal : AttackStrategyBaseSO
{
    public override void Attack(AttackComponent context, IDamageable target)
    {
        if (context == null || context.teamProvider == null)
            return;

        MonoBehaviour mb = target as MonoBehaviour;
        Transform aim = context.transform; // �⺻��: �ڱ� �ڽ�(Ÿ�� ���� �� ���)

        if (target != null && target.IsAlive() && mb != null && mb.gameObject != context.gameObject)
        {
            // �Ʊ�/���� ����
            var provider = mb.GetComponent<ITeamProvider>();
            bool isEnemy = provider == null || provider.Team != context.teamProvider.Team;

            // �����ش� disengageRange ����
            float sqrDist = (mb.transform.position - context.transform.position).sqrMagnitude;
            bool inDisengage = sqrDist <= (context.disengageRange * context.disengageRange);

            if (isEnemy && inDisengage)
            {
                context.lockedTarget = target;

                var core = mb.GetComponent<Core>();
                target.RequestDamage(core != null ? context.attackCoreDamage : context.attackDamage);
                (target as HealthComponent)?.ApplyImmediateDamage();
            }

            // Magic_Fireó�� Ÿ�� Transform�� ����Ʈ ���� �������� �ѱ�
            //aim = mb.transform;
            aim = context.firePoint;
        }

        // ����/������ �����ϰ� �� ���� �̺�Ʈ �߽�
        context.EventSender(aim);
    }

    public override void DrawGizmos(AttackComponent context, EntityData data)
    {
#if UNITY_EDITOR
        if (context == null) return;

        Vector3 origin = context.transform.position;

        // ���� ���� �ݰ�(�ִϸ��̼� Ʈ����)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, data.attackRange);

        // ������ �ݰ�(disengageRange)
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.15f);
        Gizmos.DrawWireSphere(origin, data.disengageRange);
#endif
    }
}
