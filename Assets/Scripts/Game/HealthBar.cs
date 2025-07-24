using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    private HealthComponent _health;

    // 초기화. Entity에서 직접 연결하거나, Find로 가져올 수 있음
    public void Initialize(HealthComponent hc)
    {
        _health = hc;
        _health.OnHealthChanged += UpdateBar;
        UpdateBar(_health.currentHP, _health.maxHP);
    }

    void UpdateBar(float cur, float max)
    {
        fillImage.fillAmount = cur / max;
    }

    void OnDestroy()
    {
        if (_health != null) _health.OnHealthChanged -= UpdateBar;
    }
}
