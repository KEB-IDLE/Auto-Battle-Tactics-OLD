using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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
    private bool _isAttackLock;
    private bool _isGameEnded;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _attacker = GetComponent<IAttackable>();
        _health = GetComponent<IDamageable>();
        _teamProvider = GetComponent<ITeamProvider>();
        _attackNotifier = GetComponent<IAttackNotifier>();
    }

    private void Start()
    {
        coreTransform = CoreRegistry.Instance.GetEnemyCore(_teamProvider.Team);
        _attackNotifier.OnAttackStateChanged += OnAttackStateChanged;
    }

    void Update()
    {
        if (!_agent.isOnNavMesh)
        {
            Debug.LogError("unit is not on navmesh.");
            return;
        }

        if ( _isGameEnded || _isAttackLock || !_health.IsAlive())
        {
            if (_isMoving)
                _isMoving = false;
            return;
        }
        if (_agent.isStopped)
        {
            _agent.isStopped = false;
        }
            
        var target = _attacker.DetectTarget();

        if (target != null && target.IsAlive() && CanSee(target))
        {
            var mb = target as MonoBehaviour;
            _agent.SetDestination(mb.transform.position);
        }
        else if (coreTransform != null)
        {
            _agent.SetDestination(coreTransform.position);
        }
            

        if (!_isMoving)
        {
            _isMoving = true;
            OnMove?.Invoke();
        }
    }

    public void Initialize(EntityData data)
    {
        _agent.speed = data.moveSpeed;
        _agent.acceleration = data.moveSpeed;
        _agent.autoBraking = false;
        _isMoving = false;
        _isAttackLock = false;
        _isGameEnded = false;

        switch (data.entityScale)
        {
            case EntityScale.Small:
                transform.localScale = Vector3.one * 1f;
                _agent.radius = 0.3f;
                _agent.height = 1.0f;
                _agent.agentTypeID = GetAgentTypeIDForScale(data.entityScale);
                break;
            case EntityScale.Medium:
                transform.localScale = Vector3.one * 1.2f;
                _agent.radius = 0.5f;
                _agent.height = 2.0f;
                _agent.agentTypeID = GetAgentTypeIDForScale(data.entityScale);
                break;
            case EntityScale.Large:
                transform.localScale = Vector3.one * 1.5f;
                _agent.radius = 1.0f;
                _agent.height = 3.0f;
                _agent.agentTypeID = GetAgentTypeIDForScale(data.entityScale);
                break;
        }
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            _agent.Warp(hit.position); // NavMesh 위로 이동
        }

        // 3. 경로 재설정
        _agent.ResetPath();
        if (coreTransform != null)
            _agent.SetDestination(coreTransform.position);

    }
    private void OnAttackStateChanged(bool isAttacking)
    {
        _isAttackLock = isAttacking;
        _agent.isStopped = isAttacking;
        if (_attacker.IsAttacking())
            _agent.ResetPath();
    }
    public void LookAtTarget(IDamageable target)
    {
        var mb = target as MonoBehaviour;
        if (mb != null)
        {
            Vector3 targetPos = mb.transform.position;
            targetPos.y = transform.position.y;
            transform.LookAt(targetPos);
        }
    }

    private bool CanSee(IDamageable target)
    {
        var attackComponent = _attacker as AttackComponent;
        if (attackComponent == null || target == null)
            return false;

        if (attackComponent.isMagic)
            return true;

        var firePoint = attackComponent.firePoint != null ? attackComponent.firePoint : attackComponent.transform;
        var targetTransform = (target as MonoBehaviour)?.transform;

        if (firePoint == null || targetTransform == null)
            return false;

        return attackComponent.IsTargetVisible(firePoint, targetTransform);
    }

    public void StopAllAction()
    {
        _isGameEnded = true;
        _agent.isStopped = true;
    }

    public void SetIsMine(bool isMine) => _isMine = isMine;

    int GetAgentTypeIDForScale(EntityScale scale)
    {
        switch (scale)
        {
            case EntityScale.Small: return 1479372276; // Small
            case EntityScale.Medium: return -1923039037; // Medium
            case EntityScale.Large: return -902729914; // large
            default: return 0;
        }
    }


}