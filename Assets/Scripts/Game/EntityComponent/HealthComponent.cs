using System;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier
{

    private float currentHP;
    public event Action OnDeath; // ���� �̺�Ʈ
    private float deathAnimDuration = 0.5f;

    public void Initialize(EntityData data) 
    {
        currentHP = data.maxHP;
    }
    public void Initialize(float hp)
    {
        currentHP = hp;
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
            if (!IsAlive()) DeathRoutine();
        }
    }

    private IEnumerator DeathRoutine()
    {
        OnDeath?.Invoke(); // ���� �̹�Ʈ �˸�
        yield return new WaitForSeconds(deathAnimDuration);
        Destroy(gameObject);
    }
}
