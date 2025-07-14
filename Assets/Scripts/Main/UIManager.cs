using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image profileIconImage;      // 내꺼
    public Sprite[] profileIcons;       // 전체


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
            main_champion_id = 0 // 이후 실제 선택값으로 교체 가능
        };

        StartCoroutine(APIService.Instance.Put<ProfileSettingsRequest, ResponseData>(
            APIEndpoints.UpdateProfile,
            req,
            res =>
            {
                Debug.Log($"Profile icon updated to {iconId}");

                // GameManager 데이터 갱신
                GameManager.Instance.profile.profile_icon_id = iconId;

                // UI 갱신
                UpdateProfileUI();
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

