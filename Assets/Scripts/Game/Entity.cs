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
[RequireComponent(typeof(UnitNetwork))]

public class Entity : MonoBehaviour
{
    [SerializeField] private EntityData entityData;

    private HealthComponent _health;
    private AttackComponent _attack;
    private MoveComponent _move;
    private TeamComponent _team;
    private AnimationComponent _animation;
    private EffectComponent _effect;
    private HealthBar _healthBar;

    public string UnitId { get; private set; }
    public string OwnerId { get; private set; }
    public string UnitType => entityData.unitType;
    public EntityData Data => entityData;
    public Team Team => _team.Team;


    public virtual void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _attack = GetComponent<AttackComponent>();
        _move = GetComponent<MoveComponent>();
        _team = GetComponent<TeamComponent>();
        _animation = GetComponent<AnimationComponent>();
        _effect = GetComponent<EffectComponent>();
        _healthBar = GetComponentInChildren<HealthBar>(true);

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

    public void SetData(EntityData data) => entityData = data;
    public void SetOwnership(bool isMine) => _move.SetIsMine(isMine);


    public void BindEvent()
    {
        if (_health == null || _attack == null || _move == null || _animation == null || _effect == null || _healthBar == null)
        {
            Debug.LogWarning($"⚠️ [Entity] BindEvent 실패. 필요한 컴포넌트가 빠져있음: {gameObject.name}");
            return;
        }
        _attack.OnAttackStateChanged += _animation.HandleAttack;
        _move.OnMove += _animation.HandleMove;
        _health.OnDeath += _animation.HandleDeath;
        _attack.OnAttackEffect += _effect.PlayAttackEffect;
        _health.OnTakeDamageEffect += _effect.PlayTakeDamageEffect;
        _health.OnDeathEffect += _effect.PlayDeathEffect;
        _health.OnHealthChanged += _healthBar.UpdateBar;
        CombatManager.OnGameEnd += _attack.StopAllAction;
        CombatManager.OnGameEnd += _move.StopAllAction;
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
        CombatManager.OnGameEnd -= _attack.StopAllAction;
        CombatManager.OnGameEnd -= _move.StopAllAction;
    }
    public void SetUnitId(string id)
    {
        UnitId = id;
        Debug.Log($"[Entity] SetUnitId: {id}");
    }

    public void SetOwnerId(string id)
    {
        OwnerId = id;
    }
}