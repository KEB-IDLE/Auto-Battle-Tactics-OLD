using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text goldText;

    public void OnClickAddGold()
    {
        StartCoroutine(APIService.Instance.Post<UpdateGoldRequest, UpdateGoldResponse>(
            APIEndpoints.UpdateGold,
            new UpdateGoldRequest { userId = GameManager.Instance.userData.userId, amount = 1000 },
            res =>
            {
                if (res.success)
                {
                    GameManager.Instance.userData.gold = res.newGold;
                    UpdateGoldText();
                }
            }
        ));
    }

    public void UpdateGoldText()
    {
        goldText.text = "Gold: " + GameManager.Instance.userData.gold;
    }
}
