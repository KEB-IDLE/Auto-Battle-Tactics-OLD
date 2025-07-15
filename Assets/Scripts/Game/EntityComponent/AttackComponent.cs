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
    private bool isAttackingFlag;


    private GameObject projectilePrefab;
    private ProjectileData projectileData;

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
        attackType = data.attackType;
        isAttackingFlag = false;

        if (attackType == AttackType.Melee)
        {
            projectilePrefab = null;
        }
        else
        {
            projectilePrefab = data.projectilePrefab;
            projectileData = data.projectileData;
        }
        
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
                //AttackMagic(target);
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
        // 이동 정지 이벤트(외부 MoveComponent 구독)
        try
        {
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
        finally
        {
            isAttackingFlag = false;
            Debug.Log($"[AttackRoutine END]");
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
        OnAttackPerformed?.Invoke(target);

        Vector3 spawnPos = transform.position + Vector3.up * 1f;
        Vector3 targetPos = (target as MonoBehaviour).transform.position + Vector3.up * 1.2f;
        GameObject projectileObject = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        float fireAngle = 30f;
        Vector3 velocity = CalculateVelocity(spawnPos, targetPos, fireAngle);

        var rb = projectileObject.GetComponent<Rigidbody>();
        rb.linearVelocity = velocity;

        var projectile = projectileObject.GetComponent<Projectile>();

        projectile.Initialize(
            data: projectile.data,
            owner: this.GetComponent<Entity>(),
            damage: attackDamage
        // 필요시 target, 혹은 team 등 추가
        );
        
    }
    // 타겟 위치 계산 함수
    private Vector3 CalculateVelocity(Vector3 startPoint, Vector3 endPoint, float angle)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        float radianAngle = angle * Mathf.Deg2Rad;
        float distance = Vector3.Distance(startPoint, endPoint);
        float yOffset = startPoint.y - endPoint.y;

        float initialVelocity = Mathf.Sqrt(distance * gravity / Mathf.Sin(2 * radianAngle));
        float Vy = initialVelocity * Mathf.Sin(radianAngle);
        float Vz = initialVelocity * Mathf.Cos(radianAngle);

        Vector3 velocity = new Vector3(0, Vy, Vz);

        Vector3 direction = (endPoint - startPoint).normalized;
        float angleBetweenObjects = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        return finalVelocity;
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