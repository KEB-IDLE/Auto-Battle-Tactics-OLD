using UnityEngine;
using System.Linq;
using UnityEngine.AI;


public class MoveComponent : MonoBehaviour
{

    Transform coreTransform;
    NavMeshAgent _agent;
    IAttackable _attacker;
    ITeamProvider _teamProvider;
    private float moveSpeed;

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
        // 공격 중이면 이동 중지
        if(_attacker.IsAttacking())
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            return;
        }
        if (_agent.isStopped)
        {
            _agent.isStopped = false;
        }
        var target = _attacker.DetectTarget();
        if (target != null && target.IsAlive())
        {
            var mb = target as MonoBehaviour;
            _agent.SetDestination(mb.transform.position);
        }
        else
        {
            _agent.SetDestination(coreTransform.position);
        }
    }

    public void Initialize(EntityData data)
    {
        moveSpeed = data.moveSpeed; // 이동 속도 초기화
        _agent.speed = moveSpeed;
        _agent.acceleration = moveSpeed;
        _agent.autoBraking = false;
        
    }
}
