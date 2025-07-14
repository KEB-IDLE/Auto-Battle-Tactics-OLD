using System;
using System.Collections;
using UnityEngine;

public class AttackComponent : MonoBehaviour, IAttackable, IAttackNotifier
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

    private GameObject projectilePrefab;
    private float projectileSpeed;
    private float projectileLifeTime;

    private IDamageable lockedTarget;   // 현재 공격 대상
    private ITeamProvider teamProvider; // 팀 정보 제공자
    private AttackType attackType;      // 유닛의 공격 유형

    private LayerMask allUnitMask;
    private LayerMask towerOnlyMask;
    private LayerMask coreOnlyMask;
    private LayerMask targetLayer;

    public event Action<bool> OnAttackStateChanged; // 공격 상태 변경 이벤트
    public event Action<IDamageable> OnAttackPerformed; // 공격 수행 이벤트
    


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
        projectileSpeed = data.projectileSpeed;
        projectileLifeTime = data.projectileLifeTime;
        projectilePrefab = data.projectilePrefab;
        attackType = data.attackType;
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
            lockedTarget = newTarget;
        // 2) 공격 조건 검사
        if (lockedTarget != null && CanAttack(lockedTarget))
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
        if (IsAttacking()) return false;

        float dist = Vector3.Distance(
            transform.position,
            (target as MonoBehaviour).transform.position);
        
        return dist <= attackRange;
    }

    private void TryAttack(IDamageable target)
    {
        if (CanAttack(target)) return;
        lastAttackTime = Time.time;
        if (isMelee) AttackMelee(target);
        else if (isRanged) AttackRanged(target);
        else Debug.LogWarning("지원하지 않는 공격 타입");
    }

    private IEnumerator AttackRoutine(IDamageable target)
    {
        // 이동 정지 이벤트(외부 MoveComponent 구독)
        OnAttackStateChanged?.Invoke(true);

        TryAttack(target);
        // 사거리 이탈 또는 대상 사망 전까지, 자동으로 재공격
        while (target.IsAlive() &&
               Vector3.Distance(transform.position,
                   (target as MonoBehaviour).transform.position) <= attackRange)
        {
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

    // animation cilp 중 실행할 함수
    public void OnAttackHit()
    {
        if (lockedTarget == null || !lockedTarget.IsAlive())
            return;

        // “이 대상이 코어인가?” 체크
        var coreComp = (lockedTarget as MonoBehaviour)
                          .GetComponent<CoreComponent>();
        if (coreComp != null)
            lockedTarget.TakeDamage(attackCoreDamage);
        else
            lockedTarget.TakeDamage(attackDamage);
    }

    private void AttackMelee(IDamageable target)
    {
        lockedTarget = target;
        OnAttackPerformed?.Invoke(target);
    }

    private void AttackRanged(IDamageable target)
    {
        lockedTarget = target;
        //Vector3 spawnPos = firePoint;
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