using System;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier
{

    private float currentHP;
    public event Action OnDeath; // 죽음 이벤트
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
        return currentHP > 0; // 현재 체력이 0보다 크면 살아있음
    }

    public void TakeDamage(float damage)
    {
        if (IsAlive())
        {
            currentHP -= damage; // 데미지를 받아 현재 체력 감소
            if (!IsAlive()) DeathRoutine();
        }
    }

    private IEnumerator DeathRoutine()
    {
        OnDeath?.Invoke(); // 죽음 이밴트 알림
        yield return new WaitForSeconds(deathAnimDuration);
        Destroy(gameObject);
    }
}
