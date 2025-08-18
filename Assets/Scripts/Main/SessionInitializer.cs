using System.Collections;
using UnityEngine;

/// <summary>
/// SessionInitializer 클래스는 게임 시작 시 필요한 유저 데이터를 서버에서 불러오는 역할을 합니다.
/// 데이터를 모두 불러온 후, UIManager를 통해 UI를 갱신하며
/// SessionManager에 데이터를 저장합니다.
/// </summary>

public class SessionInitializer : MonoBehaviour
{
    public IEnumerator LoadAllUserDataCoroutine()
    {
        yield return LoadProfile();
        yield return LoadOwnedIcons();
        yield return LoadRecord();
        //yield return LoadMatchHistory();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateProfile();

            UIManager.Instance.UpdateRecord();

            UIManager.Instance.UpdateGlobalRanking();

            UIManager.Instance.SetProfileCharacter(SessionManager.Instance.profile.profile_char_id);
        }
    }

    void Start()
    {
        StartCoroutine(LoadAllUserDataCoroutine());
    }

    IEnumerator LoadProfile()
    {
        yield return APIService.Instance.Get<UserProfile>(
            APIEndpoints.Profile,
            res => SessionManager.Instance.profile = res,
            err => Debug.LogError("Profile load failed: " + err)
        );
    }

    IEnumerator LoadOwnedIcons()
    {
        yield return APIService.Instance.GetList<int>(
            APIEndpoints.ProfileIcons,
            res => SessionManager.Instance.ownedProfileIcons = res,
            err => Debug.LogError("Owned icons load failed: " + err)
        );
    }

    public IEnumerator LoadRecord()
    {
        yield return APIService.Instance.Get<UserRecord>(
            APIEndpoints.Record,
            res => SessionManager.Instance.record = res,
            err => Debug.LogError("Record load failed: " + err)
        );
    }

    //IEnumerator LoadMatchHistory()
    //{
    //    yield return APIService.Instance.GetList<MatchHistory>(
    //        APIEndpoints.MatchHistory,
    //        res => GameManager.Instance.matchHistory = res,
    //        err => Debug.LogError("Match history load failed: " + err)
    //    );
    //}
}