using UnityEngine;
using System.Collections;

public class UserDataLoader : MonoBehaviour
{
    public UIManager uiManager;

    public IEnumerator LoadAllUserDataCoroutine()
    {
        yield return LoadProfile();
        yield return LoadRecord();
        yield return LoadChampions();
        yield return LoadDecks();
        yield return LoadMatchHistory();

        if (uiManager != null)
        {
            uiManager.UpdateProfileUI();
        }
        else
        {
            Debug.LogWarning("ProfileIconChanger is not assigned!");
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

    public IEnumerator LoadRecord()
    {
        yield return APIService.Instance.Get<UserRecord>(
            APIEndpoints.Record,
            res => GameManager.Instance.record = res,
            err => Debug.LogError("Record load failed: " + err)
        );
    }

    IEnumerator LoadChampions()
    {
        yield return APIService.Instance.GetList<UserChampion>(
            APIEndpoints.Champion,
            res => GameManager.Instance.champions = res,
            err => Debug.LogError("Champion load failed: " + err)
        );
    }

    IEnumerator LoadDecks()
    {
        yield return APIService.Instance.GetList<UserDeck>(
            APIEndpoints.Deck,
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

[System.Serializable]
public class UserRecord
{
    public int user_id;
    public string last_login_at;
    public int rank_match_count;
    public int rank_wins;
    public int rank_losses;
    public int rank_point;
    public string tier;
    public int global_rank;
}


[System.Serializable]
public class Champion
{
    public int id;
    public string name;
    public string type;
    public byte cost;
    public int base_hp;
    public int base_attack;
}

[System.Serializable]
public class UserChampion
{
    public int user_id;
    public int champion_id;
    public Champion Champion;
}

[System.Serializable]
public class UserDeck
{
    public int id;
    public int user_id;
    public string deck_name;
    public int[] champion_ids;
}

[System.Serializable]
public class MatchHistory
{
    public int id;
    public int user_id;
    public int match_id;
    public string result;
    public string created_at;
}
