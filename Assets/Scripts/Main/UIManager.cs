using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Text nicknameText;

    public Image profileIconImage;
    public Sprite[] profileIcons;

    // public Image mainChampionImage;
    // public Sprite[] championIcons;

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
            profileIconImage.sprite = profileIcons[iconId];
        }
        else
        {
            Debug.LogWarning("Invalid profile icon ID");
        }

        // int champId = profile.main_champion_id;
        // if (champId >= 0 && champId < championIcons.Length)
        // {
        //     mainChampionImage.sprite = championIcons[champId];
        // }
        // else
        // {
        //     Debug.LogWarning("Invalid main champion ID");
        // }
    }

    public void ChangeNickname(string newNickname)
    {
        // 별도 요청 클래스 구현 필요 시 작성
    }

    public void ChangeProfileIcon(int iconId)
    {
        var req = new ProfileUpdateRequest { profile_icon_id = iconId };

        StartCoroutine(APIService.Instance.Put<ProfileUpdateRequest, UserProfile>(
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

    public void ChangeMainChampion(int champId)
    {
        var req = new MainChampionUpdateRequest { main_champion_id = champId };

        StartCoroutine(APIService.Instance.Put<MainChampionUpdateRequest, UserProfile>(
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

}

[System.Serializable]
public class UserProfile
{
    public int user_id;
    public string nickname;
    public int profile_icon_id;
    public int main_champion_id;
    public int level;
    public int exp;
    public int gold;
}

[System.Serializable]
class ProfileUpdateRequest
{
    public int profile_icon_id;
}

[System.Serializable]
class MainChampionUpdateRequest
{
    public int main_champion_id;
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
