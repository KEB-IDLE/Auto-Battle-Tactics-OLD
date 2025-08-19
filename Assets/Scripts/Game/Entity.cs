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
[RequireComponent(typeof(AudioSource))]
public class Entity : MonoBehaviour
{
    [HideInInspector] public EntityData entityData;

    private HealthComponent _health;
    private AttackComponent _attack;
    private MoveComponent _move;
    private TeamComponent _team;
    private AnimationComponent _animation;
    private EffectComponent _effect;
    private HealthBar _healthBar;

    public string UnitId { get; private set; }
    public string OwnerId { get; private set; }

    // ⚠️ entityData가 null일 수 있으니 안전하게
    public string UnitType => entityData != null ? entityData.unitType : string.Empty;
    public EntityData Data => entityData;
    public Team Team => _team.Team;

    // 지연 초기화 상태 플래그
    public bool IsInitialized { get; private set; }
    private bool _eventsBound;

    public virtual void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _attack = GetComponent<AttackComponent>();
        _move = GetComponent<MoveComponent>();
        _team = GetComponent<TeamComponent>();
        _animation = GetComponent<AnimationComponent>();
        _effect = GetComponent<EffectComponent>();
        _healthBar = GetComponentInChildren<HealthBar>(true);

        // ❗ 여기서는 절대 entityData를 요구하지 말 것.
        // 초기화 전 전투 로직이 돌지 않도록 잠시 꺼둔다.
        SetSubsystemsActive(false);
    }

    public void Start()
    {
        // 프리팹에 데이터가 직결되어 있거나, SetData가 Awake~Start 사이에 들어왔다면 여기서 초기화됨
        TryInitialize();
    }

    public void OnDestroy()
    {
        if (_eventsBound) UnbindEvent();
    }

    public void SetUnitId(string id)
    {
        UnitId = id;
        Debug.Log($"[Entity] SetUnitId: {id}");
    }

    // 런타임 데이터 주입 진입점
    public void SetData(EntityData data)
    {
        entityData = data;
        if (!Application.isPlaying) return;
        TryInitialize(); // 데이터가 늦게 와도 즉시 한 번만 초기화
    }

    public void SetOwnership(bool isMine) => _move.SetIsMine(isMine);
    public void SetOwnerId(string id) => OwnerId = id;

    // ---- 지연 초기화 본체 ----
    private void TryInitialize()
    {
        if (!Application.isPlaying) return;
        if (IsInitialized) return;
        if (entityData == null) return; // 데이터 아직 없음 → 대기

        // 초기화 순서: 스탯/바/전투/이동/애니/이펙트 → 이벤트 바인딩 → 풀 등록 → 서브시스템 활성화
        _health.Initialize(entityData);
        if (_healthBar != null) _healthBar.Initialize(_health);

        _attack.Initialize(entityData);
        _move.Initialize(entityData);
        _animation.Initialize(entityData);
        _effect.Initialize(entityData);

        if (!_eventsBound)
        {
            BindEvent();
            _eventsBound = true;
        }

        RegisterRequiredPools(); // 아래에 null 가드 추가됨

        SetSubsystemsActive(true);

        IsInitialized = true;
    }

    private void SetSubsystemsActive(bool active)
    {
        if (_attack != null) _attack.enabled = active;
        if (_move != null) _move.enabled = active;
        if (_animation != null) _animation.enabled = active;
        if (_effect != null) _effect.enabled = active;
        // Health/Team/Bar는 enabled 유지해도 무방
    }

    /// <summary>
    /// 이 유닛이 필요로 하는 모든 풀을 ObjectPoolManager에 등록합니다.
    /// </summary>
    private void RegisterRequiredPools()
    {
        if (!Application.isPlaying) return;
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager.Instance is null!");
            return;
        }
        if (entityData == null) return; // 안전 가드

        // 1. 발사체 풀 (원거리만)
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

            // 1-1. Flight Effect 풀
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

        // 2. 공격 이펙트
        RegisterEffectPool(entityData.attackEffectPrefab);

        // 3. 기타 이펙트들
        RegisterEffectPool(entityData.summonEffectPrefab);
        RegisterEffectPool(entityData.takeDamageEffectPrefeb);
        RegisterEffectPool(entityData.deathEffectPrefab);
    }

    private void RegisterEffectPool(GameObject effectPrefab)
    {
        if (effectPrefab == null) return;

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
        CombatManager.OnRoundStart += _move.IsBattleStarted;
    }

    public void UnbindEvent()
    {
        if (_attack == null || _move == null || _animation == null || _health == null || _healthBar == null || _effect == null)
            return;

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
        CombatManager.OnRoundStart -= _move.IsBattleStarted;
    }
}
