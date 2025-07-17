using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

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
    private bool isAttackingFlag;
    public Transform firePoint;


    private GameObject projectilePrefab;
    private ProjectileData projectileData;

    private IDamageable lockedTarget;   // ���� ���� ���
    private IOrientable orientable;
    private ITeamProvider teamProvider; // �� ���� ������
    private AttackType attackType;      // ������ ���� ����

    private LayerMask allUnitMask;
    private LayerMask towerOnlyMask;
    private LayerMask coreOnlyMask;
    private LayerMask targetLayer;

    public event Action<bool> OnAttackStateChanged; // ���� ���� ���� �̺�Ʈ

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
        attackType = data.attackType;
        isAttackingFlag = false;
        firePoint = transform.Find("FirePoint");

        if (attackType == AttackType.Melee)
        {
            projectilePrefab = null;
        }
        else
        {
            projectilePrefab = data.projectilePrefab;
            //projectileData = data.projectileData;
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
        {
            bool visible = true;
            if (firePoint != null)
                visible = IsTargetVisible(firePoint, (newTarget as MonoBehaviour).transform);
            if (visible)
                lockedTarget = newTarget;
            else
                lockedTarget = null;
        }
        // 2) ���� ���� �˻�
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
                Debug.LogWarning("�������� �ʴ� ���� Ÿ��");
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
            // ��Ÿ� ��Ż �Ǵ� ��� ��� ������, �ڵ����� �����
            while (target.IsAlive() &&
                   Vector3.Distance(transform.position,
                       (target as MonoBehaviour).transform.position) <= attackRange)
            {
                orientable?.LookAtTarget((target as MonoBehaviour).transform.position);
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
        finally
        {
            isAttackingFlag = false;
            Debug.Log($"[AttackRoutine END]");
        }
    }

    // animation cilp �� ������ �޼���
    // ���� ���� ��

    private void AttackMelee(IDamageable target)
    {
        lockedTarget = target;

        if (lockedTarget == null || !lockedTarget.IsAlive())
            return;

        // ���� ����� �ھ��ΰ�?�� üũ
        var coreComp = (lockedTarget as MonoBehaviour)
                          .GetComponent<Core>();
        if (coreComp != null)
            lockedTarget.TakeDamage(attackCoreDamage);
        else
            lockedTarget.TakeDamage(attackDamage);
    }

    private void AttackRanged(IDamageable target)
    {

        lockedTarget = target;
        var projGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        var projectile = projGO.GetComponent<Projectile>();
        projectile.Initialize(
        owner: this.GetComponent<Entity>(),             // ����ü ������
        damage: attackDamage,                           // �����
        coreDamage: attackCoreDamage,                   // �ھ� ���ݷ� 
        target: (target as MonoBehaviour).transform);   // ��ǥ Transform ����
    }

    private void AttackMagic(IDamageable target)
    {
        lockedTarget = target;

        // ��� ����!
        if (lockedTarget != null && lockedTarget.IsAlive())
        {
            var coreComp = (lockedTarget as MonoBehaviour)
                                .GetComponent<Core>();
            if (coreComp != null)
                lockedTarget.TakeDamage(attackCoreDamage);
            else
                lockedTarget.TakeDamage(attackDamage);

            // �߰�: ���� ȿ��, ��ƼŬ, ���� �� ���⼭ Instantiate
        }
    }

    private bool IsTargetVisible(Transform fireOrigin, Transform target)
    {
        Vector3 origin = fireOrigin.position;
        Vector3 dest = target.position;
        Vector3 dir = (dest - origin).normalized;
        float dist = Vector3.Distance(origin, dest);

        // "Obstacle" �� ��ֹ� ���̾� ����!
        int raycastMask = LayerMask.GetMask("Agent", "Tower", "Core", "Obstacle", "Structure");

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, raycastMask))
        {
            // Ray�� Ÿ���� ��Ȯ�� ������ ���� ���� ����
            return hit.transform == target;
        }
        // �ƹ��͵� �� ������ �þ� ����(������)
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