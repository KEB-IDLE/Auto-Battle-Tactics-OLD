using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
    [Header("Entity Information")]
    //public string entityName;   
    public float maxHP;         
    public float attackDamage;  
    public float attackCoreDamage; 
    public float attackCooldown;
    public float moveSpeed;    
    public EntityScale entityScale; 

    [Header("Target Detection")]
    public float detectionRadius;   // ���� ��� ���� ����
    public float attackRange;       // ���� ���� ����
    public float disengageRange;    // ��׷� Ǫ�� �Ѱ� �Ÿ�
    public AttackableType attackableType; // ���� ���� Ÿ��
    public enum AttackPriority
    {
        AllUnits,    // Agent, Tower, Core
        TowersOnly,  // Tower, Core
        CoreOnly     // Core
    }
    public AttackPriority attackPriority; // ���� �켱����
    
    [Header("Entity Prefab")]
    public GameObject entityPrefab; // ��ƼƼ ������
    public GameObject summonEffectPrefab; // ��ȯ ����Ʈ ������
    public GameObject attackEffectPrefab; // ���� ����Ʈ ������
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
