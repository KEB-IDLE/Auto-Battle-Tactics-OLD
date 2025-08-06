using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public UserProfile profile;
    public UserRecord record;
    public List<MatchHistory> matchHistory = new();

    public List<int> ownedProfileIcons = new List<int>();  // 유저가 가진 프로필 아이콘 ID 리스트

    public string accessToken;  // JWT 토큰

    // 사용 미정
    public string opponentId;
    public string roomId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}