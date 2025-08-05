using System;
using System.Collections;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier
{
    public float currentHP;
    public float maxHP;
    private float deathAnimDuration;
    public float pendingDamage = 0f;


#pragma warning disable 67
    public event Action<Transform> OnTakeDamageEffect;
    public event Action<Transform> OnDeathEffect;
    public event Action OnDeath; // 죽음 이벤트
    public event Action<float, float> OnHealthChanged;
#pragma warning restore 67

    private Coroutine damageRoutine;

    public void Awake()
    {
        CombatManager.Instance?.Register(this);
    }
    public void OnDestroy()
    {
        CombatManager.Instance?.Unregister(this);
    }


    public void Initialize(EntityData data)
    {
        this.maxHP = data.maxHP;
        currentHP = data.maxHP;

        deathAnimDuration = data.deathClip != null ? data.deathClip.length : 0f;

        if (damageRoutine == null)
            damageRoutine = StartCoroutine(ApplyDamageEndOfFrame());
    }

    public void Initialize(float hp)
    {
        maxHP = hp;               
        currentHP = hp;

        if (damageRoutine == null)
            damageRoutine = StartCoroutine(ApplyDamageEndOfFrame());
    }


    public bool IsAlive() => currentHP > 0f;
    public void RequestDamage(float dmg) => pendingDamage += dmg;


    private IEnumerator ApplyDamageEndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (!IsAlive())
            {
                StartCoroutine(DeathRoutine());
                break;
            }
        }
    }

    public void ApplyImmediateDamage()
    {
        if (pendingDamage <= 0f) return;

        OnTakeDamageEffect?.Invoke(transform);
        currentHP -= pendingDamage;
        pendingDamage = 0f;

        Debug.Log($"🩸 [HealthComponent] {gameObject.name} 현재 HP: {currentHP}");

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void ApplyPendingDamage()
    {
        if (pendingDamage > 0f && this.IsAlive())
        {
            OnTakeDamageEffect?.Invoke(this.transform);
            currentHP -= pendingDamage;
            pendingDamage = 0f;
            OnHealthChanged?.Invoke(currentHP, maxHP);


        }
    }
    private IEnumerator DeathRoutine()
    {
        Debug.Log("deathRoutine Called");
        OnDeath?.Invoke(); // 죽음 이밴트 알림
        OnDeathEffect?.Invoke(this.transform);
        yield return new WaitForSeconds(deathAnimDuration);
        gameObject.GetComponent<Entity>().UnbindEvent();

        Destroy(gameObject);
    }
    public float CurrentHp => currentHP;
    public float MaxHp => maxHP;
}

