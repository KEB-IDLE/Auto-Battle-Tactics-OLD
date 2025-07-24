using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public UserDataLoader userdataloader;

    public TMP_Text nicknameText;
    public Image profileIcon;
    public Sprite[] profileIcons;

    public GameObject[] characterPrefabs;
    private GameObject currentCharacterInstance;

    public TMP_Text levelText;
    public TMP_Text goldText;

    public Image[] iconImages;
    Color enabledColor = Color.white;
    Color disabledColor = new Color(1f, 1f, 1f, 0.1f);

    public TMP_Text rankMatchText;
    public TMP_Text rankWinsText;
    public TMP_Text rankPointText;
    public TMP_Text globalRankText;

    public TMP_Text[] rankTexts;
    public Image[] rankIcons;

    public void OnClickJoinMatch()
    {
        StartCoroutine(MatchManager.Instance.StartMatchFlow());
    }

    public void OnClickEndMatch()
    {
        StartCoroutine(MatchManager.Instance.EndMatchFlow());
    }

    // 메인캐릭터 생성
    public void SpawnCharacter(int charId)
    {
        int prefabIndex = charId - 1;

        if (prefabIndex < 0 || prefabIndex >= characterPrefabs.Length)
        {
            Debug.LogWarning("Invalid character ID: " + charId);
            return;
        }

        if (currentCharacterInstance != null)
            Destroy(currentCharacterInstance);

        GameObject prefab = characterPrefabs[prefabIndex];
        currentCharacterInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
    }

    // 메인캐릭터 선택
    public void ChangeProfileCharacter(int charId)
    {
        StartCoroutine(UserService.Instance.ChangeProfileCharacter(
            charId,
            profile =>
            {
                GameManager.Instance.profile = profile;
                UpdateProfileUI();
                SpawnCharacter(charId);
            },
            err => Debug.LogWarning("Main champion update failed: " + err)
        ));
    }

    // 프로필 아이콘 시각화
    public void RefreshIconButtonVisuals()
    {
        var ownedIcons = GameManager.Instance.ownedProfileIcons;
        for (int i = 0; i < profileIcons.Length; i++)
        {
            int iconId = i + 1;
            bool owned = ownedIcons.Contains(iconId);
            iconImages[i].color = owned ? enabledColor : disabledColor;
        }
    }

    // 아이콘 버튼 클릭시
    public void OnIconButtonClicked(int iconId)
    {
        var ownedIcons = GameManager.Instance.ownedProfileIcons;

        if (ownedIcons.Contains(iconId))
            ChangeProfileIcon(iconId);
        else
            PurchaseProfileIcon(iconId);
    }

    // 아이콘 구매 요청
    public void PurchaseProfileIcon(int iconId)
    {
        StartCoroutine(UserService.Instance.PurchaseProfileIcon(
            iconId,
            res =>
            {
                if (res.success)
                {
                    Debug.Log($"Icon {iconId} purchased successfully.");
                    GameManager.Instance.profile.gold = res.gold;

                    if (!GameManager.Instance.ownedProfileIcons.Contains(iconId))
                        GameManager.Instance.ownedProfileIcons.Add(iconId);

                    UpdateProfileUI();
                }
                else
                {
                    Debug.LogWarning("Purchase failed: " + res.message);
                }
            },
            err => Debug.LogError("Icon purchase error: " + err)
        ));
    }

    // 프로필 아이콘 변경
    public void ChangeProfileIcon(int iconId)
    {
        StartCoroutine(UserService.Instance.ChangeProfileIcon(
            iconId,
            profile =>
            {
                GameManager.Instance.profile = profile;
                UpdateProfileUI();
            },
            err => Debug.LogWarning("Profile icon update failed: " + err)
        ));
    }

    // 레벨 변경 버튼
    public void ChangeLevel(int deltaLevel)
    {
        int newLevel = GameManager.Instance.profile.level + deltaLevel;
        StartCoroutine(UserService.Instance.ChangeLevel(
            newLevel,
            profile =>
            {
                GameManager.Instance.profile = profile;
                UpdateProfileUI();
            },
            err => Debug.LogWarning("Level update failed: " + err)
        ));
    }

    // 경험치 변경 버튼
    public void ChangeExp(int deltaExp)
    {
        int newExp = GameManager.Instance.profile.exp + deltaExp;
        StartCoroutine(UserService.Instance.ChangeExp(
            newExp,
            profile =>
            {
                GameManager.Instance.profile = profile;
                UpdateProfileUI();
            },
            err => Debug.LogWarning("Exp update failed: " + err)
        ));
    }

    // 골드 변경 버튼
    public void ChangeGold(int deltaGold)
    {
        int newGold = GameManager.Instance.profile.gold + deltaGold;
        StartCoroutine(UserService.Instance.ChangeGold(
            newGold,
            profile =>
            {
                GameManager.Instance.profile = profile;
                UpdateProfileUI();
            },
            err => Debug.LogWarning("Gold update failed: " + err)
        ));
    }

    // 랭크 전적 변경 버튼
    public void UpdateUserRecord(int deltaMatch, int deltaWins, int deltaLosses, int deltaPoint)
    {
        var record = GameManager.Instance.record;

        var req = new UserRecordUpdateRequest
        {
            rank_match_count = record.rank_match_count + deltaMatch,
            rank_wins = record.rank_wins + deltaWins,
            rank_losses = record.rank_losses + deltaLosses,
            rank_point = record.rank_point + deltaPoint
        };

        StartCoroutine(UserService.Instance.UpdateUserRecord(
            req,
            updated =>
            {
                GameManager.Instance.record = updated;
                UpdateRecordUI();
                GetGlobalRanking();
            },
            err => Debug.LogWarning("Failed to update user record: " + err)
        ));
    }

    // 테스트용
    public void RecordButton()
    {
        UpdateUserRecord(1, 1, 0, 10);
    }

    // 글로벌 랭킹 조회 요청
    public void GetGlobalRanking()
    {
        StartCoroutine(UserService.Instance.GetGlobalRanking(
            entries => UpdateGlobalRankingUI(entries),
            err => Debug.LogError("Ranking API Error: " + err)
        ));
    }

    // 글로벌 랭킹 UI
    public void UpdateGlobalRankingUI(GlobalRankEntry[] entries)
    {
        for (int i = 0; i < 5; i++)
        {
            if (i >= rankTexts.Length || i >= rankIcons.Length)
            {
                Debug.LogWarning("랭킹 UI 배열이 부족합니다.");
                continue;
            }

            var entry = entries[i];
            rankTexts[i].text = $"{entry.nickname} ({entry.rank_point}pt)";

            int iconIndex = entry.profile_icon_id - 1;
            if (iconIndex >= 0 && iconIndex < profileIcons.Length)
                rankIcons[i].sprite = profileIcons[iconIndex];
            else
                Debug.LogWarning($"잘못된 아이콘 ID: {entry.profile_icon_id}");
        }
    }

    // 프로필 UI 갱신
    public void UpdateProfileUI()
    {
        var profile = GameManager.Instance.profile;

        if (nicknameText != null) nicknameText.text = profile.nickname;
        if (levelText != null) levelText.text = $"Lv {profile.level}";
        if (goldText != null) goldText.text = $"Gold {profile.gold}";

        int iconId = profile.profile_icon_id;
        int iconIndex = iconId - 1;

        if (iconIndex >= 0 && iconIndex < profileIcons.Length)
            profileIcon.sprite = profileIcons[iconIndex];
        else
            Debug.LogWarning("Invalid profile icon ID");

        RefreshIconButtonVisuals();
    }

    // 랭크 전적 UI 갱신
    public void UpdateRecordUI()
    {
        var record = GameManager.Instance.record;

        if (rankMatchText != null)
            rankMatchText.text = $"Matches: {record.rank_match_count}";
        if (rankWinsText != null)
            rankWinsText.text = $"Wins: {record.rank_wins}";
        if (rankPointText != null)
            rankPointText.text = $"Points: {record.rank_point}";
        if (globalRankText != null)
            globalRankText.text = $"Global Rank: {record.global_rank}";
    }
}


[System.Serializable]
class ProfileIconUpdateRequest
{
    public int profile_icon_id;
}

[System.Serializable]
class ProfileCharacterUpdateRequest
{
    public int profile_char_id;
}

[System.Serializable]
class LevelUpdateRequest
{
    public int level;
}

[System.Serializable]
class ExpUpdateRequest
{
    public int exp;
}

[System.Serializable]
class GoldUpdateRequest
{
    public int gold;
}

[System.Serializable]
public class UserRecordUpdateRequest
{
    public int rank_match_count;
    public int rank_wins;
    public int rank_losses;
    public int rank_point;
}

[System.Serializable]
public class UserProfileResponse
{
    public string message;
    public bool success;
    public UserProfile data;
}

[System.Serializable]
public class UserRecordResponse
{
    public bool success;
    public string message;
    public UserRecord data;
}

[System.Serializable]
public class UserProfile
{
    public int user_id;
    public string nickname;
    public int profile_icon_id;
    public int profile_char_id;
    public int level;
    public int exp;
    public int gold;
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
public class MatchHistory
{
    public int id;
    public int user_id;
    public int match_id;
    public string result;
    public string created_at;
}


[System.Serializable]
public class IconPurchaseRequest
{
    public int icon_id;
}

[System.Serializable]
public class IconPurchaseResponse
{
    public bool success;
    public string message;
    public int gold;
}

[System.Serializable]
public class GlobalRankEntry
{
    public string nickname;
    public int profile_icon_id;
    public int rank_point;
}

[System.Serializable]
public class GlobalRankingResponse
{
    public bool success;
    public GlobalRankEntry[] data;

}
