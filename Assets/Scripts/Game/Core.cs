using UnityEngine;

[RequireComponent (typeof(TeamComponent))]
[RequireComponent (typeof(HealthComponent))]
[RequireComponent (typeof(CoreComponent))]
//[RequireComponent (typeof(Animator))]
//[RequireComponent (typeof(AnimationComponent))]
public class Core : MonoBehaviour
{
    
    [SerializeField] private ObjectData objectData;
    private HealthComponent _health;
    private HealthBar _healthBar;
    private TeamComponent _team;
    private CoreComponent _core;
    private HealthBar healthBar;
    public ObjectData GetObjectData() => objectData;


    void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _team = GetComponent<TeamComponent>();
        _core = GetComponent<CoreComponent>();
        _healthBar = GetComponentInChildren<HealthBar>(true);

        if (objectData == null)
        {
            Debug.LogError($"{name} ObjectData is null!");
            return;
        }
    }

    public void SetData(ObjectData data) => objectData = data;

    public void BindEvent()
    {
        _health.OnHealthChanged += _healthBar.UpdateBar;
        _health.OnDeath += _core.OnCoreDestroyed;
    }

    public void UnBindEvent()
    {
        _health.OnDeath -= _core.OnCoreDestroyed;
        _health.OnHealthChanged -= _healthBar.UpdateBar;
    }
}