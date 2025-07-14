using UnityEngine;
using System.Collections;

public class UserDataLoader : MonoBehaviour
{
    public void LoadAllUserData()
    {
        StartCoroutine(LoadProfile());
        StartCoroutine(LoadRecord());
        StartCoroutine(LoadChampions());
        StartCoroutine(LoadDecks());
        StartCoroutine(LoadMatchHistory());
    }

    IEnumerator LoadProfile()
    {
        yield return APIService.Instance.Get<UserProfile>(
            APIEndpoints.Profile,
            res => GameManager.Instance.profile = res,
            err => Debug.LogError("Profile load failed: " + err)
        );
    }

    IEnumerator LoadRecord()
    {
        yield return APIService.Instance.Get<UserRecord>(
            APIEndpoints.RankRecord,
            res => GameManager.Instance.record = res,
            err => Debug.LogError("Record load failed: " + err)
        );
    }

    IEnumerator LoadChampions()
    {
        yield return APIService.Instance.GetList<UserChampion>(
            APIEndpoints.ChampionList,
            res => GameManager.Instance.champions = res,
            err => Debug.LogError("Champion load failed: " + err)
        );
    }

    IEnumerator LoadDecks()
    {
        yield return APIService.Instance.GetList<UserDeck>(
            APIEndpoints.DeckList,
            res => GameManager.Instance.decks = res,
            err => Debug.LogError("Deck load failed: " + err)
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
