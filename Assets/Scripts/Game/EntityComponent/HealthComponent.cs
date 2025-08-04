using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier
{
    [HideInInspector] public float currentHP;
    [HideInInspector] public float maxHP;
    [HideInInspector] public float pendingDamage = 0f;
    private bool isTargetable;
    private float deathAnimDuration;

#pragma warning disable 67
    public event Action<Transform> OnTakeDamageEffect;
    public event Action<Transform> OnDeathEffect;
    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;
#pragma warning restore 67

    private Coroutine damageRoutine;

    public void Initialize(EntityData data)
    {
        maxHP = data.maxHP;
        currentHP = data.maxHP;
        deathAnimDuration = (data.deathClip != null) ? data.deathClip.length : 0f;
        isTargetable = true;
        if (damageRoutine == null)
            damageRoutine = StartCoroutine(ApplyDamageEndOfFrame());
    }

    public void Initialize(float hp)
    {
        maxHP = hp;
        currentHP = hp;
        isTargetable = true;
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
        isTargetable = false;
        var coll = GetComponent<Collider>();
        if (coll != null) coll.enabled = false;

        OnDeath?.Invoke(); // 죽음 이밴트 알림
        OnDeathEffect?.Invoke(this.transform);
        yield return new WaitForSeconds(deathAnimDuration);

        var entity = GetComponent<Entity>();
        if(entity != null)
        {
            entity.UnbindEvent();
            Destroy(gameObject);
        }
        else
        {
            var core = GetComponent<Core>();
            if (core != null)
            {
                core.UnBindEvent();
                Destroy(gameObject);
            }
        }
       
    }
    public bool IsTargetable() => isTargetable;
    public float CurrentHp => currentHP;
    public float MaxHp => maxHP;
}

