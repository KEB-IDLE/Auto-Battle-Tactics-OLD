using UnityEngine;

public class CoreRegistry : MonoBehaviour
{
    public static CoreRegistry Instance { get; private set; }

    [Header("������ �巡��: �Ʊ� �ھ�, ���� �ھ�")]
    [SerializeField] private Transform redCoreTransform;
    [SerializeField] private Transform blueCoreTransform;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // (�ʿ��ϴٸ�) DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /// <summary>���� ���� �ھ� Transform</summary>
    public Transform GetCore(Team team)
        => team == Team.Red ? redCoreTransform : blueCoreTransform;

    /// <summary>�� ���� �ѱ�� ���� �ھ� ��ȯ</summary>
    public Transform GetEnemyCore(Team myTeam)
        => GetCore(myTeam == Team.Red ? Team.Blue : Team.Red);
}
