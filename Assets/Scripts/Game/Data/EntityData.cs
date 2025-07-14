using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
    [Header("Entity Information")]
    //public string entityName;   
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

    [Header("Target Detection")]
    [Tooltip("���� ���� ����")]
    public float detectionRadius;   // ���� ��� ���� ����
    [Tooltip("���� ���� ���� ����")]
    public float attackRange;       // ���� ���� ����
    [Tooltip("Ÿ���� �����ϴ� �Ӱ� �Ÿ�")]
    public float disengageRange;    // ��׷� Ǫ�� �Ѱ� �Ÿ�
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

    [Header("Projectile Info")]
    [Tooltip("�߻�ü ������")]
    public GameObject projectilePrefab;
    [Tooltip("�߻�ü �ӵ�")]
    public float projectileSpeed;
    [Tooltip("�߻�ü ���� �ð�")]
    public float projectileLifeTime;

    [Header("Entity Prefab")]
    public GameObject entityPrefab; // ��ƼƼ ������

    [Tooltip("��ȯ ����Ʈ")]
    public GameObject summonEffectPrefab; // ��ȯ ����Ʈ ������
    [Tooltip("���� ����Ʈ")]
    public GameObject attackEffectPrefab; // ���� ����Ʈ ������
    [Tooltip("���� ����Ʈ")]
    public GameObject deathEffectPrefab; // ���� ����Ʈ ������

    [Header("Audio Settings")]
    public AudioBehaviour summonSound; // ��ȯ ����
    public AudioBehaviour attackSound; // ���� ����
    public AudioBehaviour deathSound;  // ���� ����

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