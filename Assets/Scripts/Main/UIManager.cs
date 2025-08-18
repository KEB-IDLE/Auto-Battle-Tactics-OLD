using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainScene UI 상태들을 관리하는 싱글턴 클래스입니다.
/// 유저 프로필, 캐릭터 커스터마이징, 랭킹, 재화 등 UI 요소를 서버 데이터와 동기화하고 갱신합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // MainPanel - 유저 프로필 관련 UI 요소
    public TMP_Text nicknameText;         // 닉네임 텍스트
    public Image profileIcon;             // 프로필 아이콘 이미지
    public Sprite[] profileIcons;         // 프로필 아이콘 스프라이트 배열
    public TMP_Text levelText;            // 레벨 텍스트
    public TMP_Text goldText;             // 골드 텍스트

    // IconPanel - 프로필 아이콘 관련 UI 요소
    public Image[] iconPanelImages;                                  // 아이콘 버튼 이미지 배열
    Color enabledColor = Color.white;                           // 아이콘 활성화 색상
    Color disabledColor = new Color(1f, 1f, 1f, 80f / 255f);    // 비활성화 색상(반투명)

    // CharacterPanel - 캐릭터 관련 UI 요소
    public GameObject[] characterPrefabs;           // 캐릭터 프리팹 배열
    private GameObject currentCharacterInstance;    // 현재 씬에 생성된 캐릭터 인스턴스

    // RankingPanel - 단일 유저 랭킹 정보 UI 요소
    public Image rankProfileIcon;         // 랭킹 프로필 아이콘
    public TMP_Text rankNicknameText;     // 랭킹 닉네임 텍스트
    public TMP_Text globalRankText;       // 글로벌 랭킹 텍스트
    public TMP_Text rankMatchText;        // 매치 횟수 텍스트
    public TMP_Text rankWinsText;         // 승리 횟수 텍스트
    public TMP_Text rankPointText;        // 랭킹 점수 텍스트

    // RankingPanel - 글로벌 랭킹 리스트 UI 요소
    public TMP_Text[] rankNumberTexts;    // 순위 텍스트 배열
    public Image[] rankIcons;             // 아이콘 이미지 배열
    public TMP_Text[] rankNameTexts;      // 닉네임 텍스트 배열
    public TMP_Text[] rankPointTexts;     // 점수 텍스트 배열

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
    /// 메인 프로필 캐릭터를 서버에 저장하고 UI 갱신을 시작합니다.
    /// </summary>
    /// <param name="charId">선택한 캐릭터 ID</param>
    public void SetProfileCharacter(int charId)
    {
        StartCoroutine(UpdateProfileCharacter(charId));
    }

    /// <summary>
    /// 서버에 프로필 캐릭터 변경 요청을 보내고,
    /// 성공 시 세션 데이터와 UI 캐릭터를 갱신합니다.
    /// </summary>
    /// <param name="charId">변경할 캐릭터 ID</param>
    private IEnumerator UpdateProfileCharacter(int charId)
    {
        yield return GameAPIClient.Instance.SetProfileCharacter(
            charId,
            profile => { SessionManager.Instance.profile = profile; },
            err => Debug.LogWarning("메인 캐릭터 변경 실패: " + err)
        );

        yield return SpawnCharacter(charId);
        UpdateProfile();
    }

    /// <summary>
    /// 캐릭터 프리팹을 씬에 생성(스폰)합니다.
    /// 기존 캐릭터 인스턴스가 있으면 제거 후 새로 생성합니다.
    /// </summary>
    /// <param name="charId">생성할 캐릭터 ID</param>
    private IEnumerator SpawnCharacter(int charId)
    {
        int prefabIndex = charId - 1;

        if (prefabIndex < 0 || prefabIndex >= characterPrefabs.Length)
        {
            Debug.LogWarning("잘못된 캐릭터 ID: " + charId);
            yield break;
        }

        if (currentCharacterInstance != null)
            Destroy(currentCharacterInstance);

        GameObject prefab = characterPrefabs[prefabIndex];
        currentCharacterInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        yield return null;
    }

    /// <summary>
    /// 세션에 저장된 유저 프로필 정보를 UI 요소에 반영하여 갱신합니다.
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

    /// <summary>
    /// 프로필 아이콘 버튼 UI에서 선택된 아이콘만 강조 표시합니다.
    /// </summary>
    /// <param name="iconIndex">선택된 아이콘 인덱스</param>
    public void HighlightSelectedIcon(int iconIndex)
    {
        if (iconPanelImages == null || iconPanelImages.Length == 0) return;

        // 모든 아이콘 크기 초기화
        for (int i = 0; i < iconPanelImages.Length; i++)
        {
            iconPanelImages[i].transform.localScale = Vector3.one;
        }

        // 선택된 아이콘만 크기 확대 및 버튼 선택 상태 적용
        if (iconIndex >= 0 && iconIndex < iconPanelImages.Length)
        {
            iconPanelImages[iconIndex].transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            Button selectedBtn = iconPanelImages[iconIndex].GetComponent<Button>();
            if (selectedBtn != null)
            {
                selectedBtn.Select(); // 강제로 선택 상태로 변경
            }
        }
    }

    /// <summary>
    /// 프로필 아이콘 소유 여부에 따라 아이콘 버튼 색상을 갱신합니다.
    /// </summary>
    public void RefreshIconButton()
    {
        var ownedIcons = SessionManager.Instance.ownedProfileIcons;
        for (int i = 0; i < iconPanelImages.Length; i++)
        {
            int iconId = i + 1;
            bool owned = ownedIcons.Contains(iconId);
            iconPanelImages[i].color = owned ? enabledColor : disabledColor;
        }
    }

    /// <summary>
    /// 프로필 아이콘 버튼 클릭 시 호출됩니다.
    /// 소유한 아이콘이면 프로필 아이콘 변경, 그렇지 않으면 구매를 시도합니다.
    /// </summary>
    /// <param name="iconId">클릭한 아이콘 ID</param>
    public void OnClickIcon(int iconId)
    {
        var ownedIcons = SessionManager.Instance.ownedProfileIcons;

        if (ownedIcons.Contains(iconId))
            SetProfileIcon(iconId);
        else
            PurchaseProfileIcon(iconId);
    }

    /// <summary>
    /// 서버에 프로필 아이콘 구매 요청을 보내고 결과에 따라 UI 및 세션을 갱신합니다.
    /// </summary>
    /// <param name="iconId">구매할 아이콘 ID</param>
    public void PurchaseProfileIcon(int iconId)
    {
        StartCoroutine(GameAPIClient.Instance.PurchaseIcon(
            iconId,
            res =>
            {
                if (res.success)
                {
                    Debug.Log($"아이콘 {iconId} 구매 성공.");
                    SessionManager.Instance.profile.gold = res.gold;

                    if (!SessionManager.Instance.ownedProfileIcons.Contains(iconId))
                        SessionManager.Instance.ownedProfileIcons.Add(iconId);

                    UpdateProfile();
                }
                else
                {
                    Debug.LogWarning("구매 실패: " + res.message);
                }
            },
            err => Debug.LogError("아이콘 구매 에러: " + err)
        ));
    }

    /// <summary>
    /// 서버에 프로필 아이콘 변경 요청을 보내고 UI와 세션을 갱신합니다.
    /// </summary>
    /// <param name="iconId">변경할 아이콘 ID</param>
    public void SetProfileIcon(int iconId)
    {
        StartCoroutine(GameAPIClient.Instance.SetProfileIcon(
            iconId,
            profile =>
            {
                SessionManager.Instance.profile = profile;
                UpdateProfile();
                UpdateGlobalRanking();
            },
            err => Debug.LogWarning("프로필 아이콘 변경 실패: " + err)
        ));
    }

    /// <summary>
    /// 레벨 값을 변경 요청하고, 완료 후 UI를 갱신합니다.
    /// </summary>
    /// <param name="deltaLevel">변경할 레벨 증감량</param>
    public void ChangeLevel(int deltaLevel)
    {
        StartCoroutine(GameAPIClient.Instance.AddLevel(
            deltaLevel,
            onComplete: UpdateProfile
        ));
    }

    /// <summary>
    /// 경험치 값을 변경 요청하고, 완료 후 UI를 갱신합니다.
    /// </summary>
    /// <param name="deltaExp">변경할 경험치 증감량</param>
    public void ChangeExp(int deltaExp)
    {
        StartCoroutine(GameAPIClient.Instance.AddExp(
            deltaExp,
            onComplete: UpdateProfile
        ));
    }

    /// <summary>
    /// 골드 값을 변경 요청하고, 완료 후 UI를 갱신합니다.
    /// </summary>
    /// <param name="deltaGold">변경할 골드 증감량</param>
    public void ChangeGold(int deltaGold)
    {
        StartCoroutine(GameAPIClient.Instance.AddGold(
            deltaGold,
            onComplete: UpdateProfile
        ));
    }

    /// <summary>
    /// 랭크 전적(매치 수, 승리, 패배, 점수) 변경 요청을 서버에 보내고,
    /// 성공 시 UI와 글로벌 랭킹을 갱신합니다.
    /// </summary>
    /// <param name="deltaMatch">매치 수 증감량</param>
    /// <param name="deltaWins">승리 수 증감량</param>
    /// <param name="deltaLosses">패배 수 증감량</param>
    /// <param name="deltaPoint">랭킹 점수 증감량</param>
    public void ChangeRecord(int deltaMatch, int deltaWins, int deltaLosses, int deltaPoint)
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
                UpdateRecord();
                UpdateGlobalRanking();
            },
            err => Debug.LogWarning("랭크 전적 업데이트 실패: " + err)
        ));
    }

    /// <summary>
    /// 테스트용 버튼 함수 - 랭크 전적 업데이트 시뮬레이션
    /// </summary>
    public void TestRecord()
    {
        ChangeRecord(1, 1, 0, 10);
    }

    /// <summary>
    /// 서버에서 글로벌 랭킹 데이터를 받아와 UI를 갱신합니다.
    /// </summary>
    public void UpdateGlobalRanking()
    {
        StartCoroutine(GameAPIClient.Instance.GetGlobalRanking(
            entries =>
            {
                for (int i = 0; i < entries.Length && i < rankNumberTexts.Length; i++)
                {
                    var entry = entries[i];

                    rankNumberTexts[i].text = entry.rank.ToString();
                    rankNameTexts[i].text = entry.nickname;
                    rankPointTexts[i].text = entry.rank_point.ToString();

                    int iconIndex = entry.profile_icon_id - 1;
                    if (iconIndex >= 0 && iconIndex < profileIcons.Length)
                        rankIcons[i].sprite = profileIcons[iconIndex];
                    else
                        Debug.LogWarning($"잘못된 아이콘 ID: {entry.profile_icon_id}");
                }
            },
            err => Debug.LogError("랭킹 API 오류: " + err)
        ));
    }

    /// <summary>
    /// 랭크 전적 UI 요소(매치 수, 승리, 점수, 글로벌 랭킹 등)를 갱신합니다.
    /// </summary>
    public void UpdateRecord()
    {
        var record = SessionManager.Instance.record;
        var profile = SessionManager.Instance.profile;

        if (rankMatchText != null)
            rankMatchText.text = $"{record.rank_match_count}";
        if (rankWinsText != null)
            rankWinsText.text = $"{record.rank_wins}";
        if (rankPointText != null)
            rankPointText.text = $"{record.rank_point}";
        if (globalRankText != null)
            globalRankText.text = $"{record.global_rank}";

        if (rankNicknameText != null)
            rankNicknameText.text = profile.nickname;

        if (rankProfileIcon != null)
        {
            int iconIndex = profile.profile_icon_id - 1;
            if (iconIndex >= 0 && iconIndex < profileIcons.Length)
                rankProfileIcon.sprite = profileIcons[iconIndex];
            else
                Debug.LogWarning($"잘못된 프로필 아이콘 ID: {profile.profile_icon_id}");
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
