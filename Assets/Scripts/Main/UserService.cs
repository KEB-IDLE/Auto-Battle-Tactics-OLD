using System;
using System.Collections;
using UnityEngine;

public class UserService : MonoBehaviour
{
    public static UserService Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator ChangeProfileCharacter(int charId, Action<UserProfile> onSuccess, Action<string> onError)
    {
        var req = new ProfileCharacterUpdateRequest { profile_char_id = charId };

        yield return APIService.Instance.Put<ProfileCharacterUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileCharacter,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    public IEnumerator ChangeProfileIcon(int iconId, Action<UserProfile> onSuccess, Action<string> onError)
    {
        var req = new ProfileIconUpdateRequest { profile_icon_id = iconId };

        yield return APIService.Instance.Put<ProfileIconUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileIcon,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    public IEnumerator PurchaseProfileIcon(int iconId, Action<IconPurchaseResponse> onSuccess, Action<string> onError)
    {
        var req = new IconPurchaseRequest { icon_id = iconId };

        yield return APIService.Instance.Post<IconPurchaseRequest, IconPurchaseResponse>(
            APIEndpoints.ProfileIcons,
            req,
            res => onSuccess?.Invoke(res),
            err => onError?.Invoke(err)
        );
    }

    public IEnumerator ChangeLevel(int newLevel, Action<UserProfile> onSuccess, Action<string> onError)
    {
        var req = new LevelUpdateRequest { level = newLevel };

        yield return APIService.Instance.Put<LevelUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileLevel,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    public IEnumerator ChangeExp(int newExp, Action<UserProfile> onSuccess, Action<string> onError)
    {
        var req = new ExpUpdateRequest { exp = newExp };

        yield return APIService.Instance.Put<ExpUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileExp,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    public IEnumerator ChangeGold(int newGold, Action<UserProfile> onSuccess, Action<string> onError)
    {
        var req = new GoldUpdateRequest { gold = newGold };

        yield return APIService.Instance.Put<GoldUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileGold,
            req,
            res => onSuccess?.Invoke(res.data),
            err => onError?.Invoke(err)
        );
    }

    public IEnumerator UpdateUserRecord(UserRecordUpdateRequest req, Action<UserRecord> onSuccess, Action<string> onError)
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
