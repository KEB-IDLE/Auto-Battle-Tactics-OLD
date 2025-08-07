using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 로그인한 유저의 세션 정보를 저장하고 관리하는 싱글턴 클래스입니다.
/// 유저 프로필, 기록, JWT 토큰 등을 전역에서 접근 가능하게 유지합니다.
/// </summary>
public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public UserProfile profile;                             // 유저의 프로필 정보
    public UserRecord record;                               // 유저의 게임 전적/기록
    public List<int> ownedProfileIcons = new List<int>();   // 유저가 가진 프로필 아이콘 ID 리스트
    public string accessToken;                              // JWT 토큰

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