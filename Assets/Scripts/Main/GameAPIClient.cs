using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 유저 관련 서버 API 요청을 처리하는 싱글턴 클래스입니다.
/// 프로필 캐릭터, 아이콘, 수치(레벨/경험치/골드), 랭킹 등의 데이터를 서버와 동기화합니다.
/// </summary>
public class GameAPIClient : MonoBehaviour
{
    public static GameAPIClient Instance { get; private set; }

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

    // ===================== 프로필 캐릭터 관련 =====================

    /// <summary>
    /// 메인 프로필 캐릭터를 서버에 저장하는 요청을 보냅니다.
    /// </summary>
    public IEnumerator SetProfileCharacter(int charId, Action<UserProfile> onSuccess, Action<string> onError)
    {
        var req = new ProfileCharacterUpdateRequest { profile_char_id = charId };

        yield return APIService.Instance.Put<ProfileCharacterUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileCharacter,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    // ===================== 프로필 아이콘 관련 =====================

    /// <summary>
    /// 프로필 아이콘을 서버에 저장하고 갱신합니다.
    /// </summary>
    public IEnumerator SetProfileIcon(int iconId, Action<UserProfile> onSuccess, Action<string> onError)
    {
        var req = new ProfileIconUpdateRequest { profile_icon_id = iconId };

        yield return APIService.Instance.Put<ProfileIconUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileIcon,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    /// <summary>
    /// 프로필 아이콘을 구매 요청합니다. 성공 시 소유 목록 및 골드를 반환합니다.
    /// </summary>
    public IEnumerator PurchaseIcon(int iconId, Action<IconPurchaseResponse> onSuccess, Action<string> onError)
    {
        var req = new IconPurchaseRequest { icon_id = iconId };

        yield return APIService.Instance.Post<IconPurchaseRequest, IconPurchaseResponse>(
            APIEndpoints.ProfileIcons,
            req,
            res => onSuccess?.Invoke(res),
            err => onError?.Invoke(err)
        );
    }

    // ===================== 프로필 수치 관련 =====================

    /// <summary>
    /// 현재 레벨에 deltaLevel을 더한 값을 서버에 저장합니다.
    /// </summary>
    public IEnumerator AddLevel(int deltaLevel, Action onComplete = null)
    {
        int newLevel = SessionManager.Instance.profile.level + deltaLevel;
        yield return SetLevel(
            newLevel,
            profile => { SessionManager.Instance.profile = profile; onComplete?.Invoke(); },
            err => Debug.LogWarning("Level update failed: " + err)
        );
    }

    /// <summary>
    /// 현재 경험치에 deltaExp를 더한 값을 서버에 저장합니다.
    /// </summary>
    public IEnumerator AddExp(int deltaExp, Action onComplete = null)
    {
        int newExp = SessionManager.Instance.profile.exp + deltaExp;
        yield return SetExp(
            newExp,
            profile => { SessionManager.Instance.profile = profile; onComplete?.Invoke(); },
            err => Debug.LogWarning("Exp update failed: " + err)
        );
    }

    /// <summary>
    /// 현재 골드에 deltaGold를 더한 값을 서버에 저장합니다.
    /// </summary>
    public IEnumerator AddGold(int deltaGold, Action onComplete = null)
    {
        int newGold = SessionManager.Instance.profile.gold + deltaGold;
        yield return SetGold(
            newGold,
            profile => { SessionManager.Instance.profile = profile; onComplete?.Invoke(); },
            err => Debug.LogWarning("Gold update failed: " + err)
        );
    }

    /// <summary>
    /// 특정 레벨 값을 서버에 저장합니다.
    /// </summary>
    public IEnumerator SetLevel(int newLevel, Action<UserProfile> onSuccess, Action<string> onError)
    {
        var req = new LevelUpdateRequest { level = newLevel };

        yield return APIService.Instance.Put<LevelUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileLevel,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    /// <summary>
    /// 특정 경험치 값을 서버에 저장합니다.
    /// </summary>
    public IEnumerator SetExp(int newExp, Action<UserProfile> onSuccess, Action<string> onError)
    {
        var req = new ExpUpdateRequest { exp = newExp };

        yield return APIService.Instance.Put<ExpUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileExp,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    /// <summary>
    /// 특정 골드 값을 서버에 저장합니다.
    /// </summary>
    public IEnumerator SetGold(int newGold, Action<UserProfile> onSuccess, Action<string> onError)
    {
        var req = new GoldUpdateRequest { gold = newGold };

        yield return APIService.Instance.Put<GoldUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileGold,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    // ===================== 유저 랭킹 관련 =====================

    /// <summary>
    /// 유저의 전적(승리 수, 랭크 점수 등)을 서버에 업데이트합니다.
    /// </summary>
    public IEnumerator UpdateRecord(UserRecordUpdateRequest req, Action<UserRecord> onSuccess, Action<string> onError)
    {
        yield return APIService.Instance.Put<UserRecordUpdateRequest, UserRecordResponse>(
            APIEndpoints.Record,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    /// <summary>
    /// 글로벌 랭킹 상위 유저 목록을 서버에서 받아옵니다.
    /// </summary>
    public IEnumerator GetGlobalRanking(Action<GlobalRankEntry[]> onSuccess, Action<string> onError)
    {
        yield return APIService.Instance.Get<GlobalRankingResponse>(
            APIEndpoints.GlobalRanking,
            res =>
            {
                if (res.success) onSuccess?.Invoke(res.data);
                else onError?.Invoke("Fetch failed");
            },
            err => onError?.Invoke(err)
        );
    }
}
