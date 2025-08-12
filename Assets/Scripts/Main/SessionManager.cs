﻿using System.Collections.Generic;
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

    // ✅ 게스트 ID를 저장할 PlayerPrefs 키
    private const string GuestIdKey = "guest_user_id";


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
    // ✅ 로그인 안 되어 있으면 게스트 ID 생성/반환 (영구 보관)
    public int GetOrCreateUserId()
    {
        // 1) 로그인한 경우: 실제 프로필 ID 사용
        if (profile != null && profile.user_id > 0)
            return profile.user_id;

        // 2) 저장된 게스트 ID 있으면 사용
        int saved = PlayerPrefs.GetInt(GuestIdKey, 0);
        if (saved > 0) return saved;

        // 3) 처음이면 새 게스트 ID 생성해서 저장
        int guestId = Mathf.Abs(System.Guid.NewGuid().GetHashCode());
        PlayerPrefs.SetInt(GuestIdKey, guestId);
        PlayerPrefs.Save();
        Debug.Log($"[Session] 게스트 ID 생성: {guestId}");
        return guestId;
    }

    public bool IsGuest()
    {
        return !(profile != null && profile.user_id > 0);
    }

    // (선택) 게스트 ID 초기화하고 싶을 때 호출
    public void ResetGuestId()
    {
        PlayerPrefs.DeleteKey(GuestIdKey);
    }
}