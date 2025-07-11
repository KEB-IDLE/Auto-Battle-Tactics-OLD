using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public UserData userData = new UserData();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}

[System.Serializable]
public class UserData
{
    public int userId;
    public string nickname;
    public int level;
    public int exp;
    public int gold;
    public string tier;
    public int rankPoint;
    public int rankWins;
    public int rankLosses;
}
