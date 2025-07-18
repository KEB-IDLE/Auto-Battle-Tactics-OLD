using System;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier, IEffectNotifier
{

    private float currentHP;
    
    private float deathAnimDuration = 0.5f;
    public event Action OnDeath; // 죽음 이벤트

#pragma warning disable 67
    public event Action<Transform> OnAttackEffect;
    public event Action<Transform> OnTakeDamageEffect;
    public event Action<Transform> OnDeathEffect;
#pragma warning restore 67

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
            // 여기에 피격 이펙트 추가하기.
            OnTakeDamageEffect?.Invoke(this.transform);
            currentHP -= damage; // 데미지를 받아 현재 체력 감소
            if (!IsAlive()) DeathRoutine();
        }
    }
    private IEnumerator DeathRoutine()
    {
        OnDeath?.Invoke(); // 죽음 이밴트 알림
        OnDeathEffect?.Invoke(this.transform);
        yield return new WaitForSeconds(deathAnimDuration);
        Destroy(gameObject);
    }
}
