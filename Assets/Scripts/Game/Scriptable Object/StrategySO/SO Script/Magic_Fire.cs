using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Magic_Fire", menuName = "AttackStrategy/Magic/Fire")]
public class Magic_Fire : AttackStrategyBaseSO
{
    public override float DelayTime => 0.6f;

    public override void Attack(AttackComponent context, IDamageable target)
    {
        context.ExecuteStrategyCoroutine(AttackAndDelay(context, target));
    }

    private IEnumerator AttackAndDelay(AttackComponent context, IDamageable target)
    {
        context.EventSender((target as MonoBehaviour).transform);
        yield return new WaitForSeconds(DelayTime);

        if (target == null || !target.IsAlive())
            yield break;

        var mono = target as MonoBehaviour;
        if (mono == null)
            yield break;

        Vector3 center = mono.transform.position;
        Collider[] hits = Physics.OverlapSphere(center, context.magicRadius, context.targetLayer);

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
