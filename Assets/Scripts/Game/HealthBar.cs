using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;

    // 초기화. Entity에서 직접 연결하거나, Find로 가져올 수 있음
    public void Initialize(HealthComponent hc)
    {
        UpdateBar(hc.currentHP, hc.maxHP);
    }

    public void UpdateBar(float cur, float max)
    {
        fillImage.fillAmount = cur / max;
    }
}
