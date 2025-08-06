using UnityEngine;

[CreateAssetMenu(menuName = "AttackStrategy/Ranged")]
public class RangedAttackStrategySO : AttackStrategyBaseSO
{
    public override void Attack(AttackComponent context, IDamageable target)
    {
        if (target == null || !target.IsAlive())
            return;

        context.lockedTarget = target;
        var pool = ObjectPoolManager.Instance.GetPool(context.projectilePoolName);
        if (pool == null)
        {
            Debug.LogError($"[AttackComponent] ProjectilePool({context.projectilePoolName}) is null");
            return;
        }

        var projectileObj = pool.Get(context.firePoint.position, Quaternion.identity);
        var projectile = projectileObj.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogError("[AttackComponent] Projectile ������Ʈ�� �����ϴ�!");
            return;
        }

        projectile.SetPoolName(context.projectilePoolName);

        Transform targetHitPoint = null;
        var mb = (target as MonoBehaviour);
        if (mb != null)
            targetHitPoint = mb.transform.Find("HitPoint");
        if (targetHitPoint == null)
            targetHitPoint = mb?.transform;

        projectile.Initialize(
            owner: context.GetComponent<Entity>(),
            damage: context.attackDamage,
            coreDamage: context.attackCoreDamage,
            targetEntity: (target as MonoBehaviour).transform,
            hitPoint: targetHitPoint,
            poolName: context.projectilePoolName,
            disengageRange: context.disengageRange
        );

        //context.OnAttackEffect?.Invoke(context.firePoint);
        context.EventSender(context.transform);
    }
}
