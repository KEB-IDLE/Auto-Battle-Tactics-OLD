using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class AttackComponent : MonoBehaviour, IAttackable
{       
    private float attackDamage;         // �����
    private float attackCoreDamage; // �ھ� ���� �����

    private float attackCooldown;       // ����ݱ��� ���ð�
    private float lastAttackTime;       // ������ ���� �ð�
    private float detectionRadius;      // ���� ��� ���� ����
    private float attackRange; // ���� ����
    private float disengageRange; // ���� ������ �Ÿ��� �� ������ ����� ���� ����

    private IDamageable lockedTarget;   // ���� ���� ���
    private ITeamProvider teamProvider; // �� ���� ������

    private LayerMask allUnitMask;
    private LayerMask towerOnlyMask;
    private LayerMask coreOnlyMask;
    private LayerMask targetLayer;

    public event Action<bool> OnAttackStateChanged; // ���� ���� ���� �̺�Ʈ
        //private Transform firePoint;     // ����ü�� ����Ʈ ���� ��ġ

    public Transform LockedTargetTransform
        => (lockedTarget as MonoBehaviour)?.transform;


    private void Awake()
    {
        teamProvider = GetComponent<ITeamProvider>();
        Debug.Log($"[{name}] AttackComponent.Awake �� �� ��: {teamProvider?.Team}");
        if (teamProvider == null)
        {
            Debug.LogError($"{name}�� ITeamProvider(TeamComponent)�� �Ҵ���� �ʾҽ��ϴ�!");
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

        // 2) ���� ���� �˻�
        if (lockedTarget != null && CanAttack(lockedTarget))
        {
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // 3) ���� ���� (�ڷ�ƾ���ε� ��ȯ ����)
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
        // �ִϸ��̼� Ʈ���� ���� �ΰ� ���� �߰� ����
        target.TakeDamage(attackDamage);
        lastAttackTime = Time.time;
    }
    private IEnumerator AttackRoutine(IDamageable target)
    {
        // �̵� ���� �̺�Ʈ(�ܺ� MoveComponent ����)
        OnAttackStateChanged?.Invoke(true);

        // �� �� ����
        Attack(target);

        // ��Ÿ� ��Ż �Ǵ� ��� ��� ������, �ڵ����� �����
        while (target.IsAlive() &&
               Vector3.Distance(transform.position,
                   (target as MonoBehaviour).transform.position) <= attackRange)
        {
            yield return new WaitForSeconds(attackCooldown);
            Attack(target);
        }

        // ���� ���� �̺�Ʈ
        OnAttackStateChanged?.Invoke(false);

        // ���� ����� Ÿ�� �ʱ�ȭ
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