using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
//using static UnityEngine.GraphicsBuffer;
//using static UnityEngine.UI.GridLayoutGroup;

public class AttackComponent : MonoBehaviour, IAttackable, IAttackNotifier
{
    // Ony for Gizmo test
    private EntityData _entityData; // EntityData를 통해 초기화할 수 있도록
    
    private float attackDamage;
    private float attackCoreDamage;
    private float attackImpactRatio;
    private float attackCooldown;
    private float attackAnimLength;
    private float detectionRadius;
    private float attackRange;
    private float disengageRange;
    private float magicRadius;
    [HideInInspector] public Transform firePoint;

    private string projectilePoolName;
    private IDamageable lockedTarget;

    private ITeamProvider teamProvider;
    private AttackType attackType;
    private IOrientable _orientable;
    private LayerMask allUnitMask;
    private LayerMask towerOnlyMask;
    private LayerMask coreOnlyMask;
    private LayerMask targetLayer;

    private Coroutine attackCoroutine;
    private bool isAttackingFlag;
    private bool isDead;

#pragma warning disable 67
    public event Action<bool> OnAttackStateChanged;
    public event Action<Transform> OnAttackEffect;
#pragma warning restore 67

    public Transform LockedTargetTransform
        => (lockedTarget as MonoBehaviour)?.transform;


    private void Awake()
    {
        _orientable = GetComponent<IOrientable>();
        teamProvider = GetComponent<ITeamProvider>();
        if (teamProvider == null)
            Debug.LogError($"{name}에 ITeamProvider(TeamComponent)가 할당되지 않았습니다!");
        var health = GetComponent<HealthComponent>();
        if (health != null)
            health.OnDeath += OnOwnerDeath;
    
    }

    public void Initialize(EntityData data)
    {
        // only for gizmo test
        _entityData = data;
        attackDamage = data.attackDamage;
        attackCoreDamage = data.attackCoreDamage;
        if(data.attackClip !=  null)
            attackAnimLength = data.attackClip.length;
        attackCooldown = attackAnimLength;
        attackImpactRatio = data.attackImpactRatio;
        detectionRadius = data.detectionRadius;
        attackRange = data.attackRange;
        disengageRange = data.disengageRange;
        attackType = data.attackType;
        magicRadius = data.magicRadius;
        isAttackingFlag = false;
        isDead = false;
        firePoint = transform.Find("FirePoint");
        projectilePoolName = data.projectilePoolName;

        allUnitMask = LayerMask.GetMask("Agent", "Tower", "Core");
        towerOnlyMask = LayerMask.GetMask("Tower", "Core");
        coreOnlyMask = LayerMask.GetMask("Core");

        switch (data.attackPriority)
        {
            case EntityData.AttackPriority.AllUnits:
                targetLayer = allUnitMask;
                break;
            case EntityData.AttackPriority.TowersOnly:
                targetLayer = towerOnlyMask;
                break;
            case EntityData.AttackPriority.CoreOnly:
                targetLayer = coreOnlyMask;
                break;
        }
    }

    void Update()
    {
        if (attackCoroutine != null || isDead) return;

        var newTarget = DetectTarget();
        if (newTarget != null)
        {
            bool visible = true;
            switch (attackType)
            {
                case AttackType.Melee:
                case AttackType.Ranged:
                    if (firePoint != null)
                        visible = IsTargetVisible(firePoint, (newTarget as MonoBehaviour).transform);
                    break;
                case AttackType.Magic:
                    visible = true; // 무조건 true
                    break;
            }
            if (visible)
                lockedTarget = newTarget;
            else
                lockedTarget = null;
        }
        if (lockedTarget != null && CanAttack(lockedTarget))
            attackCoroutine = StartCoroutine(AttackRoutine(lockedTarget));

    }

    public IDamageable DetectTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            targetLayer);

        IDamageable best = null;
        float bestDist = float.MaxValue;

        foreach (var col in hits)
        {
            if (col.gameObject == gameObject) continue;

            var dmg = col.GetComponent<IDamageable>();
            if (dmg == null || !dmg.IsAlive()) continue;

            var provider = col.GetComponent<ITeamProvider>();
            if (provider == null || provider.Team == teamProvider.Team)
                continue;
            float dist = Vector3.Distance(transform.position, col.transform.position);

            if (dist < bestDist)
            {
                bestDist = dist;
                best = dmg;
            }
        }
        return best;
    }
    private bool CanAttack(IDamageable target)
    {
        float distance = Vector3.Distance(
            transform.position,
            (target as MonoBehaviour).transform.position);
        return distance <= attackRange;
    }
    private void TryAttack(IDamageable target)
    {
        if (!CanAttack(target)) return;
        switch (attackType)
        {
            case AttackType.Melee:
                AttackMelee(target);
                break;
            case AttackType.Ranged:
                AttackRanged(target);
                break;
            case AttackType.Magic:
                AttackMagic(target);
                break;
            default:
                Debug.LogWarning("지원하지 않는 공격 타입");
                break;
        }
    }
    private IEnumerator AttackRoutine(IDamageable target)
    {
        isAttackingFlag = true;
        OnAttackStateChanged?.Invoke(true);
        float impactDelay = attackAnimLength * attackImpactRatio;
        var myHealth = GetComponent<HealthComponent>();

        while (myHealth != null &&
                   myHealth.IsAlive() &&
                   target.IsAlive() &&
                   Vector3.Distance(transform.position,
                       (target as MonoBehaviour).transform.position) <= attackRange)
        {
            _orientable.LookAtTarget(target);
            yield return new WaitForSeconds(impactDelay);
            TryAttack(target);
            yield return new WaitForSeconds(attackCooldown - impactDelay);
        }

        if (!target.IsAlive() ||
            Vector3.Distance(transform.position,
                (target as MonoBehaviour).transform.position) > disengageRange)
        {
            lockedTarget = null;
        }

        attackCoroutine = null;
        isAttackingFlag = false;
        OnAttackStateChanged?.Invoke(false);
    }


    private void AttackMelee(IDamageable target)
    {
        lockedTarget = target;
        if (lockedTarget == null || !lockedTarget.IsAlive())
            return;

        OnAttackEffect?.Invoke(transform);

        var coreComp = (lockedTarget as MonoBehaviour).GetComponent<Core>();
        target.RequestDamage(coreComp != null ? attackCoreDamage : attackDamage);
        (target as HealthComponent)?.ApplyImmediateDamage();
    }

    private void AttackRanged(IDamageable target)
    {
        lockedTarget = target;
        var pool = ObjectPoolManager.Instance.GetPool(projectilePoolName);
        if (pool == null)
        {
            Debug.LogError($"[AttackComponent] ProjectilePool({projectilePoolName}) is null");
            return;
        }

        var projectileObj = pool.Get(firePoint.position, Quaternion.identity);
        var projectile = projectileObj.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogError("[AttackComponent] Projectile 컴포넌트가 없습니다!");
            return;
        }

        projectile.SetPoolName(projectilePoolName);

        projectile.Initialize(
            owner: this.GetComponent<Entity>(),
            damage: attackDamage,
            coreDamage: attackCoreDamage,
            target: (target as MonoBehaviour).transform,
            poolName: projectilePoolName,
            disengageRange : disengageRange
        );

        if (OnAttackEffect == null)
            Debug.Log("OnAttackEffect is null");
        else
            OnAttackEffect?.Invoke(firePoint);
    }
    private void AttackMagic(IDamageable target)
    {
        if (target == null || !target.IsAlive())
            return;

        Vector3 center = (target as MonoBehaviour).transform.position;
        Collider[] hits = Physics.OverlapSphere(center, magicRadius, targetLayer);

        OnAttackEffect?.Invoke((target as MonoBehaviour).transform);

        foreach (var col in hits)
        {
            var dmg = col.GetComponent<IDamageable>();
            var coreComp = col.GetComponent<Core>();
            if (dmg != null && dmg.IsAlive())
                dmg.RequestDamage(coreComp != null ? attackCoreDamage : attackDamage);
            (dmg as HealthComponent)?.ApplyImmediateDamage();
        }
    }

    public bool IsTargetVisible(Transform fireOrigin, Transform target)
    {
        Vector3 origin = fireOrigin.position;
        Vector3 dest = target.position;
        Vector3 dir = (dest - origin).normalized;
        float dist = Vector3.Distance(origin, dest);

        int raycastMask = LayerMask.GetMask("Agent", "Tower", "Core", "Obstacle", "Structure");

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, raycastMask))
            return hit.transform == target;

        return false;
    }

    public bool IsAttacking()
    {
        return isAttackingFlag;
    }

    private void OnOwnerDeath()
    {
        isDead = true;
        GetComponent<Collider>().enabled = false;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            lockedTarget = null;
            isAttackingFlag = false;
            OnAttackStateChanged?.Invoke(false);
        }
    }

    public bool isMelee => attackType == AttackType.Melee;
    public bool isRanged => attackType == AttackType.Ranged;
    public bool isMagic => attackType == AttackType.Magic;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_entityData == null) return;

        // detectionRadius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _entityData.detectionRadius);

        // attackRange
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _entityData.attackRange);

        // disengageRange
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _entityData.disengageRange);
    }
#endif

}


