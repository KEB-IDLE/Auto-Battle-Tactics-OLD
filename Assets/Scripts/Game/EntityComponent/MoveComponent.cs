using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveComponent : MonoBehaviour, IMoveNotifier, IOrientable
{
    private bool _isMine;
    public event Action OnMove;

    Transform coreTransform;
    IAttackNotifier _attackNotifier;
    NavMeshAgent _agent;
    IAttackable _attacker;
    IDamageable _health;
    ITeamProvider _teamProvider;
    bool _isMoving;
    bool _isAttackLock;
    bool _isGameEnded;
    bool _isBattleStarted;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _attacker = GetComponent<IAttackable>();
        _health = GetComponent<IDamageable>();
        _teamProvider = GetComponent<ITeamProvider>();
        _attackNotifier = GetComponent<IAttackNotifier>();
    }

    void OnEnable()
    {
        if (_attackNotifier != null)
            _attackNotifier.OnAttackStateChanged += OnAttackStateChanged;
    }

    void OnDisable()
    {
        if (_attackNotifier != null)
            _attackNotifier.OnAttackStateChanged -= OnAttackStateChanged;
    }

    void Start()
    {
        coreTransform = CoreRegistry.Instance.GetEnemyCore(_teamProvider.Team);
        // 배치 단계에서는 스폰 시점에 isStopped=true가 들어오므로 여기서 건드릴 필요 없음
    }

    void Update()
    {
        // 에이전트 유효성 / NavMesh 탑재 확인
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh || !_isBattleStarted)
            return;

        if (_isGameEnded || _isAttackLock || (_health != null && !_health.IsAlive()))
        {
            if (_isMoving) _isMoving = false;
            return;
        }

        // 목적지 갱신
        Vector3? dest = null;

        var target = _attacker != null ? _attacker.DetectTarget() : null;
        if (target != null && target.IsAlive() && CanSee(target))
        {
            var mb = target as MonoBehaviour;
            if (mb != null) dest = mb.transform.position;
        }
        else if (coreTransform != null)
        {
            dest = coreTransform.position;
        }

        if (dest.HasValue)
        {
            // 불필요한 호출 줄이기
            if (!_agent.hasPath || Vector3.SqrMagnitude(_agent.destination - dest.Value) > 0.25f)
            {
                _agent.isStopped = false;
                _agent.SetDestination(dest.Value);
            }
        }

        if (!_isMoving)
        {
            _isMoving = true;
            OnMove?.Invoke();
        }
    }

    public void Initialize(EntityData data)
    {
        if (_agent == null) return;

        _agent.speed = data.moveSpeed;
        _agent.acceleration = data.moveSpeed;
        _agent.autoBraking = true;

        _isMoving = false;
        _isAttackLock = false;
        _isGameEnded = false;
        _isBattleStarted = false;

        string agentTypeName = data.entityScale switch
        {
            EntityScale.Small => "Small",
            EntityScale.Medium => "Medium",
            EntityScale.Large => "Large",
            _ => null
        };

        if (!string.IsNullOrEmpty(agentTypeName))
        {
            // 이름으로 ID를 찾아서 적용
            int resolved = NavMeshAgentTypeResolver.GetIdOrFallback(agentTypeName, _agent.agentTypeID);
            if (_agent.agentTypeID != resolved)
                _agent.agentTypeID = resolved;

            // (선택) 빌드 설정값으로 반지름/키를 맞춰주면 충돌/통과 이슈를 줄일 수 있음
            var s = NavMesh.GetSettingsByID(_agent.agentTypeID);
            if (s.agentRadius > 0f) _agent.radius = s.agentRadius;
            if (s.agentHeight > 0f) _agent.height = s.agentHeight;
        }


        // 에이전트가 꺼져있으면(배치 단계 등) 더 진행하지 않음
        if (!_agent.enabled) return;

        // NavMesh 위가 아니면 한 번만 올려보고 실패하면 바로 중단
        if (!_agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 3f, NavMesh.AllAreas)
                && _agent.Warp(hit.position))
            {
                // OK
            }
            else
            {
                Debug.Log("Unit is not on navmesh");
                return; // ❗ 여기서 끝내면 ResetPath/SetDestination 호출 안 함
            }
        }

        if (_agent.hasPath) _agent.ResetPath();
        if (coreTransform != null) _agent.SetDestination(coreTransform.position);
    }
    private void OnAttackStateChanged(bool isAttacking)
    {
        _isAttackLock = isAttacking;

        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            return;

        _agent.isStopped = isAttacking;
        if (isAttacking && _attacker != null && _attacker.IsAttacking() && _agent.hasPath)
            _agent.ResetPath();
    }

    public void LookAtTarget(IDamageable target)
    {
        var mb = target as MonoBehaviour;
        if (mb == null) return;

        Vector3 targetPos = mb.transform.position;
        targetPos.y = transform.position.y;
        transform.LookAt(targetPos);
    }

    private bool CanSee(IDamageable target)
    {
        var attackComponent = _attacker as AttackComponent;
        if (attackComponent == null || target == null) return false;

        if (attackComponent.isMagic) return true;

        var firePoint = attackComponent.firePoint != null ? attackComponent.firePoint : attackComponent.transform;
        var targetTransform = (target as MonoBehaviour)?.transform;
        if (firePoint == null || targetTransform == null) return false;

        return attackComponent.IsTargetVisible(firePoint, targetTransform);
    }

    public void StopAllAction()
    {
        _isGameEnded = true;
        if (_agent != null) _agent.isStopped = true;
    }

    public void SetIsMine(bool isMine) => _isMine = isMine;
    public void IsBattleStarted()
    {
        var rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.None;
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;

        _isBattleStarted = true;
    }
}

public static class NavMeshAgentTypeResolver
{
    private static Dictionary<string, int> _nameToId;

    /// <summary>프로젝트의 모든 NavMesh Agent Type을 스캔해 이름→ID 매핑을 캐시합니다.</summary>
    private static void EnsureCache()
    {
        if (_nameToId != null) return;
        _nameToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        int count = NavMesh.GetSettingsCount();
        for (int i = 0; i < count; i++)
        {
            var settings = NavMesh.GetSettingsByIndex(i);     // 각 타입의 설정
            int id = settings.agentTypeID;
            string name = NavMesh.GetSettingsNameFromID(id);  // 에디터에 보이는 이름
            if (!string.IsNullOrEmpty(name))
                _nameToId[name] = id;
        }
    }

    /// <summary>이름으로 ID를 찾습니다. 실패 시 false.</summary>
    public static bool TryGetId(string agentTypeName, out int id)
    {
        EnsureCache();
        return _nameToId.TryGetValue(agentTypeName, out id);
    }

    /// <summary>이름으로 ID를 찾고, 없으면 fallback 반환.</summary>
    public static int GetIdOrFallback(string agentTypeName, int fallbackId, bool warnIfMissing = true)
    {
        EnsureCache();
        if (_nameToId.TryGetValue(agentTypeName, out var id)) return id;
        if (warnIfMissing)
            Debug.LogWarning($"[NavMeshAgentTypeResolver] AgentType '{agentTypeName}'을(를) 찾지 못했습니다. fallback ID {fallbackId} 사용.");
        return fallbackId;
    }
}