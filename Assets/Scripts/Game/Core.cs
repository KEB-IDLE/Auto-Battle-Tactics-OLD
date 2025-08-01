using UnityEngine;

[RequireComponent (typeof(TeamComponent))]
[RequireComponent (typeof(HealthComponent))]
[RequireComponent (typeof(CoreComponent))]
//[RequireComponent (typeof(Animator))]
//[RequireComponent (typeof(AnimationComponent))]
public class Core : MonoBehaviour
{
    
    [SerializeField] private float maxHP;
    private HealthComponent _health;
    private TeamComponent _team;
    private CoreComponent _core;

    void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _team = GetComponent<TeamComponent>();
        _core = GetComponent<CoreComponent>();
    }

    private void Start()
    {
        _health.Initialize(maxHP);
        //07.18�� �ؾ��� ��..
    }
}
