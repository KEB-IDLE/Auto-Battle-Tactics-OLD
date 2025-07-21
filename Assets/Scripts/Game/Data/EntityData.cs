using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
    [Header("Entity Information")]

    [Tooltip("�̸� ����")]
    public string entityName;
    [Tooltip("�ִ� ü�� ����")]
    public float maxHP;
    [Tooltip("���� ��� ���ݷ�")]
    public float attackDamage;
    [Tooltip("�ھ� ��� ���ݷ�")]
    public float attackCoreDamage;
    [Tooltip("����ݱ��� ���ð�")]
    public float attackCooldown;
    [Tooltip("�̵� �ӵ�")]
    public float moveSpeed;
    [Tooltip("��ü ũ��")]
    public EntityScale entityScale;
    [Header("Layer ����")]
    [Tooltip("7: Human, 8:Mutant 9: Tower, 10: Core, 11: Projectile, 12: Structure, 13: Obstacle")]
    public int Layer;

    [Header("Target Detection")]
    [Tooltip("���� ���� ����")]
    public float detectionRadius;
    [Tooltip("���� ���� ���� ����")]
    public float attackRange;
    [Tooltip("Ÿ���� �����ϴ� �Ӱ� �Ÿ�")]
    public float disengageRange;
    public enum AttackPriority
    {
        AllUnits,    // Agent, Tower, Core
        TowersOnly,  // Tower, Core
        CoreOnly     // Core
    }
    [Tooltip("���� ����� Agent, Tower, Core �� � Ÿ������ ����")]
    public AttackPriority attackPriority; // ���� �켱����
    [Tooltip("���� ���� ���� ���� üũ")]
    public AttackableType attackableType; // ���� ���� Ÿ��

    [Tooltip("������ ���� ��� (�ٰŸ� = Melee, ���Ÿ� = Ranged etc..)")]
    public AttackType attackType;

    [Header("Entity Prefab")]
    public GameObject entityPrefab; // ��ƼƼ ������

    [Header("Projectile Info")]
    [Tooltip("�߻�ü ������")]
    public GameObject projectilePrefab;
    [Tooltip("������Ʈ Ǯ�� �ڵ����� �����մϴ�.")]
    public string projectilePoolName;

    [Header("Effect Prefab")]
    [Tooltip("��ȯ ����Ʈ")]
    public GameObject summonEffectPrefab; // ��ȯ ����Ʈ ������
    [Tooltip("���� ����Ʈ")]
    public GameObject attackEffectPrefab; // ���� ����Ʈ ������
    [Tooltip("�ǰ� ����Ʈ")]
    public GameObject takeDamageEffectPrefeb; // ���� ����Ʈ ������
    [Tooltip("���� ����Ʈ")]
    public GameObject deathEffectPrefab; // ���� ����Ʈ ������

    [Tooltip("�߻�ü �̵� ����Ʈ")]
    public GameObject projectileAttackingEffectPrefab;

    [Header("Audio Settings")]
    public AudioClip summonSound; // ��ȯ ����
    public AudioClip attackSound; // ���� ����
    public AudioClip deathSound;  // ���� ����

    [Header("Animation Settings")]
    [Tooltip("�� ��ƼƼ ���� AnimatorController (�⺻ ��Ʈ�ѷ��� Override �ϴ� �뵵)")]
    public RuntimeAnimatorController animatorController;
    [Tooltip("���� �ִϸ��̼� Ŭ��")]
    public AnimationClip attackClip;
    [Tooltip("�̵� �ִϸ��̼� Ŭ��")]
    public AnimationClip runClip;
    [Tooltip("���� �ִϸ��̼� Ŭ��")]
    public AnimationClip deathClip;
    [Tooltip("��� �ִϸ��̼� Ŭ��")]
    public AnimationClip idleClip;

}
public enum AttackableType
{
    GroundOnly,
    Both
}
public enum EntityScale
{
    Small,
    Medium,
    Large
}
public enum AttackType
{
    Melee,
    Ranged,
    Magic
}