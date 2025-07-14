using UnityEngine;

public class CoreComponent : MonoBehaviour
{
    [SerializeField] private ObjectData coreData;
    private HealthComponent _health;


    void Awake()
    {
        _health = GetComponent<HealthComponent>();
        //_health.Initialize(coreData);
    }

}
