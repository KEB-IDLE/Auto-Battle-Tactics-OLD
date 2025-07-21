using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
//using static UnityEngine.GraphicsBuffer;
//using static UnityEngine.UI.GridLayoutGroup;

public class AttackComponent : MonoBehaviour, IAttackable, IAttackNotifier, IEffectNotifier
{
    // Ony for Gizmo test
    private EntityData _entityData; // EntityData를 통해 초기화할 수 있도록
    
    private float attackDamage;         // 대미지
    private float attackCoreDamage;     // 코어 공격 대미지
    private float attackCooldown;       // 재공격까지 대기시간
    private float lastAttackTime;       // 마지막 공격 시간
    private float detectionRadius;      // 공격 대상 인지 범위
    private float attackRange;          // 공격 범위
    private float disengageRange;       // 공격 대상과의 거리가 이 범위를 벗어나면 공격 중지
    private bool isAttackingFlag;
    public Transform firePoint;


    private GameObject projectilePrefab;
    private string projectilePoolName;

    private IDamageable lockedTarget;   // 현재 공격 대상
    private IOrientable orientable;
    private ITeamProvider teamProvider; // 팀 정보 제공자
    private AttackType attackType;      // 유닛의 공격 유형

    private LayerMask allUnitMask;
    private LayerMask towerOnlyMask;
    private LayerMask coreOnlyMask;
    private LayerMask targetLayer;
    


#pragma warning disable 67
    public event Action<bool> OnAttackStateChanged;
    public event Action<Transform> OnAttackEffect;
    public event Action<Transform> OnTakeDamageEffect;
    public event Action<Transform> OnDeathEffect;
#pragma warning restore 67

    public Transform LockedTargetTransform
        => (lockedTarget as MonoBehaviour)?.transform;


    private void Awake()
    {
        teamProvider = GetComponent<ITeamProvider>();
        if (teamProvider == null)
        {
            Debug.LogError($"{name}에 ITeamProvider(TeamComponent)가 할당되지 않았습니다!");
        }
    }

    public void Initialize(EntityData data)
    {
        // only for gizmo test
        _entityData = data;
        attackDamage = data.attackDamage;
        attackCoreDamage = data.attackCoreDamage;
        attackCooldown = data.attackCooldown;
        detectionRadius = data.detectionRadius;
        attackRange = data.attackRange;
        disengageRange = data.disengageRange;
        attackType = data.attackType;
        isAttackingFlag = false;
        firePoint = transform.Find("FirePoint");
        projectilePoolName = data.projectilePoolName;

        if (attackType == AttackType.Melee)
            projectilePrefab = null;
        else
            projectilePrefab = data.projectilePrefab;

        
        lastAttackTime = 0f;

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
        var newTarget = DetectTarget();

        if (newTarget != null)
        {
            bool visible = true;
            if (firePoint != null)
                visible = IsTargetVisible(firePoint, (newTarget as MonoBehaviour).transform);
            if (visible)
                lockedTarget = newTarget;
            else
                lockedTarget = null;
        }
        // 2) 공격 조건 검사
        if (lockedTarget != null && CanAttack(lockedTarget) && !isAttackingFlag)
                StartCoroutine(AttackRoutine(lockedTarget));
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
        lastAttackTime = Time.time;
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
        try
        {
            TryAttack(target);
            // 사거리 이탈 또는 대상 사망 전까지, 자동으로 재공격
            while (target.IsAlive() &&
                   Vector3.Distance(transform.position,
                       (target as MonoBehaviour).transform.position) <= attackRange)
            {
                orientable?.LookAtTarget((target as MonoBehaviour).transform.position);
                yield return new WaitForSeconds(attackCooldown);
                TryAttack(target);
            }

            // 공격 종료 이벤트
            OnAttackStateChanged?.Invoke(false);

            // 범위 벗어나면 타겟 초기화
            if (!target.IsAlive() ||
                Vector3.Distance(transform.position,
                    (target as MonoBehaviour).transform.position) > disengageRange)
            {
                lockedTarget = null;
            }
        } 
        finally
        {
            isAttackingFlag = false;
            Debug.Log($"[AttackRoutine END]");
        }
    }

    // animation cilp 중 실행할 메서드
    // 근접 공격 시

    private void AttackMelee(IDamageable target)
    {
        lockedTarget = target;
        if (lockedTarget == null || !lockedTarget.IsAlive())
            return;
        var coreComp = (lockedTarget as MonoBehaviour)
                          .GetComponent<Core>();

        //여기에 타격 이펙트 추가
        OnAttackEffect?.Invoke(firePoint);

        if (coreComp != null)
            lockedTarget.TakeDamage(attackCoreDamage);
        else
            lockedTarget.TakeDamage(attackDamage);
    }

    private void AttackRanged(IDamageable target)
    {
        lockedTarget = target;
        var pool = ProjectilePoolManager.Instance.GetPool(projectilePoolName);
        if(pool == null)
        {
            Debug.LogError("[AttackComponent] ProjectilePool을 찾지 못했습니다!");
            return;
        }

        var projectile = pool.GetProjectile();
        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = Quaternion.identity;
        projectile.SetPool(pool);
        projectile.Initialize(
            owner: this.GetComponent<Entity>(),
            damage: attackDamage,
            coreDamage: attackCoreDamage,
            target: (target as MonoBehaviour).transform
            );
    }

    private void AttackMagic(IDamageable target)
    {
        lockedTarget = target;

        // 즉시 피해!
        if (lockedTarget != null && lockedTarget.IsAlive())
        {
            var coreComp = (lockedTarget as MonoBehaviour)
                                .GetComponent<Core>();
            if (coreComp != null)
                lockedTarget.TakeDamage(attackCoreDamage);
            else
                lockedTarget.TakeDamage(attackDamage);

            // 추가: 연출 효과, 파티클, 사운드 등 여기서 Instantiate
        }
    }

    private bool IsTargetVisible(Transform fireOrigin, Transform target)
    {
        Vector3 origin = fireOrigin.position;
        Vector3 dest = target.position;
        Vector3 dir = (dest - origin).normalized;
        float dist = Vector3.Distance(origin, dest);

        // "Obstacle" 등 장애물 레이어 포함!
        int raycastMask = LayerMask.GetMask("Agent", "Tower", "Core", "Obstacle", "Structure");

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, raycastMask))
        {
            // Ray가 타겟을 정확히 맞췄을 때만 공격 가능
            return hit.transform == target;
        }
        // 아무것도 안 맞으면 시야 막힘(비정상)
        return false;
    }


    public bool IsAttacking()
        => Time.time < lastAttackTime + attackCooldown;
    public bool isMelee => attackType == AttackType.Melee;
    public bool isRanged => attackType == AttackType.Ranged;
    public bool isMagic => attackType == AttackType.Magic;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_entityData == null) return;

        // 감지 반경(detectionRadius)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _entityData.detectionRadius);

        // 공격 반경(attackRange)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _entityData.attackRange);

        // 해제 반경(disengageRange)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _entityData.disengageRange);
    }
#endif

}