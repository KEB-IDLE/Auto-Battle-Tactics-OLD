using UnityEngine;
using UnityEngine.UI;

public class ProfileIconChanger : MonoBehaviour
{
    public Image profileIconImage;      // ³»²¨
    public Sprite[] profileIcons;       // ÀüÃ¼

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

