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
    private float moveSpeed;
    bool _isMoving;
    private bool _isAttackLock;
    private bool isGameEnded = false;

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
        if ( isGameEnded || _isAttackLock || !_health.IsAlive())
        {
            if (_isMoving)
                _isMoving = false;
            return;
        }
        if (_agent.isStopped)
            _agent.isStopped = false;

        var target = _attacker.DetectTarget();
        if (target != null && target.IsAlive() && CanSee(target))
        {
            var mb = target as MonoBehaviour;
            _agent.SetDestination(mb.transform.position);
            //LookAtTarget(target);
        }
        else
            _agent.SetDestination(coreTransform.position);

        if (!_isMoving)
        {
            _isMoving = true;
            OnMove?.Invoke();
        }
    }

    public void Initialize(EntityData data)
    {
        moveSpeed = data.moveSpeed;
        _agent.speed = moveSpeed;
        _agent.acceleration = moveSpeed;
        _agent.autoBraking = false;
        _isMoving = false;

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
        isGameEnded = true;
        _agent.isStopped = true;
    }

    public void SetIsMine(bool isMine) => _isMine = isMine;



}