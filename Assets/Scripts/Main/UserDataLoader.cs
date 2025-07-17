using System.Collections;
using UnityEngine;

public class UserDataLoader : MonoBehaviour
{
    public UIManager uiManager;

    public IEnumerator LoadAllUserDataCoroutine()
    {
        yield return LoadProfile();
        yield return LoadOwnedIcons();  // 추가
        yield return LoadRecord();
        yield return LoadMatchHistory();

        if (uiManager != null)
        {
            uiManager.UpdateProfileUI();
            uiManager.UpdateRecordUI();

            uiManager.GetGlobalRanking();

            uiManager.SpawnCharacter(GameManager.Instance.profile.profile_char_id);
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
            res => GameManager.Instance.profile = res,
            err => Debug.LogError("Profile load failed: " + err)
        );
    }

    //새로 추가한 유저가 소유한 아이콘 리스트 로드
    IEnumerator LoadOwnedIcons()
    {
        yield return APIService.Instance.GetList<int>(  // int 리스트로 받는다고 가정 (icon_id 리스트)
            APIEndpoints.ProfileIcons,
            res => GameManager.Instance.ownedProfileIcons = res,
            err => Debug.LogError("Owned icons load failed: " + err)
        );
    }

    public IEnumerator LoadRecord()
    {
        yield return APIService.Instance.Get<UserRecord>(
            APIEndpoints.Record,
            res => GameManager.Instance.record = res,
            err => Debug.LogError("Record load failed: " + err)
        );
    }

    IEnumerator LoadMatchHistory()
    {
        yield return APIService.Instance.GetList<MatchHistory>(
            APIEndpoints.MatchHistory,
            res => GameManager.Instance.matchHistory = res,
            err => Debug.LogError("Match history load failed: " + err)
        );
    }
}
