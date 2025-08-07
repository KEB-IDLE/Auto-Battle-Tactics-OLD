using UnityEngine;

[CreateAssetMenu(menuName = "AttackStrategy/Melee")]
public class MeleeAttackStrategySO : AttackStrategyBaseSO
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
}
