using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public UserProfile profile;
    public UserRecord record;
    public List<MatchHistory> matchHistory = new();

    public List<int> ownedProfileIcons = new List<int>();  // 유저가 가진 프로필 아이콘 ID 리스트

    public string accessToken;  // JWT 토큰

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