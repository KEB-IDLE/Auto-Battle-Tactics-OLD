using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    public void Initialize(HealthComponent hc) => UpdateBar(hc.display_currentHP, hc.display_maxHP);
    public void UpdateBar(float cur, float max) => fillImage.fillAmount = cur / max;

}
