using UnityEngine;

[CreateAssetMenu(menuName = "AttackStrategy/Magic/Normal")]
public class Magic_Normal : AttackStrategyBaseSO
{

    public override float DelayTime => base.DelayTime;
    public override void Attack(AttackComponent context, IDamageable target)
    {
        if (target == null || !target.IsAlive())
            return;
        var mono = target as MonoBehaviour;
        if (mono == null)
        {
            Debug.Log("ERROR");
            return;
        }
        Vector3 center = mono.transform.position;
        Collider[] hits = Physics.OverlapSphere(center, context.magicRadius, context.targetLayer);

        context.EventSender(mono.transform);

        foreach (var col in hits)
        {
            if (col.gameObject == context.gameObject)
                continue;
            var provider = col.GetComponent<ITeamProvider>();
            if (provider != null && provider.Team == context.teamProvider.Team)
                continue;

            var dmg = col.GetComponent<IDamageable>();
            //var core = col.GetComponent<Core>();
            if (dmg != null && dmg.IsAlive())
                dmg.RequestDamage(context.attackDamage);
                //dmg.RequestDamage(core != null ? context.attackCoreDamage : context.attackDamage);
            (dmg as HealthComponent)?.ApplyImmediateDamage();
        }
    }

    public override void DrawGizmos(AttackComponent context, EntityData data)
    {
#if UNITY_EDITOR
        if (context == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(context.transform.position, data.attackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(context.transform.position, data.magicRadius);
#endif
    }
}
