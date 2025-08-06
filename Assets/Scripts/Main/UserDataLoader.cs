using System.Collections;
using UnityEngine;

public class UserDataLoader : MonoBehaviour
{
    public UIManager uiManager;

    public IEnumerator LoadAllUserDataCoroutine()
    {
        yield return LoadProfile();
        yield return LoadOwnedIcons();
        yield return LoadRecord();
        //yield return LoadMatchHistory();

        if (uiManager != null)
        {
            uiManager.UpdateProfileUI();
            uiManager.UpdateRecordUI();

            uiManager.InitializeIconSelection();

            uiManager.GetGlobalRanking();

            StartCoroutine(UserManager.Instance.SpawnCharacter(SessionManager.Instance.profile.profile_char_id));
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