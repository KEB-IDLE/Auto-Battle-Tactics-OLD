
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CapsuleCollider))]
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
    [SerializeField] private EntityData entityData;

    private HealthComponent _health; 
    private AttackComponent _attack;
    private MoveComponent   _move;
    private TeamComponent   _team;
    private AnimationComponent _animation;
    private EffectComponent _effect;
    private HealthBar _healthBar;

    public virtual void Awake()
    {
        _health =   GetComponent<HealthComponent>();
        _attack =   GetComponent<AttackComponent>();
        _move =     GetComponent<MoveComponent>();
        _team =     GetComponent<TeamComponent>();
        _animation = GetComponent<AnimationComponent>();
        _effect =   GetComponent<EffectComponent>();
        _healthBar = GetComponentInChildren<HealthBar>();

        if (entityData == null)
        {
            Debug.LogError($"{name} EntityData is null!");
            return;
        }
    }

    public void Start()
    {
        _health.Initialize(entityData);
        _healthBar.Initialize(_health);
        _attack.Initialize(entityData);
        _move.Initialize(entityData);
        _animation.Initialize(entityData);
        _effect.Initialize(entityData);
        BindEvent();
    }

    public void SetData(EntityData data)
    {
        entityData = data;
    }

    public void BindEvent()
    {
        _attack.OnAttackStateChanged += _animation.HandleAttack;
        _move.OnMove += _animation.HandleMove;
        _health.OnDeath += _animation.HandleDeath;
        _attack.OnAttackEffect += _effect.PlayAttackEffect;
        _health.OnTakeDamageEffect += _effect.PlayTakeDamageEffect;
        _health.OnDeathEffect += _effect.PlayDeathEffect;
        _health.OnHealthChanged += _healthBar.UpdateBar;
    }

    public void UnbindEvent()
    {
        _attack.OnAttackStateChanged -= _animation.HandleAttack;
        _move.OnMove -= _animation.HandleMove;
        _health.OnDeath -= _animation.HandleDeath;
        _attack.OnAttackEffect -= _effect.PlayAttackEffect;
        _health.OnTakeDamageEffect -= _effect.PlayTakeDamageEffect;
        _health.OnDeathEffect -= _effect.PlayDeathEffect;
        _health.OnHealthChanged -= _healthBar.UpdateBar;
    }
}

/* network

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

    private HealthComponent     _health;
    private AttackComponent     _attack;
    private MoveComponent       _move;
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
            Debug.LogError($"{name}??EntityDataê°€ ? ë‹¹?˜ì? ?Šì•˜?µë‹ˆ??");
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
*/