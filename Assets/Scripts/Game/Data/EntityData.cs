using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
    [Header("Entity Information")]

    [Tooltip("이름 설정")]
    public string entityName;
    [Tooltip("최대 체력 설정")]
    public float maxHP;
    [Tooltip("유닛 대상 공격력")]
    public float attackDamage;
    [Tooltip("코어 대상 공격력")]
    public float attackCoreDamage;
    [Tooltip("재공격까지 대기시간")]
    public float attackCooldown;
    [Tooltip("이동 속도")]
    public float moveSpeed;
    [Tooltip("개체 크기")]
    public EntityScale entityScale;
    [Header("Layer 설정")]
    [Tooltip("7: Human, 8:Mutant 9: Tower, 10: Core, 11: Projectile, 12: Structure, 13: Obstacle")]
    public int Layer;

    [Header("Target Detection")]
    [Tooltip("공격 인지 범위")]
    public float detectionRadius;
    [Tooltip("실제 공격 가능 범위")]
    public float attackRange;
    [Tooltip("타겟을 해제하는 임계 거리")]
    public float disengageRange;
    public enum AttackPriority
    {
        AllUnits,    // Agent, Tower, Core
        TowersOnly,  // Tower, Core
        CoreOnly     // Core
    }
    [Tooltip("공격 대상이 Agent, Tower, Core 중 어떤 타입인지 설정")]
    public AttackPriority attackPriority; // 공격 우선순위
    [Tooltip("공중 공격 가능 여부 체크")]
    public AttackableType attackableType; // 공격 가능 타입

    [Tooltip("유닛의 공격 방식 (근거리 = Melee, 원거리 = Ranged etc..)")]
    public AttackType attackType;

    [Header("Entity Prefab")]
    public GameObject entityPrefab; // 엔티티 프리팹

    [Header("Projectile Info")]
    [Tooltip("발사체 프리팹")]
    public GameObject projectilePrefab;
    [Tooltip("오브젝트 풀에 자동으로 연결합니다.")]
    public string projectilePoolName;

    [Header("Effect Prefab")]
    [Tooltip("소환 이펙트")]
    public GameObject summonEffectPrefab; // 소환 이펙트 프리팹
    [Tooltip("공격 이펙트")]
    public GameObject attackEffectPrefab; // 공격 이펙트 프리팹
    [Tooltip("피격 이펙트")]
    public GameObject takeDamageEffectPrefeb; // 공격 이펙트 프리팹
    [Tooltip("죽음 이펙트")]
    public GameObject deathEffectPrefab; // 죽음 이펙트 프리팹

    [Tooltip("발사체 이동 이펙트")]
    public GameObject projectileAttackingEffectPrefab;

    [Header("Audio Settings")]
    public AudioClip summonSound; // 소환 사운드
    public AudioClip attackSound; // 공격 사운드
    public AudioClip deathSound;  // 죽음 사운드

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
public enum AttackType
{
    Melee,
    Ranged,
    Magic
}