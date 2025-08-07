using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    private HealthComponent health;

    public void Initialize(HealthComponent hc)
    {
        if (hc == null)
        {
            Debug.LogWarning("⚠️ HealthComponent가 null입니다.");
            return;
        }

        health = hc;
        health.OnHealthChanged -= UpdateBar; // 중복 방지
        health.OnHealthChanged += UpdateBar;

        UpdateBar(health.CurrentHp, health.MaxHp);
    }

    public void UpdateBar(float cur, float max)
    {
        if (fillImage == null)
        {
            Debug.LogError("❌ fillImage가 연결되지 않았습니다! HealthBar에서 할당 확인 필요");
            return;
        }

        float ratio = Mathf.Clamp01(cur / max);
        fillImage.fillAmount = ratio;

        Debug.Log($"✅ HealthBar 갱신됨: {gameObject.name} → {ratio} ({cur}/{max})");
    }
}
