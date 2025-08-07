using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 클라이언트의 UI를 관리하는 싱글턴 클래스입니다.
/// 유저 프로필, 커스터마이징, 랭킹, 재화 등의 UI를 서버와 연동하여 갱신하거나 처리합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // MainPanel 내 프로필
    public TMP_Text nicknameText;
    public Image profileIcon;
    public Sprite[] profileIcons;
    public TMP_Text levelText;
    public TMP_Text goldText;

    // CustomPanel 아이콘
    public Image[] iconImages;
    Color enabledColor = Color.white;
    Color disabledColor = new Color(1f, 1f, 1f, 80f / 255f);

    // CustomPanel 캐릭터
    public GameObject[] characterPrefabs;
    private GameObject currentCharacterInstance;

    // RankingPanel 내 랭킹
    public Image rankProfileIcon;
    public TMP_Text rankNicknameText;
    public TMP_Text globalRankText;
    public TMP_Text rankMatchText;
    public TMP_Text rankWinsText;
    public TMP_Text rankPointText;

    // RankingPanel 글로벌 랭킹
    public TMP_Text[] rankNumberTexts;   // 순위 텍스트
    public Image[] rankIcons;            // 아이콘
    public TMP_Text[] rankNameTexts;     // 닉네임 텍스트
    public TMP_Text[] rankPointTexts;    // 점수 텍스트

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

    /// <summary>
    /// 메인 캐릭터 선택 - 서버에 저장 후 UI 갱신
    /// </summary>
    public void SetProfileCharacter(int charId)
    {
        StartCoroutine(UpdateProfileCharacter(charId));
    }

    /// <summary>
    /// 프로필 캐릭터 변경 요청 후, 세션/캐릭터/UI 갱신
    /// </summary>
    private IEnumerator UpdateProfileCharacter(int charId)
    {
        yield return GameAPIClient.Instance.SetProfileCharacter(
            charId,
            profile => { SessionManager.Instance.profile = profile; },
            err => Debug.LogWarning("Main champion update failed: " + err)
        );

        yield return SpawnCharacter(charId);
        UpdateProfile();
    }

    /// <summary>
    /// 캐릭터 프리팹을 씬에 스폰
    /// </summary>
    private IEnumerator SpawnCharacter(int charId)
    {
        int prefabIndex = charId - 1;

        if (prefabIndex < 0 || prefabIndex >= characterPrefabs.Length)
        {
            Debug.LogWarning("Invalid character ID: " + charId);
            yield break;
        }

        if (currentCharacterInstance != null)
            Destroy(currentCharacterInstance);

        GameObject prefab = characterPrefabs[prefabIndex];
        currentCharacterInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        yield return null;
    }

    /// <summary>
    /// UI 요소를 SessionManager의 유저 정보로 갱신
    /// </summary>
    public void UpdateProfile()
    {
        var profile = SessionManager.Instance.profile;

        nicknameText.text = profile.nickname;
        levelText.text = "Lv. " + profile.level.ToString();
        goldText.text = profile.gold.ToString();

        int iconId = profile.profile_icon_id - 1;
        if (iconId >= 0 && iconId < profileIcons.Length)
        {
            profileIcon.sprite = profileIcons[iconId];
        }

        HighlightSelectedIcon(SessionManager.Instance.profile.profile_icon_id - 1);
        RefreshIconButton();
    }

    // 아이콘 크기 초기화 후 특정 아이콘만 강조
    public void HighlightSelectedIcon(int iconIndex)
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
    public void RefreshIconButton()
    {
        var ownedIcons = SessionManager.Instance.ownedProfileIcons;
        for (int i = 0; i < profileIcons.Length; i++)
        {
            int iconId = i + 1;
            bool owned = ownedIcons.Contains(iconId);
            iconImages[i].color = owned ? enabledColor : disabledColor;
        }
    }

    // 아이콘 버튼 클릭시
    public void OnClickIcon(int iconId)
    {
        var ownedIcons = SessionManager.Instance.ownedProfileIcons;

        if (ownedIcons.Contains(iconId))
            SetProfileIcon(iconId);
        else
            PurchaseProfileIcon(iconId);
    }

    // 아이콘 구매 요청
    public void PurchaseProfileIcon(int iconId)
    {
        StartCoroutine(GameAPIClient.Instance.PurchaseIcon(
            iconId,
            res =>
            {
                if (res.success)
                {
                    Debug.Log($"Icon {iconId} purchased successfully.");
                    SessionManager.Instance.profile.gold = res.gold;

                    if (!SessionManager.Instance.ownedProfileIcons.Contains(iconId))
                        SessionManager.Instance.ownedProfileIcons.Add(iconId);

                    UpdateProfile();
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
    public void SetProfileIcon(int iconId)
    {
        StartCoroutine(GameAPIClient.Instance.SetProfileIcon(
            iconId,
            profile =>
            {
                SessionManager.Instance.profile = profile;
                UpdateProfile();
                GetGlobalRanking();
            },
            err => Debug.LogWarning("Profile icon update failed: " + err)
        ));
    }


    // 레벨 변경
    public void ChangeLevel(int deltaLevel)
    {
        StartCoroutine(GameAPIClient.Instance.AddLevel(
            deltaLevel,
            onComplete: UpdateProfile
        ));
    }

    // 경험치 변경
    public void ChangeExp(int deltaExp)
    {
        StartCoroutine(GameAPIClient.Instance.AddExp(
            deltaExp,
            onComplete: UpdateProfile
        ));
    }

    // 골드 변경
    public void ChangeGold(int deltaGold)
    {
        StartCoroutine(GameAPIClient.Instance.AddGold(
            deltaGold,
            onComplete: UpdateProfile
        ));
    }




    // 랭크 전적 변경 버튼
    public void UpdateUserRecord(int deltaMatch, int deltaWins, int deltaLosses, int deltaPoint)
    {
        var record = SessionManager.Instance.record;

        var req = new UserRecordUpdateRequest
        {
            rank_match_count = record.rank_match_count + deltaMatch,
            rank_wins = record.rank_wins + deltaWins,
            rank_losses = record.rank_losses + deltaLosses,
            rank_point = record.rank_point + deltaPoint
        };

        StartCoroutine(GameAPIClient.Instance.UpdateRecord(
            req,
            updated =>
            {
                SessionManager.Instance.record = updated;
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
        StartCoroutine(GameAPIClient.Instance.GetGlobalRanking(
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
            rankPointTexts[i].text = entry.rank_point.ToString();

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
        var record = SessionManager.Instance.record;
        var profile = SessionManager.Instance.profile;

        // 랭크 전적 텍스트
        if (rankMatchText != null)
            rankMatchText.text = $"{record.rank_match_count}";
        if (rankWinsText != null)
            rankWinsText.text = $"{record.rank_wins}";
        if (rankPointText != null)
            rankPointText.text = $"{record.rank_point}";
        if (globalRankText != null)
            globalRankText.text = $"{record.global_rank}";

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
