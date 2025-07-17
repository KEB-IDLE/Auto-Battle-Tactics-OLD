using UnityEngine;

public class CoreComponent : MonoBehaviour
{
    // �ھ �μ����� ����..

    private void Start()
    {
        var hp = GetComponent<HealthComponent>();
        hp.OnDeath += OnCoreDestroyed;
    }

    private void OnCoreDestroyed()
    {
        // ���� ���� ó��, �ھ� ���� ���� ��
        Debug.Log("�ھ� �ı�! ���� ���� ó��");
    }

}
