using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier
{

    private float currentHP;
    public event Action OnDeath; // 죽음 이벤트

    public void Initialize(EntityData data) 
    {
        currentHP = data.maxHP;
    }

    public bool IsAlive()
    {
        return currentHP > 0; // 현재 체력이 0보다 크면 살아있음
    }

    public void TakeDamage(float damage)
    {
        if (IsAlive())
        {
            currentHP -= damage; // 데미지를 받아 현재 체력 감소
            if (!IsAlive()) Die();
        }
    }

    public void Die()
    {
        if(IsAlive()) return;
        OnDeath?.Invoke(); // 죽음 이밴트 알림
        return;
    }

}
