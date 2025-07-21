
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public UserDataLoader userdataloader;

    public TMP_Text nicknameText;
    public Image profileIcon;
    public Sprite[] profileIcons;

    public GameObject[] characterPrefabs; // 인스펙터에 3D 캐릭터 프리팹들 등록
    private GameObject currentCharacterInstance; // 현재 소환된 캐릭터를 추적

    public TMP_Text levelText;
    public TMP_Text goldText;

    public Image[] iconImages;           // 버튼에 붙은 Image 컴포넌트 (흐림 처리용)
    Color enabledColor = Color.white;
    Color disabledColor = new Color(1f, 1f, 1f, 0.1f);  // 반투명 (흐릿함)

    // 내꺼
    public TMP_Text rankMatchText;
    public TMP_Text rankWinsText;
    public TMP_Text rankPointText;
    public TMP_Text globalRankText;

    // 전체 랭킹
    public TMP_Text[] rankTexts;        // 닉네임 + 점수 텍스트들 (1~5위)
    public Image[] rankIcons;          // 프로필 아이콘 이미지들 (1~5위용)


    public void OnClickJoinMatch()
    {
        StartCoroutine(MatchService.Instance.JoinMatchQueue(
            () =>
            {
                Debug.Log("큐 등록 완료, 매칭 대기 시작...");
                StartCoroutine(PollMatchStatus());
            },
            err =>
            {
                Debug.LogWarning("큐 등록 실패: " + err);
            }
        ));
    }

    private IEnumerator PollMatchStatus()
    {
        float pollInterval = 2f;
        float timeout = 30f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            bool waiting = true;

            yield return MatchService.Instance.CheckMatchStatus(
                opponentId =>
                {
                    Debug.Log("매칭 성공! 상대: " + opponentId);
                    GameManager.Instance.opponentId = opponentId;
                    SceneManager.LoadScene(2);
                    waiting = false;
                },
                () =>
                {
                    Debug.Log("아직 매칭되지 않음...");
                },
                err =>
                {
                    Debug.LogWarning("매칭 상태 확인 중 오류: " + err);
                }
            );

            if (!waiting) yield break;

            yield return new WaitForSeconds(pollInterval);
            elapsed += pollInterval;
        }

        Debug.LogWarning("⏰ 매칭 시간 초과");
        // TODO: 매칭 실패 UI 띄우기 등 처리
    }




    public void SpawnCharacter(int charId)
    {
        // charId는 1부터 시작 → 배열 인덱스는 0부터 시작
        int prefabIndex = charId - 1;

        // 경계 체크
        if (prefabIndex < 0 || prefabIndex >= characterPrefabs.Length)
        {
            Debug.LogWarning("Invalid character ID: " + charId);
            return;
        }

        // 기존 캐릭터 제거
        if (currentCharacterInstance != null)
        {
            Destroy(currentCharacterInstance);
        }

        // 새로운 캐릭터 소환
        GameObject prefab = characterPrefabs[prefabIndex];
        currentCharacterInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
    }

    public void ChangeProfileCharacter(int charId)
    {
        var req = new ProfileCharacterUpdateRequest { profile_char_id = charId };

        StartCoroutine(APIService.Instance.Put<ProfileCharacterUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileCharacter,
            req,
            res =>
            {
                GameManager.Instance.profile = res.data;
                UpdateProfileUI();

                // UI 갱신 후 캐릭터 소환
                SpawnCharacter(charId);
            },
            err =>
            {
                Debug.LogWarning("Main champion update failed: " + err);
            }
        ));
    }

    public void RefreshIconButtonVisuals()
    {
        var ownedIcons = GameManager.Instance.ownedProfileIcons;
        int selectedIconId = GameManager.Instance.profile.profile_icon_id;

        for (int i = 0; i < profileIcons.Length; i++)
        {
            int iconId = i + 1;

            bool owned = ownedIcons.Contains(iconId);

            // 흐림 처리
            iconImages[i].color = owned ? enabledColor : disabledColor;
        }
    }


    public void OnIconButtonClicked(int iconId)
    {
        var ownedIcons = GameManager.Instance.ownedProfileIcons;
        Debug.Log("1");
        if (ownedIcons.Contains(iconId))
        {
            Debug.Log("2");

            // 이미 소유 중 => 프로필 아이콘 변경 API 호출
            ChangeProfileIcon(iconId);
        }
        else
        {
            // 소유하지 않은 아이콘 => 구매 API 호출
            PurchaseProfileIcon(iconId);
        }
    }

    // 아이콘 구매
    public void PurchaseProfileIcon(int iconId)
    {
        var req = new IconPurchaseRequest { icon_id = iconId };

        StartCoroutine(APIService.Instance.Post<IconPurchaseRequest, IconPurchaseResponse>(
            APIEndpoints.ProfileIcons,
            req,
            res =>
            {
                if (res.success)
                {
                    Debug.Log($"Icon {iconId} purchased successfully.");

                    // 골드 갱신
                    GameManager.Instance.profile.gold = res.gold;

                    // 아이콘 리스트에 추가 (중복 방지)
                    if (!GameManager.Instance.ownedProfileIcons.Contains(iconId))
                    {
                        GameManager.Instance.ownedProfileIcons.Add(iconId);
                    }

                    UpdateProfileUI();
                }
                else
                {
                    Debug.LogWarning("Purchase failed: " + res.message);
                }
            },
            err =>
            {
                Debug.LogError("Icon purchase error: " + err);
            }
        ));
    }

    public void UpdateProfileUI()
    {
        var profile = GameManager.Instance.profile;

        if (nicknameText != null) nicknameText.text = profile.nickname;
        if (levelText != null) levelText.text = $"Lv {profile.level}";
        if (goldText != null) goldText.text = $"Gold {profile.gold}";

        int iconId = profile.profile_icon_id;
        int iconIndex = iconId - 1;

        // 프로필 아이콘 이미지 설정
        if (iconIndex >= 0 && iconIndex < profileIcons.Length)
            profileIcon.sprite = profileIcons[iconIndex];
        else
            Debug.LogWarning("Invalid profile icon ID");

        // 아이콘 버튼 상태 갱신
        RefreshIconButtonVisuals();
    }

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



    public void ChangeProfileIcon(int iconId)
    {
        var req = new ProfileIconUpdateRequest { profile_icon_id = iconId };

        StartCoroutine(APIService.Instance.Put<ProfileIconUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileIcon,
            req,
            res =>
            {
                GameManager.Instance.profile = res.data;
                UpdateProfileUI();
            },
            err =>
            {
                Debug.LogWarning("Profile icon update failed: " + err);
            }
        ));
    }



    public void ChangeLevel(int deltaLevel)
    {
        var currentLevel = GameManager.Instance.profile.level;
        var req = new LevelUpdateRequest { level = currentLevel + deltaLevel };

        StartCoroutine(APIService.Instance.Put<LevelUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileLevel,
            req,
            res =>
            {
                GameManager.Instance.profile = res.data;
                UpdateProfileUI();
            },
            err =>
            {
                Debug.LogWarning("Level update failed: " + err);
            }
        ));
    }

    public void ChangeExp(int deltaExp)
    {
        var currentExp = GameManager.Instance.profile.exp;
        var req = new ExpUpdateRequest { exp = currentExp + deltaExp };

        StartCoroutine(APIService.Instance.Put<ExpUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileExp,
            req,
            res =>
            {
                GameManager.Instance.profile = res.data;
                UpdateProfileUI();
            },
            err =>
            {
                Debug.LogWarning("Exp update failed: " + err);
            }
        ));
    }

    public void ChangeGold(int deltaGold)
    {
        var currentGold = GameManager.Instance.profile.gold;
        var req = new GoldUpdateRequest { gold = currentGold + deltaGold };

        StartCoroutine(APIService.Instance.Put<GoldUpdateRequest, UserProfileResponse>(
            APIEndpoints.ProfileGold,
            req,
            res =>
            {
                GameManager.Instance.profile = res.data;
                UpdateProfileUI();
            },
            err =>
            {
                Debug.LogWarning("Gold update failed: " + err);
            }
        ));
    }

    public void UpdateUserRecord(int deltaMatch, int deltaWins, int deltaLosses, int deltaPoint)
    {
        var record = GameManager.Instance.record;

        int newMatchCount = record.rank_match_count + deltaMatch;
        int newWins = record.rank_wins + deltaWins;
        int newLosses = record.rank_losses + deltaLosses;
        int newPoint = record.rank_point + deltaPoint;

        var req = new UserRecordUpdateRequest
        {
            rank_match_count = newMatchCount,
            rank_wins = newWins,
            rank_losses = newLosses,
            rank_point = newPoint
        };

        StartCoroutine(APIService.Instance.Put<UserRecordUpdateRequest, UserRecordResponse>(
            APIEndpoints.Record,
            req,
            res =>
            {
                GameManager.Instance.record = res.data;

                UpdateRecordUI(); // UI 갱신
                GetGlobalRanking(); // 글로벌 랭킹 갱신
            },
            err =>
            {
                Debug.LogWarning("Failed to update user record: " + err);
            }
        ));
    }

    public void RecordButton()
    {
        UpdateUserRecord(1, 1, 0, 10);

    }

    public void GetGlobalRanking()
    {
        StartCoroutine(APIService.Instance.Get<GlobalRankingResponse>(
            APIEndpoints.GlobalRanking,
            res =>
            {
                if (res.success)
                {
                    Debug.Log("Global Ranking Received:");

                    UpdateGlobalRankingUI(res.data);
                }
                else
                {
                    Debug.LogWarning("Failed to fetch global ranking.");
                }
            },
            err =>
            {
                Debug.LogError("Ranking API Error: " + err);
            }
        ));
    }

    public void UpdateGlobalRankingUI(GlobalRankEntry[] entries)
    {
        for (int i = 0; i < 5; i++)
        {
            var entry = entries[i];

            // 안전 체크
            if (i >= rankTexts.Length || i >= rankIcons.Length)
            {
                Debug.LogWarning("랭킹 UI 배열이 부족합니다.");
                continue;
            }

            // 텍스트 설정
            rankTexts[i].text = $"{entry.nickname} ({entry.rank_point}pt)";

            // 아이콘 설정
            int iconIndex = entry.profile_icon_id - 1;
            if (iconIndex >= 0 && iconIndex < profileIcons.Length)
            {
                rankIcons[i].sprite = profileIcons[iconIndex];
            }
            else
            {
                Debug.LogWarning($"잘못된 아이콘 ID: {entry.profile_icon_id}");
            }
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
