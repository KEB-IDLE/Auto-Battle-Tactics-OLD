using UnityEngine;
using UnityEngine.UI;

public class ProfileIconChanger : MonoBehaviour
{
    public Image profileIconImage;      // 내꺼
    public Sprite[] profileIcons;       // 전체


    void Start()
    {
        UpdateProfileUI();
    }

    public void UpdateProfileUI()
    {
        int iconId = GameManager.Instance.profile.profile_icon_id;

        if (iconId >= 0 && iconId < profileIcons.Length)
        {
            profileIconImage.sprite = profileIcons[iconId];
        }
        else
        {
            Debug.LogWarning("Invalid profile icon ID");
        }

        // 닉네임, 레벨, 경험치 등 추가 가능
        // nicknameText.text = GameManager.Instance.profile.nickname;
    }




    public void ChangeProfileIcon(int iconId)
    {
        var req = new ProfileSettingsRequest
        {
            profile_icon_id = iconId,
            main_champion_id = 0
        };

        StartCoroutine(APIService.Instance.Put<ProfileSettingsRequest, ResponseData>(
            "/profile/settings",
            req,
            res =>
            {
                Debug.Log($"Profile icon updated to {iconId}");
                if (iconId >= 0 && iconId < profileIcons.Length)
                {
                    profileIconImage.sprite = profileIcons[iconId];
                }
            },
            err =>
            {
                Debug.LogWarning("Update failed: " + err);
            }
        ));
    }
}




[System.Serializable]
public class ProfileSettingsRequest
{
    public int profile_icon_id;
    public int main_champion_id;
}

