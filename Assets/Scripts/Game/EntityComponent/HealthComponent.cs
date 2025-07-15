using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier
{

    private float currentHP;
    public event Action OnDeath; // ���� �̺�Ʈ

    public void Initialize(EntityData data) 
    {
        currentHP = data.maxHP;
    }

    public bool IsAlive()
    {
        return currentHP > 0; // ���� ü���� 0���� ũ�� �������
    }

    public void TakeDamage(float damage)
    {
        if (IsAlive())
        {
            currentHP -= damage; // �������� �޾� ���� ü�� ����
            if (!IsAlive()) Die();
        }
    }

    public void Die()
    {
        if(IsAlive()) return;
        OnDeath?.Invoke(); // ���� �̹�Ʈ �˸�
        return;
    }

}
