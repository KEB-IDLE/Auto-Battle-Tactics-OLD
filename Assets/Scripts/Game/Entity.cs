/*
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AnimationComponent))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(AttackComponent))]
[RequireComponent(typeof(MoveComponent))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TeamComponent))]
[RequireComponent(typeof(EffectComponent))]
public class Entity : MonoBehaviour
{

    [Header("Scriptable Object")]
<<<<<<< HEAD
    [Tooltip("Asset > Script > Game > ScriptableObject > Unit/Projectile ÏÑ†ÌÉù ÌõÑ ÏõêÌïòÎäî Îç∞Ïù¥ÌÑ∞ ÏÇΩÏûÖ")]
=======
    [Tooltip("Asset > Script > Game > ScriptableObject > Unit/Projectile º±≈√ »ƒ ø¯«œ¥¬ µ•¿Ã≈Õ ª¿‘")]
>>>>>>> aaf1d00e3d080ef1c53cdeddc65fc6d773072373
    [SerializeField] private EntityData entityData;

    private HealthComponent _health; 
    private AttackComponent _attack;
    private MoveComponent _move;
    private TeamComponent _team;
    private AnimationComponent _animation;
    private EffectComponent _effect;

    public virtual void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _attack = GetComponent<AttackComponent>();
        _move = GetComponent<MoveComponent>();
        _team = GetComponent<TeamComponent>();
        _animation = GetComponent<AnimationComponent>();
        _effect = GetComponent<EffectComponent>();

        if (entityData == null)
        {
            Debug.LogError($"{name}Ïóê EntityDataÍ∞Ä Ìï†ÎãπÎêòÏßÄ ÏïäÏïòÏäµÎãàÎã§!");
            return;
        }
    }

    public void Start()
    {
        _health.Initialize(entityData);
        _attack.Initialize(entityData);
        _move.Initialize(entityData);
        _animation.Initialize(entityData);
        _effect.Initialize(entityData);
        _animation.Bind();
        _effect.Bind();
    }
}
*/

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AnimationComponent))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(AttackComponent))]
[RequireComponent(typeof(MoveComponent))]
[RequireComponent(typeof(PositionComponent))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TeamComponent))]
[RequireComponent(typeof(EffectComponent))]
public class Entity : MonoBehaviour
{

    [SerializeField] private EntityData entityData;

    private HealthComponent _health;
    private AttackComponent _attack;
    private MoveComponent _move;
    private PositionComponent _position;
    private TeamComponent _team;
    private AnimationComponent _animation;
    private EffectComponent _effect;
    public string UnitId { get; private set; }
    public string UnitType => LayerMask.LayerToName(gameObject.layer);
    public EntityData Data => entityData;

    public virtual void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _attack = GetComponent<AttackComponent>();
        _move = GetComponent<MoveComponent>();
        _position = GetComponent<PositionComponent>();
        _team = GetComponent<TeamComponent>();
        _animation = GetComponent<AnimationComponent>();
        _effect = GetComponent<EffectComponent>();

        if (entityData == null)
        {
            Debug.LogError($"{name}Ïóê EntityDataÍ∞Ä Ìï†ÎãπÎêòÏßÄ ÏïäÏïòÏäµÎãàÎã§!");
            return;
        }
    }

    public void Start()
    {
        _health.Initialize(entityData);
        _attack.Initialize(entityData);
        _move.Initialize(entityData);
        _position.Initialize(entityData);
        _animation.Initialize(entityData);
        _effect.Initialize(entityData);
        _animation.Bind();
        _effect.Bind();
    }
    public void SetUnitId(string id)
    {
        UnitId = id;
    }
}
