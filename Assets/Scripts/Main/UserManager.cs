using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// UserManager�� ���� ���� ���� API ȣ�⸸ ����ϴ� �̱����Դϴ�.
/// </summary>
public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }

    public GameObject[] characterPrefabs;  // ����ĳ���� ������
    private GameObject currentCharacterInstance;

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

    // ===================== ������ ĳ���� ���� =====================


    /// <summary>
    /// ���� ������ ĳ���͸� ������ �����ϴ� ��û
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

    // ===================== ������ ������ ���� =====================

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

    // ===================== ������ ��ġ ���� =====================

    public IEnumerator AddLevel(int deltaLevel, Action onComplete = null)
    {
        int newLevel = SessionManager.Instance.profile.level + deltaLevel;
        yield return SetLevel(
            newLevel,
            profile => { SessionManager.Instance.profile = profile; onComplete?.Invoke(); },
            err => Debug.LogWarning("Level update failed: " + err)
        );
    }

    public IEnumerator AddExp(int deltaExp, Action onComplete = null)
    {
        int newExp = SessionManager.Instance.profile.exp + deltaExp;
        yield return SetExp(
            newExp,
            profile => { SessionManager.Instance.profile = profile; onComplete?.Invoke(); },
            err => Debug.LogWarning("Exp update failed: " + err)
        );
    }

    public IEnumerator AddGold(int deltaGold, Action onComplete = null)
    {
        int newGold = SessionManager.Instance.profile.gold + deltaGold;
        yield return SetGold(
            newGold,
            profile => { SessionManager.Instance.profile = profile; onComplete?.Invoke(); },
            err => Debug.LogWarning("Gold update failed: " + err)
        );
    }

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

    // ===================== ���� ��ŷ ���� =====================

    public IEnumerator UpdateRecord(UserRecordUpdateRequest req, Action<UserRecord> onSuccess, Action<string> onError)
    {
        yield return APIService.Instance.Put<UserRecordUpdateRequest, UserRecordResponse>(
            APIEndpoints.Record,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

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