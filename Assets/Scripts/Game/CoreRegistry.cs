using UnityEngine;

public class CoreRegistry : MonoBehaviour
{
    public static CoreRegistry Instance { get; private set; }

    [Header("씬에서 드래그: 아군 코어, 적군 코어")]
    [SerializeField] private Transform redCoreTransform;
    [SerializeField] private Transform blueCoreTransform;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // (필요하다면) DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /// <summary>지정 팀의 코어 Transform</summary>
    public Transform GetCore(Team team)
        => team == Team.Red ? redCoreTransform : blueCoreTransform;

    /// <summary>내 팀을 넘기면 적군 코어 반환</summary>
    public Transform GetEnemyCore(Team myTeam)
        => GetCore(myTeam == Team.Red ? Team.Blue : Team.Red);
}
