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
        
        // 필요한 풀을 자동으로 등록
        RegisterRequiredPools();
    }

    /// <summary>
    /// 이 유닛이 필요로 하는 모든 풀을 ObjectPoolManager에 등록합니다.
    /// </summary>
    private void RegisterRequiredPools()
    {
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager.Instance is null!");
            return;
        }

        // 1. 발사체 풀 등록 (원거리 공격 유닛만)
        if (entityData.attackType == AttackType.Ranged && entityData.projectilePrefab != null)
        {
            string projectilePoolName = entityData.projectilePoolName;
            if (!ObjectPoolManager.Instance.HasPool(projectilePoolName))
            {
                ObjectPoolManager.Instance.RegisterProjectilePool(
                    projectilePoolName, 
                    entityData.projectilePrefab, 
                    poolSize: 15
                );
            }

            // 2. Flight Effect 풀 등록 (발사체에 Flight Effect가 있는 경우)
            if (entityData.projectileData != null && entityData.projectileData.FlightEffectPrefab != null)
            {
                string flightEffectPoolName = entityData.projectileData.FlightEffectPrefab.name;
                if (!ObjectPoolManager.Instance.HasPool(flightEffectPoolName))
                {
                    ObjectPoolManager.Instance.RegisterEffectPool(
                        flightEffectPoolName,
                        entityData.projectileData.FlightEffectPrefab,
                        poolSize: 10
                    );
                }
            }
        }

        // 3. 공격 이펙트 풀 등록
        if (entityData.attackEffectPrefab != null)
        {
            string attackEffectPoolName = entityData.attackEffectPrefab.name;
            if (!ObjectPoolManager.Instance.HasPool(attackEffectPoolName))
            {
                ObjectPoolManager.Instance.RegisterEffectPool(
                    attackEffectPoolName,
                    entityData.attackEffectPrefab,
                    poolSize: 8
                );
            }
        }

        // 4. 기타 이펙트들 등록
        RegisterEffectPool(entityData.summonEffectPrefab, "summon");
        RegisterEffectPool(entityData.takeDamageEffectPrefeb, "takeDamage");  
        RegisterEffectPool(entityData.deathEffectPrefab, "death");
    }

    /// <summary>
    /// 개별 이펙트 풀을 등록하는 헬퍼 메서드
    /// </summary>
    private void RegisterEffectPool(GameObject effectPrefab, string effectType)
    {
        if (effectPrefab != null)
        {
            string poolName = effectPrefab.name;
            if (!ObjectPoolManager.Instance.HasPool(poolName))
            {
                ObjectPoolManager.Instance.RegisterEffectPool(
                    poolName,
                    effectPrefab,
                    poolSize: 5
                );
            }
        }
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
        _health.OnDeath += _move.StopAllAction;
        CombatManager.OnGameEnd += _attack.StopAllAction;
        CombatManager.OnGameEnd += _move.StopAllAction;
        CombatManager.OnGameEnd += _animation.StopAllAction;
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
        _health.OnDeath -= _move.StopAllAction;
        CombatManager.OnGameEnd -= _attack.StopAllAction;
        CombatManager.OnGameEnd -= _move.StopAllAction;
        CombatManager.OnGameEnd -= _animation.StopAllAction;
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