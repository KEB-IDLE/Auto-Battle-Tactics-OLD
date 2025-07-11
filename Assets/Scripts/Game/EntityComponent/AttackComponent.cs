using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class AttackComponent : MonoBehaviour, IAttackable
{       
    private float attackDamage;         // 대미지
    private float attackCoreDamage; // 코어 공격 대미지

    private float attackCooldown;       // 재공격까지 대기시간
    private float lastAttackTime;       // 마지막 공격 시간
    private float detectionRadius;      // 공격 대상 인지 범위
    private float attackRange; // 공격 범위
    private float disengageRange; // 공격 대상과의 거리가 이 범위를 벗어나면 공격 중지

    private IDamageable lockedTarget;   // 현재 공격 대상
    private ITeamProvider teamProvider; // 팀 정보 제공자

    private LayerMask allUnitMask;
    private LayerMask towerOnlyMask;
    private LayerMask coreOnlyMask;
    private LayerMask targetLayer;

    public event Action<bool> OnAttackStateChanged; // 공격 상태 변경 이벤트
        //private Transform firePoint;     // 투사체나 이펙트 시작 위치

    public Transform LockedTargetTransform
        => (lockedTarget as MonoBehaviour)?.transform;


    private void Awake()
    {
        teamProvider = GetComponent<ITeamProvider>();
        Debug.Log($"[{name}] AttackComponent.Awake → 내 팀: {teamProvider?.Team}");
        if (teamProvider == null)
        {
            Debug.LogError($"{name}에 ITeamProvider(TeamComponent)가 할당되지 않았습니다!");
        }
    }

    public void Initialize(EntityData data)
    {

        attackDamage = data.attackDamage;
        attackCoreDamage = data.attackCoreDamage;
        attackCooldown = data.attackCooldown;
        detectionRadius = data.detectionRadius;
        attackRange = data.attackRange;
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
            lockedTarget = newTarget;
        }

        // 2) 공격 조건 검사
        if (lockedTarget != null && CanAttack(lockedTarget))
        {
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // 3) 공격 실행 (코루틴으로도 전환 가능)
            StartCoroutine(AttackRoutine(lockedTarget));
        }
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
            {
                continue;
            }
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
        if (Time.time < lastAttackTime + attackCooldown)
            return false;

        float dist = Vector3.Distance(
            transform.position,
            (target as MonoBehaviour).transform.position);

        return dist <= attackRange;
    }

    private void Attack(IDamageable target)
    {
        // 애니메이션 트리거 같은 부가 로직 추가 가능
        target.TakeDamage(attackDamage);
        lastAttackTime = Time.time;
    }
    private IEnumerator AttackRoutine(IDamageable target)
    {
        // 이동 정지 이벤트(외부 MoveComponent 구독)
        OnAttackStateChanged?.Invoke(true);

        // 한 번 공격
        Attack(target);

        // 사거리 이탈 또는 대상 사망 전까지, 자동으로 재공격
        while (target.IsAlive() &&
               Vector3.Distance(transform.position,
                   (target as MonoBehaviour).transform.position) <= attackRange)
        {
            yield return new WaitForSeconds(attackCooldown);
            Attack(target);
        }

        // 공격 종료 이벤트
        OnAttackStateChanged?.Invoke(false);

        // 범위 벗어나면 타겟 초기화
        if (!target.IsAlive() ||
            Vector3.Distance(transform.position,
                (target as MonoBehaviour).transform.position) > detectionRadius)
        {
            lockedTarget = null;
        }
    }
    public bool IsAttacking()
        => Time.time < lastAttackTime + attackCooldown;

}