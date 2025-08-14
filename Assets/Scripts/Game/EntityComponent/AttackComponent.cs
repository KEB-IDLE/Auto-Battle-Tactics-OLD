/*

using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
//using static UnityEngine.GraphicsBuffer;
//using static UnityEngine.UI.GridLayoutGroup;

public class AttackComponent : MonoBehaviour, IAttackable, IAttackNotifier
{
    private EntityData _entityData; 
    public float attackDamage;
    public float attackCoreDamage;
    private float attackImpactRatio;
    private float attackCooldown;
    private float attackAnimLength;
    private float detectionRadius;
    private float attackRange;
    private float disengageRange;
    private float magicRadius;
    [HideInInspector] public Transform firePoint;

    private string projectilePoolName;
    public IDamageable lockedTarget;

    private ITeamProvider teamProvider;
    private AttackType attackType;
    private IOrientable _orientable;
    private LayerMask targetLayer;

    private Coroutine attackCoroutine;
    private bool isAttackingFlag;
    private bool isDead;
    private bool isGameEnded = false;

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
        if (data.attackClip != null)
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


        targetLayer = LayerMask.GetMask("Agent", "Tower", "Core");
        switch (data.attackPriority)
        {
            case EntityData.AttackPriority.TowersOnly:
                targetLayer = LayerMask.GetMask("Tower", "Core");
                break;
            case EntityData.AttackPriority.CoreOnly:
                targetLayer = LayerMask.GetMask("Core");
                break;
            default: break;
        }
    }

    void Update()
    {
        if (attackCoroutine != null || isDead || isGameEnded) return;

        var newTarget = DetectTarget();
        if (newTarget != null)
        {
            var mono = newTarget as MonoBehaviour;
            bool visible = (attackType == AttackType.Magic || attackType == AttackType.Melee) ? true :
                (firePoint != null && mono != null && IsTargetVisible(firePoint, (mono.transform)));

            if (visible)
                lockedTarget = newTarget;
            else
                lockedTarget = null;
        }
        if (lockedTarget != null && CanAttack(lockedTarget))
            attackCoroutine = StartCoroutine(AttackRoutine(lockedTarget));

    }

    public void StopAllAction()
    {
        isGameEnded = true;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        isAttackingFlag = false;
        isDead = true;
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
            if (col.gameObject == this.gameObject) continue;

            var dmg = col.GetComponent<IDamageable>();
            if (dmg == null || !dmg.IsAlive()) continue;
            if (dmg is HealthComponent hc && !hc.IsTargetable()) continue;

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
        var mono = target as MonoBehaviour;
        if (mono == null) return false;
        float distance = Vector3.Distance(
            transform.position,
            mono.transform.position);
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
        var targetMono = target as MonoBehaviour;

        while (myHealth != null &&
                   myHealth.IsAlive() &&
                   target != null &&
                   target.IsAlive() &&
                   targetMono != null &&
                   Vector3.Distance(transform.position,
                       (target as MonoBehaviour).transform.position) <= attackRange)
        {
            _orientable.LookAtTarget(target);
            yield return new WaitForSeconds(impactDelay);
            TryAttack(target);
            yield return new WaitForSeconds(attackCooldown - impactDelay);
        }

        if (!target.IsAlive() || target == null || targetMono == null ||
            Vector3.Distance(transform.position,
                targetMono.transform.position) > disengageRange)
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

        Transform targetHitPoint = null;
        var mb = (target as MonoBehaviour);
        if (mb != null)
            targetHitPoint = mb.transform.Find("HitPoint");
        if (targetHitPoint == null)
            targetHitPoint = mb?.transform;


        projectile.Initialize(
            owner: this.GetComponent<Entity>(),
            damage: attackDamage,
            coreDamage: attackCoreDamage,
            targetEntity: (target as MonoBehaviour).transform,
            hitPoint: targetHitPoint,
            poolName: projectilePoolName,
            disengageRange: disengageRange
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
            // ✅ 자기 자신 제외
            if (col.gameObject == this.gameObject)
                continue;
            // ✅ 아군 제외
            var provider = col.GetComponent<ITeamProvider>();
            if (provider != null && provider.Team == teamProvider.Team)
                continue;

            var dmg = col.GetComponent<IDamageable>();
            var core = col.GetComponent<Core>();
            if (dmg != null && dmg.IsAlive())
                dmg.RequestDamage(core != null ? attackCoreDamage : attackDamage);
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

    public bool IsAttacking() => isAttackingFlag;
    public bool isMelee => attackType == AttackType.Melee;
    public bool isRanged => attackType == AttackType.Ranged;
    public bool isMagic => attackType == AttackType.Magic;

#if UNITY_EDITOR
    // Ony for Gizmo test

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


*/

using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

public class AttackComponent : MonoBehaviour, IAttackable, IAttackNotifier
{
    // 기존 필드
    [HideInInspector] public float attackDamage;
    [HideInInspector] public float attackCoreDamage;
    [HideInInspector] public float attackImpactRatio;
    [HideInInspector] public float attackCooldown;
    [HideInInspector] public float attackAnimLength;
    [HideInInspector] public float detectionRadius;
    [HideInInspector] public float attackRange;
    [HideInInspector] public float disengageRange;
    [HideInInspector] public float magicRadius;
    [HideInInspector] public string projectilePoolName;
    [HideInInspector] public ITeamProvider teamProvider;
    [HideInInspector] public LayerMask targetLayer;
    [HideInInspector] public IDamageable lockedTarget;
    [HideInInspector] public IOrientable _orientable;
    [HideInInspector] public Transform firePoint;

    private AttackType attackType;
    private Coroutine attackCoroutine;
    private bool isAttackingFlag;
    private bool isDead;
    private bool isGameEnded = false;

    // SO 전략 필드
    private AttackStrategyBaseSO attackStrategy;

#pragma warning disable 67
    public event Action<bool> OnAttackStateChanged;
    public event Action<Transform> OnAttackEffect;
#pragma warning restore 67

    public void Initialize(EntityData data)
    {
        attackDamage = data.attackDamage;
        attackCoreDamage = data.attackCoreDamage;
        if (data.attackClip != null)
            attackAnimLength = data.attackClip.length;
        attackCooldown = attackAnimLength;
        attackImpactRatio = data.attackImpactRatio;
        detectionRadius = data.detectionRadius;
        attackRange = data.attackRange;
        attackType = data.attackType;

        disengageRange = data.disengageRange;
        magicRadius = data.magicRadius;
        isAttackingFlag = false;
        isDead = false;
        firePoint = transform.Find("FirePoint");
        projectilePoolName = data.projectilePoolName;

        teamProvider = GetComponent<ITeamProvider>();
        _orientable = GetComponent<IOrientable>();

        targetLayer = LayerMask.GetMask("Agent", "Tower", "Core");
        switch (data.attackPriority)
        {
            case EntityData.AttackPriority.TowersOnly:
                targetLayer = LayerMask.GetMask("Tower", "Core");
                break;
            case EntityData.AttackPriority.CoreOnly:
                targetLayer = LayerMask.GetMask("Core");
                break;
            default: break;
        }

        attackStrategy = data.attackStrategy;
    }

    void Update()
    {
        if (attackCoroutine != null || isDead || isGameEnded) return;

        var newTarget = DetectTarget();
        if (newTarget != null)
        {
            var mono = newTarget as MonoBehaviour;
            bool visible = true;
            if (attackType == AttackType.Ranged && firePoint != null && mono != null)
                visible = IsTargetVisible(firePoint, mono.transform);

            lockedTarget = visible ? newTarget : null;
        }
        if (lockedTarget != null && CanAttack(lockedTarget))
            attackCoroutine = StartCoroutine(AttackRoutine(lockedTarget));
    }

    public IDamageable DetectTarget()
    {
        var hits = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            targetLayer);

        IDamageable best = null;
        float bestDist = float.MaxValue;
        Vector3 origin = (firePoint != null ? firePoint.position : transform.position);

        foreach (var col in hits)
        {
            if (col.gameObject == this.gameObject) continue;
            var dmg = col.GetComponent<IDamageable>();
            if (dmg == null || !dmg.IsAlive()) continue;
            if (dmg is HealthComponent hc && !hc.IsTargetable()) continue;

            var provider = col.GetComponent<ITeamProvider>();
            if (provider == null || provider.Team == teamProvider.Team) continue;

            Vector3 closest = (col != null) ? col.ClosestPoint(origin) : col.transform.position;
            float dist = Vector3.Distance(origin, closest);

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
        var mono = target as MonoBehaviour;
        if (mono == null) return false;

        Vector3 origin = (firePoint != null? firePoint.position : transform.position);
        var collider = mono.GetComponent<Collider>();
        Vector3 closest = (collider != null ? collider.ClosestPoint(origin) : mono.transform.position);

        float distance = Vector3.Distance(origin, closest);
        return distance <= attackRange;
    }
    private void TryAttack(IDamageable target)
    {
        if (!CanAttack(target)) return;
        attackStrategy?.Attack(this, target);
    }

    private IEnumerator AttackRoutine(IDamageable target)
    {
        isAttackingFlag = true;
        OnAttackStateChanged?.Invoke(true);
        float impactDelay = attackAnimLength * attackImpactRatio;
        var myHealth = GetComponent<HealthComponent>();
        var targetMono = target as MonoBehaviour;

        while (myHealth != null &&
                   myHealth.IsAlive() &&
                   target != null &&
                   target.IsAlive() &&
                   targetMono != null &&
                   SurfaceDistanceTo(target) <= attackRange)
        {
            _orientable?.LookAtTarget(target);
            yield return new WaitForSeconds(impactDelay);
            TryAttack(target);
            yield return new WaitForSeconds(attackCooldown - impactDelay);
        }

        if (!target.IsAlive() || target == null || targetMono == null ||
            SurfaceDistanceTo(target) > disengageRange)
        {
            lockedTarget = null;
        }

        attackCoroutine = null;
        isAttackingFlag = false;
        OnAttackStateChanged?.Invoke(false);
    }


    public void ExecuteStrategyCoroutine(IEnumerator routine) => StartCoroutine(routine);
    public bool IsTargetVisible(Transform fireOrigin, Transform target)
    {
        Vector3 origin = fireOrigin.position;
        Vector3 dest = target.position;
        Vector3 dir = (dest - origin).normalized;
        float dist = Vector3.Distance(origin, dest);

        int raycastMask = LayerMask.GetMask("Agent", "Tower", "Core", "Obstacle", "Structure");

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, raycastMask))
        {
            var ht = hit.transform;
            if (ht == target || ht.IsChildOf(target) || target.IsChildOf(ht))
            {
                Debug.Log("true");
                return true;
            }

        }

        Debug.Log("false");
        return false;
    }

    private float SurfaceDistanceTo(IDamageable target)
    {
        var tMono = target as MonoBehaviour;
        if (tMono == null) return float.MaxValue;

        Vector3 origin = (firePoint != null ? firePoint.position : transform.position);
        var col = tMono.GetComponent<Collider>();
        Vector3 closest = (col != null ? col.ClosestPoint(origin) : tMono.transform.position);
        return Vector3.Distance(origin, closest);
    }

    public void StopAllAction()
    {
        isGameEnded = true;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        isAttackingFlag = false;
        isDead = true;
    }
    public void EventSender(Transform t) => OnAttackEffect?.Invoke(t);
    public Transform LockedTargetTransform => (lockedTarget as MonoBehaviour)?.transform;
    public bool IsAttacking() => isAttackingFlag;
    public bool isMelee => attackType == AttackType.Melee;
    public bool isRanged => attackType == AttackType.Ranged;
    public bool isMagic => attackType == AttackType.Magic;



    private void OnDrawGizmosSelected()
    {
        EntityData data = GetComponent<Entity>().entityData;
#if UNITY_EDITOR

        if (data == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRadius);

        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, data.attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, data.disengageRange);

        data.attackStrategy?.DrawGizmos(this, data);

#endif
    }
}
