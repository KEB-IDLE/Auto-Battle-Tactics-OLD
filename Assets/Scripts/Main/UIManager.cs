using System.Security.Cryptography;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //public Text nicknameText;

    public Image profileIconImage;      // 내꺼
    public Sprite[] profileIcons;       // 전체

    //public Image mainChampionImage;
    //public Sprite[] championIcons; // 차후 3d object로 수정

    //public Text levelText;
    //public Text expText;
    //public Text goldText;


    public void UpdateProfileUI()
    {
        var profile = GameManager.Instance.profile;

        //// 닉네임, 레벨, 경험치, 골드 UI 갱신
        //if (nicknameText != null) nicknameText.text = profile.nickname;
        //if (levelText != null) levelText.text = $"Lv {profile.level}";
        //if (expText != null) expText.text = $"EXP {profile.exp}";
        //if (goldText != null) goldText.text = $"Gold {profile.gold}";

        // 프로필 아이콘 업데이트
        int iconId = profile.profile_icon_id;
        if (iconId >= 0 && iconId < profileIcons.Length)
        {
            profileIconImage.sprite = profileIcons[iconId];
        }
        else
        {
            Debug.LogWarning("Invalid profile icon ID");
        }

        //// 메인 챔피언 아이콘 업데이트
        //int champId = profile.main_champion_id;
        //if (champId >= 0 && champId < championIcons.Length)
        //{
        //    mainChampionImage.sprite = championIcons[champId];
        //}
        //else
        //{
        //    Debug.LogWarning("Invalid main champion ID");
        //}
    }

    public void ChangeProfileIcon(int iconId)
    {

        var req = new ProfilePatchData
        {
            profile_icon_id = iconId
        };

        StartCoroutine(APIService.Instance.Put<ProfilePatchData, UserProfile>(
            APIEndpoints.UpdateProfile,
            req,
            res =>
            {
                GameManager.Instance.profile = res;
                UpdateProfileUI();
            },
            err =>
            {
                Debug.LogWarning("Update failed: " + err);
            }
        ));
    }

}

// 임시 클래스 정의
[System.Serializable]
class ProfilePatchData
{
    public int profile_icon_id;
}
