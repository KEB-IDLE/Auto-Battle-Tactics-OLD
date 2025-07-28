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

    public TMP_Text levelText;
    public TMP_Text goldText;

    public Image[] iconImages;
    Color enabledColor = Color.white;
    Color disabledColor = new Color(1f, 1f, 1f, 80f / 255f);

    // 내 랭킹
    public Image rankProfileIcon;
    public TMP_Text rankNicknameText;
    public TMP_Text globalRankText;
    public TMP_Text rankMatchText;
    public TMP_Text rankWinsText;
    public TMP_Text rankPointText;

    // 글로벌 랭킹
    public TMP_Text[] rankNumberTexts;   // 순위 텍스트
    public Image[] rankIcons;            // 아이콘
    public TMP_Text[] rankNameTexts;     // 닉네임 텍스트
    public TMP_Text[] rankPointTexts;    // 점수 텍스트




    public static UIManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnClickJoinMatch()
    {
        StartCoroutine(MatchManager.Instance.StartMatchFlow());
    }

    public void OnClickEndMatch()
    {
        StartCoroutine(MatchManager.Instance.EndMatchFlow());
    }

    // 프로필 UI 갱신
    public void UpdateProfileUI()
    {
        var profile = GameManager.Instance.profile;

        if (nicknameText != null) nicknameText.text = profile.nickname;
        if (levelText != null) levelText.text = $"Lv {profile.level}";
        if (goldText != null) goldText.text = $"{profile.gold}";

        int iconId = profile.profile_icon_id;
        int iconIndex = iconId - 1;

        if (iconIndex >= 0 && iconIndex < profileIcons.Length)
            profileIcon.sprite = profileIcons[iconIndex];
        else
            Debug.LogWarning("Invalid profile icon ID");

        RefreshIconButtonVisuals();
        HighlightSelectedIcon(iconIndex); // 선택 아이콘 강조
    }

    // 아이콘 크기 초기화 후 특정 아이콘만 강조
    private void HighlightSelectedIcon(int iconIndex)
    {
        if (iconImages == null || iconImages.Length == 0) return;

        // 이전 선택 아이콘 크기 리셋
        for (int i = 0; i < iconImages.Length; i++)
        {
            iconImages[i].transform.localScale = Vector3.one;
        }

        // 현재 선택 아이콘 확대
        if (iconIndex >= 0 && iconIndex < iconImages.Length)
        {
            iconImages[iconIndex].transform.localScale = new Vector3(1.2f, 1.2f, 1f);


            // Button의 "Selected Color" 상태 적용
            Button selectedBtn = iconImages[iconIndex].GetComponent<Button>();
            if (selectedBtn != null)
            {
                // 선택 상태를 강제로 트리거
                selectedBtn.Select();
            }
        }
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

    // 초기 아이콘 선택 반영 (UserDataLoader에서 LoadProfile 후 수동 호출)
    public void InitializeIconSelection()
    {
        int iconIndex = GameManager.Instance.profile.profile_icon_id - 1;
        HighlightSelectedIcon(iconIndex);
    }

    // 메인캐릭터 선택 버튼
    public void ChangeProfileCharacter(int charId)
    {
        StartCoroutine(UserManager.Instance.ChangeProfileCharacterCoroutine(
            charId,
            onComplete: UpdateProfileUI
        ));
    }

    // 아이콘 버튼 클릭시
    public void OnClickIcon(int iconId)
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
                GetGlobalRanking();
            },
            err => Debug.LogWarning("Profile icon update failed: " + err)
        ));
    }






    // 레벨 변경
    public void ChangeLevel(int deltaLevel)
    {
        StartCoroutine(UserManager.Instance.ChangeLevelCoroutine(
            deltaLevel,
            onComplete: UpdateProfileUI
        ));
    }

    // 경험치 변경
    public void ChangeExp(int deltaExp)
    {
        StartCoroutine(UserManager.Instance.ChangeExpCoroutine(
            deltaExp,
            onComplete: UpdateProfileUI
        ));
    }

    // 골드 변경
    public void ChangeGold(int deltaGold)
    {
        StartCoroutine(UserManager.Instance.ChangeGoldCoroutine(
            deltaGold,
            onComplete: UpdateProfileUI
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
        for (int i = 0; i < entries.Length && i < rankNumberTexts.Length; i++)
        {
            var entry = entries[i];

            // 순위, 닉네임, 포인트 각각 매핑
            rankNumberTexts[i].text = entry.rank.ToString();
            rankNameTexts[i].text = entry.nickname;
            rankPointTexts[i].text = $"{entry.rank_point} pt";

            // 아이콘 설정
            int iconIndex = entry.profile_icon_id - 1;
            if (iconIndex >= 0 && iconIndex < profileIcons.Length)
                rankIcons[i].sprite = profileIcons[iconIndex];
            else
                Debug.LogWarning($"잘못된 아이콘 ID: {entry.profile_icon_id}");
        }
    }



    // 랭크 전적 UI 갱신
    public void UpdateRecordUI()
    {
        var record = GameManager.Instance.record;
        var profile = GameManager.Instance.profile;

        // 랭크 전적 텍스트
        if (rankMatchText != null)
            rankMatchText.text = $"Matches: {record.rank_match_count}";
        if (rankWinsText != null)
            rankWinsText.text = $"Wins: {record.rank_wins}";
        if (rankPointText != null)
            rankPointText.text = $"Points: {record.rank_point}";
        if (globalRankText != null)
            globalRankText.text = $"Global Rank: {record.global_rank}";

        // 닉네임 텍스트 업데이트
        if (rankNicknameText != null)
            rankNicknameText.text = profile.nickname;

        // 프로필 아이콘 업데이트
        if (rankProfileIcon != null)
        {
            int iconIndex = profile.profile_icon_id - 1;
            if (iconIndex >= 0 && iconIndex < profileIcons.Length)
                rankProfileIcon.sprite = profileIcons[iconIndex];
            else
                Debug.LogWarning($"Invalid profile icon ID: {profile.profile_icon_id}");
        }
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
    public int rank;
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
