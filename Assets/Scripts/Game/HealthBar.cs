using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    private HealthComponent _health;

    // �ʱ�ȭ. Entity���� ���� �����ϰų�, Find�� ������ �� ����
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
