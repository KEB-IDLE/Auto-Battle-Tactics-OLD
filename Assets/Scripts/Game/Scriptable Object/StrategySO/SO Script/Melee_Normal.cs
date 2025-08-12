using UnityEngine;

[CreateAssetMenu(menuName = "AttackStrategy/Melee/Normal")]
public class Melee_Normal : AttackStrategyBaseSO
{
    public override void Attack(AttackComponent context, IDamageable target)
    {
        if (target == null || !target.IsAlive())
            return;

        context.lockedTarget = target;
        context.EventSender(context.transform);

        var coreComp = (target as MonoBehaviour).GetComponent<Core>();
        target.RequestDamage(coreComp != null ? context.attackCoreDamage : context.attackDamage);
        (target as HealthComponent)?.ApplyImmediateDamage();
    }

    public override void DrawGizmos(AttackComponent context, EntityData data)
    {
#if UNITY_EDITOR
        if (context == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(context.transform.position, data.attackRange);
#endif
    }
}
