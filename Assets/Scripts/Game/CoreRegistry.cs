using UnityEngine;

public class CoreRegistry : MonoBehaviour
{
    public static CoreRegistry Instance { get; private set; }

    [Header("Core Transform : RedCore, BlueCore")]
    [SerializeField] private Transform redCoreTransform;
    [SerializeField] private Transform blueCoreTransform;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }
    private void Start()
    {
        if (redCoreTransform != null)
            redCoreTransform.GetComponent<TeamComponent>()?.SetTeam(Team.Red);

        if (blueCoreTransform != null)
            blueCoreTransform.GetComponent<TeamComponent>()?.SetTeam(Team.Blue);

        Debug.Log("✅ 코어 팀 정보 설정 완료 (CoreRegistry)");
    }


    public Transform GetCore(Team team)
        => team == Team.Red ? redCoreTransform : blueCoreTransform;

    public Transform GetEnemyCore(Team myTeam)
        => GetCore(myTeam == Team.Red ? Team.Blue : Team.Red);
}
