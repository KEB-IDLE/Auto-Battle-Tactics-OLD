using UnityEngine;

[RequireComponent(typeof(TeamComponent))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(CoreComponent))]
//[RequireComponent (typeof(Animator))]
//[RequireComponent (typeof(AnimationComponent))]
public class Core : MonoBehaviour
{

    [SerializeField] private ObjectData objectData;
    private HealthComponent _health;
    private HealthBar _healthBar;
    private TeamComponent _team;
    private CoreComponent _core;
    public ObjectData GetObjectData() => objectData;



    void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _team = GetComponent<TeamComponent>();
        _core = GetComponent<CoreComponent>();
        _healthBar = GetComponentInChildren<HealthBar>(true);

        Debug.Log($"🧱 [검사용] Core.Awake() 호출됨: {gameObject.name}, ID: {GetInstanceID()}, Team: {_team.Team}");

        if (objectData == null)
        {
            Debug.LogError($"{name} ObjectData is null!");
            return;
        }
        // ✅ HealthComponent 먼저 초기화
        if (!_health.IsInitialized)
        {
            _health.Initialize(objectData.maxHP);
            Debug.Log($"⚙️ [Core] HealthComponent 초기화: {objectData.maxHP}");
        }

        if (_healthBar != null)
        {
            _healthBar.Initialize(_health);
            _healthBar.UpdateBar(_health.CurrentHp, _health.MaxHp);
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