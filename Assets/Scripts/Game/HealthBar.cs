using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;

    // �ʱ�ȭ. Entity���� ���� �����ϰų�, Find�� ������ �� ����
    public void Initialize(HealthComponent hc)
    {
        UpdateBar(hc.currentHP, hc.maxHP);
    }

    public void UpdateBar(float cur, float max)
    {
        fillImage.fillAmount = cur / max;
    }
}
