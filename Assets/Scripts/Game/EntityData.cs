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
    public float detectionRadius;   // 공격 대상 인지 범위
    public float attackRange;       // 공격 가능 범위
    public float disengageRange;    // 어그로 푸는 한계 거리
    public AttackableType attackableType; // 공격 가능 타입
    public enum AttackPriority
    {
        AllUnits,    // Agent, Tower, Core
        TowersOnly,  // Tower, Core
        CoreOnly     // Core
    }
    public AttackPriority attackPriority; // 공격 우선순위
    
    [Header("Entity Prefab")]
    public GameObject entityPrefab; // 엔티티 프리팹
    public GameObject summonEffectPrefab; // 소환 이펙트 프리팹
    public GameObject attackEffectPrefab; // 공격 이펙트 프리팹
    public GameObject deathEffectPrefab; // 죽음 이펙트 프리팹

    [Header("Audio Settings")]
    public AudioBehaviour summonSound; // 소환 사운드
    public AudioBehaviour attackSound; // 공격 사운드
    public AudioBehaviour deathSound;  // 죽음 사운드

    [Header("Animation Settings")]
    [Tooltip("이 엔티티 전용 AnimatorController (기본 컨트롤러를 Override 하는 용도)")]
    public RuntimeAnimatorController animatorController;
    [Tooltip("공격 애니메이션 클립")]
    public AnimationClip attackClip;
    [Tooltip("이동 애니메이션 클립")]
    public AnimationClip runClip;
    [Tooltip("죽음 애니메이션 클립")]
    public AnimationClip deathClip;
    [Tooltip("대기 애니메이션 클립")]
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
