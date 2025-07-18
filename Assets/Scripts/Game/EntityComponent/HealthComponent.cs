using System;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier, IEffectNotifier
{

    private float currentHP;
    
    private float deathAnimDuration = 0.5f;
    public event Action OnDeath; // ���� �̺�Ʈ

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
        return currentHP > 0; // ���� ü���� 0���� ũ�� �������
    }

    public void TakeDamage(float damage)
    {
        if (IsAlive())
        {
            // ���⿡ �ǰ� ����Ʈ �߰��ϱ�.
            OnTakeDamageEffect?.Invoke(this.transform);
            currentHP -= damage; // �������� �޾� ���� ü�� ����
            if (!IsAlive()) DeathRoutine();
        }
    }
    private IEnumerator DeathRoutine()
    {
        OnDeath?.Invoke(); // ���� �̹�Ʈ �˸�
        OnDeathEffect?.Invoke(this.transform);
        yield return new WaitForSeconds(deathAnimDuration);
        Destroy(gameObject);
    }
}
