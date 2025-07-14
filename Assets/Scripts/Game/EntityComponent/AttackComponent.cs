using System;
using System.Collections;
using UnityEngine;

public class AttackComponent : MonoBehaviour, IAttackable, IAttackNotifier
{
    // Ony for Gizmo test
    private EntityData _entityData; // EntityData�� ���� �ʱ�ȭ�� �� �ֵ���
    
    private float attackDamage;         // �����
    private float attackCoreDamage;     // �ھ� ���� �����

    private float attackCooldown;       // ����ݱ��� ���ð�
    private float lastAttackTime;       // ������ ���� �ð�
    private float detectionRadius;      // ���� ��� ���� ����
    private float attackRange;          // ���� ����
    private float disengageRange;       // ���� ������ �Ÿ��� �� ������ ����� ���� ����

    private GameObject projectilePrefab;
    private float projectileSpeed;
    private float projectileLifeTime;

    private IDamageable lockedTarget;   // ���� ���� ���
    private ITeamProvider teamProvider; // �� ���� ������
    private AttackType attackType;      // ������ ���� ����

    private LayerMask allUnitMask;
    private LayerMask towerOnlyMask;
    private LayerMask coreOnlyMask;
    private LayerMask targetLayer;

    public event Action<bool> OnAttackStateChanged; // ���� ���� ���� �̺�Ʈ
    public event Action<IDamageable> OnAttackPerformed; // ���� ���� �̺�Ʈ
    


    public Transform LockedTargetTransform
        => (lockedTarget as MonoBehaviour)?.transform;


    private void Awake()
    {
        teamProvider = GetComponent<ITeamProvider>();
        if (teamProvider == null)
        {
            Debug.LogError($"{name}�� ITeamProvider(TeamComponent)�� �Ҵ���� �ʾҽ��ϴ�!");
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
        // 2) ���� ���� �˻�
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
        else Debug.LogWarning("�������� �ʴ� ���� Ÿ��");
    }

    private IEnumerator AttackRoutine(IDamageable target)
    {
        // �̵� ���� �̺�Ʈ(�ܺ� MoveComponent ����)
        OnAttackStateChanged?.Invoke(true);

        TryAttack(target);
        // ��Ÿ� ��Ż �Ǵ� ��� ��� ������, �ڵ����� �����
        while (target.IsAlive() &&
               Vector3.Distance(transform.position,
                   (target as MonoBehaviour).transform.position) <= attackRange)
        {
            yield return new WaitForSeconds(attackCooldown);
            TryAttack(target);
        }

        // ���� ���� �̺�Ʈ
        OnAttackStateChanged?.Invoke(false);

        // ���� ����� Ÿ�� �ʱ�ȭ
        if (!target.IsAlive() ||
            Vector3.Distance(transform.position,
                (target as MonoBehaviour).transform.position) > disengageRange)
        {
            lockedTarget = null;
        }
    }

    // animation cilp �� ������ �Լ�
    public void OnAttackHit()
    {
        if (lockedTarget == null || !lockedTarget.IsAlive())
            return;

        // ���� ����� �ھ��ΰ�?�� üũ
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

        // ���� �ݰ�(detectionRadius)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _entityData.detectionRadius);

        // ���� �ݰ�(attackRange)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _entityData.attackRange);

        // ���� �ݰ�(disengageRange)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _entityData.disengageRange);
    }
#endif

}