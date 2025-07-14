using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System;



public class MoveComponent : MonoBehaviour, IMoveNotifier
{
    public event Action OnMove;

    Transform coreTransform;
    NavMeshAgent _agent;
    IAttackable _attacker;
    ITeamProvider _teamProvider;
    private float moveSpeed;
    bool _isMoving;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _attacker = GetComponent<IAttackable>();
        _teamProvider = GetComponent<ITeamProvider>();
    }

    private void Start()
    {
        coreTransform = CoreRegistry.Instance.GetEnemyCore(_teamProvider.Team);
    }

    void Update()
    {
        if(_attacker.IsAttacking())
        {
            if(_isMoving) 
                _isMoving = false;
            _agent.isStopped = true;
            _agent.ResetPath();
            return;
        }
        if (_agent.isStopped)
            _agent.isStopped = false;

        var target = _attacker.DetectTarget();
        if (target != null && target.IsAlive())
        {
            var mb = target as MonoBehaviour;
            _agent.SetDestination(mb.transform.position);
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
        moveSpeed = data.moveSpeed; // 이동 속도 초기화
        _agent.speed = moveSpeed;
        _agent.acceleration = moveSpeed;
        _agent.autoBraking = false;
        _isMoving = false;

    }
}
