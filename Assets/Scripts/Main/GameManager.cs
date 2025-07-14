using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public UserProfile profile;
    public UserRecord record;
    public List<UserChampion> champions = new();
    public List<UserDeck> decks = new();
    public List<MatchHistory> matchHistory = new();

    public string accessToken;  // JWT 토큰 저장



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
