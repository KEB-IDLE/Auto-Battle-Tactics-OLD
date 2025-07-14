using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AnimationComponent))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(AttackComponent))]
[RequireComponent(typeof(MoveComponent))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TeamComponent))]
public class Entity : MonoBehaviour
{
    
    [SerializeField] private EntityData entityData;
    private HealthComponent _health; 
    private AttackComponent _attack;
    private MoveComponent _move;
    private TeamComponent _team;
    private AnimationComponent _animation;

    public virtual void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _attack = GetComponent<AttackComponent>();
        _move = GetComponent<MoveComponent>();
        _team = GetComponent<TeamComponent>();
        _animation = GetComponent<AnimationComponent>();

        if (entityData == null)
        {
            Debug.LogError($"{name}�� EntityData�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }
    }

    public void Start()
    {
        _health.Initialize(entityData);
        _attack.Initialize(entityData);
        _move.Initialize(entityData);
        _animation.Initialize(entityData);
        _animation.Bind();
    }
}
