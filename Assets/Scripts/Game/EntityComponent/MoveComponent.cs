using System;
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

    void Awake()
    {
        _agent        = GetComponent<NavMeshAgent>();
        _attacker     = GetComponent<IAttackable>();
        _health       = GetComponent<IDamageable>();
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
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
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

    // 필요시만 사용(프로젝트마다 ID 값이 다를 수 있음)
    int GetAgentTypeIDForScale(EntityScale scale)
    {
        switch (scale)
        {
            case EntityScale.Small:  return 1479372276;     // Small
            case EntityScale.Medium: return -1923039037;    // Medium
            case EntityScale.Large:  return -902729914;     // Large
            default: return 0;
        }
    }
}
