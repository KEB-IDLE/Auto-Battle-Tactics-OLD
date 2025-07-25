using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
    [Header("Entity Information")]

    [Tooltip("Entity name")]
    public string entityName;
    [Tooltip("Maximum health")]
    public float maxHP;
    [Tooltip("Attack damage to units")]
    public float attackDamage;
    [Tooltip("Attack damage to core")]
    public float attackCoreDamage;
    [Tooltip("Attack Impact Delay (seconds)")]
    public float attackImpactRatio;

    [Tooltip("Move speed")]
    public float moveSpeed;
    [Tooltip("Entity scale")]
    public EntityScale entityScale;

    [Header("Layer")]
    [Tooltip("Layer: 7=Human, 8=Mutant, 9=Tower, 10=Core, 11=Projectile, 12=Structure, 13=Obstacle, 14=Agent")]
    public int Layer;

    [Header("Target Detection")]
    [Tooltip("Target detection radius")]
    public float detectionRadius;
    [Tooltip("Attack range")]
    public float attackRange;
    [Tooltip("Distance to disengage target")]
    public float disengageRange;
    public enum AttackPriority
    {
        AllUnits,    // Agent, Tower, Core
        TowersOnly,  // Tower, Core
        CoreOnly     // Core
    }
    [Tooltip("Attack target type (Agent, Tower, Core)")]
    public AttackPriority attackPriority;
    //[Tooltip("Can attack air units or not")]
    //public AttackableType attackableType;

    [Tooltip("Attack type (Melee, Ranged, Magic)")]
    public AttackType attackType;

    [Header("Entity Prefab")]
    public GameObject entityPrefab;

    [Header("Magic")] // 마법사의 마법 피해 범위
    public float magicRadius;

    [Header("Ranged")]
    [Tooltip("Projectile prefab")]
    public GameObject projectilePrefab;
    [Tooltip("Automatically linked to object pool")]
    public string projectilePoolName;

    [Header("Effect Prefab")]
    [Tooltip("Summon effect prefab")]
    public GameObject summonEffectPrefab;
    [Tooltip("Attack effect prefab")]
    public GameObject attackEffectPrefab;
    [Tooltip("Take damage effect prefab")]
    public GameObject takeDamageEffectPrefeb;
    [Tooltip("Death effect prefab")]
    public GameObject deathEffectPrefab;
    //[Tooltip("Projectile move effect prefab")]
    //public GameObject projectileAttackingEffectPrefab;

    [Header("Audio Settings")]
    public AudioClip summonSound;
    public AudioClip attackSound;
    public AudioClip deathSound;

    [Header("Animation Settings")]
    [Tooltip("Entity-specific AnimatorController (used to override default controller)")]
    public RuntimeAnimatorController animatorController;
    [Tooltip("Attack animation clip")]
    public AnimationClip attackClip;
    [Tooltip("Run animation clip")]
    public AnimationClip runClip;
    [Tooltip("Death animation clip")]
    public AnimationClip deathClip;
    [Tooltip("Idle animation clip")]
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
