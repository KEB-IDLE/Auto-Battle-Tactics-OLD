using UnityEngine;

[CreateAssetMenu(menuName = "AttackStrategy/Magic")]
public class MagicAttackStrategySO : AttackStrategyBaseSO
{
    public override void Attack(AttackComponent context, IDamageable target)
    {
        if (target == null || !target.IsAlive())
            return;

        Vector3 center = (target as MonoBehaviour).transform.position;
        Collider[] hits = Physics.OverlapSphere(center, context.magicRadius, context.targetLayer);

        //context.OnAttackEffect?.Invoke((target as MonoBehaviour).transform);
        context.EventSender((target as MonoBehaviour).transform);

        foreach (var col in hits)
        {
            if (col.gameObject == context.gameObject)
                continue;
            var provider = col.GetComponent<ITeamProvider>();
            if (provider != null && provider.Team == context.teamProvider.Team)
                continue;

            var dmg = col.GetComponent<IDamageable>();
            var core = col.GetComponent<Core>();
            if (dmg != null && dmg.IsAlive())
                dmg.RequestDamage(core != null ? context.attackCoreDamage : context.attackDamage);
            (dmg as HealthComponent)?.ApplyImmediateDamage();
        }
    }
}
