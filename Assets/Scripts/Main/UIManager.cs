using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public UserDataLoader userdataloader;


    public TMP_Text nicknameText;

    public Image profileIcon;
    public Sprite[] profileIcons;

    // 3D로 구현해야함
    public Image profileCharacter;
    public Sprite[] profileCharacters;

    public TMP_Text levelText;
    // public TMP_Text expText;
    public TMP_Text goldText;

    public void UpdateProfileUI()
    {
        var profile = GameManager.Instance.profile;

        if (nicknameText != null) nicknameText.text = profile.nickname;
        if (levelText != null) levelText.text = $"Lv {profile.level}";
        // if (expText != null) expText.text = $"EXP {profile.exp}";
        if (goldText != null) goldText.text = $"Gold {profile.gold}";

        int iconId = profile.profile_icon_id;
        if (iconId >= 0 && iconId < profileIcons.Length)
        {
            profileIcon.sprite = profileIcons[iconId];
        }
        else
        {
            Debug.LogWarning("Invalid profile icon ID");
        }

        int charId = profile.profile_char_id;
        if (charId >= 0 && charId < profileCharacters.Length)
        {
            profileCharacter.sprite = profileCharacters[charId];
        }
        else
        {
            Debug.LogWarning("Invalid main champion ID");
        }
    }

    public void ChangeNickname(string newNickname)
    {
        // 별도 요청 클래스 구현 필요 시 작성
    }

    public void ChangeProfileIcon(int iconId)
    {
        var req = new ProfileIconUpdateRequest { profile_icon_id = iconId };

        StartCoroutine(APIService.Instance.Put<ProfileIconUpdateRequest, UserProfile>(
            APIEndpoints.Profile,
            req,
            res =>
            {
                GameManager.Instance.profile = res;
                UpdateProfileUI();
            },
            err =>
            {
                Debug.LogWarning("Profile icon update failed: " + err);
            }
        ));
    }

    public void ChangeProfileCharacter(int champId)
    {
        var req = new ProfileCharacterUpdateRequest { profile_char_id = champId };

        StartCoroutine(APIService.Instance.Put<ProfileCharacterUpdateRequest, UserProfile>(
            APIEndpoints.Profile,
            req,
            res =>
            {
                GameManager.Instance.profile = res;
                UpdateProfileUI();
            },
            err =>
            {
                Debug.LogWarning("Main champion update failed: " + err);
            }
        ));
    }

    public void ChangeLevel(int deltaLevel)
    {
        var currentLevel = GameManager.Instance.profile.level;
        var req = new LevelUpdateRequest { level = currentLevel + deltaLevel };

        StartCoroutine(APIService.Instance.Put<LevelUpdateRequest, UserProfile>(
            APIEndpoints.Profile,
            req,
            res =>
            {
                GameManager.Instance.profile = res;
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

        StartCoroutine(APIService.Instance.Put<ExpUpdateRequest, UserProfile>(
            APIEndpoints.Profile,
            req,
            res =>
            {
                GameManager.Instance.profile = res;
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

        StartCoroutine(APIService.Instance.Put<GoldUpdateRequest, UserProfile>(
            APIEndpoints.Profile,
            req,
            res =>
            {
                GameManager.Instance.profile = res;
                UpdateProfileUI();
            },
            err =>
            {
                Debug.LogWarning("Gold update failed: " + err);
            }
        ));
    }

    // 기록 갱신 함수
    public void UpdateUserRecord(int deltaMatch, int deltaWins, int deltaLosses, int deltaPoint)
    {
        var record = GameManager.Instance.record;

        // 변화량을 기존 값에 반영
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

        StartCoroutine(APIService.Instance.Put<UserRecordUpdateRequest, APIMessageResponse>(
            APIEndpoints.Record,
            req,
            res =>
            {
                StartCoroutine(userdataloader.LoadRecord());

                // UI 갱신 - 함수 작성 필요
                // UpdateRecordUI();
            },
            err =>
            {
                Debug.LogWarning("Failed to update user record: " + err);
            }
        ));
    }

    // 실험용
    public void RecordButton()
    {
        UpdateUserRecord(1,1,0,10);
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
    public class APIMessageResponse
    {
        public string message;
    }
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
