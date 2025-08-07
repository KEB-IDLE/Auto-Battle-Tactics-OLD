using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier
{
    [HideInInspector] public float logical_maxHP;
    [HideInInspector] public float logical_currentHP;
    [HideInInspector] public float display_maxHP;
    [HideInInspector] public float display_currentHP;
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
        logical_maxHP = data.maxHP;
        display_maxHP = data.maxHP;
        logical_currentHP = data.maxHP;
        display_currentHP = data.maxHP;
        deathAnimDuration = (data.deathClip != null) ? data.deathClip.length : 0f;
        isTargetable = true;
        if (damageRoutine == null)
            damageRoutine = StartCoroutine(ApplyDamageEndOfFrame());
    }

    public void Initialize(float hp)
    {
        logical_maxHP = hp;
        display_maxHP = hp;
        logical_currentHP = hp;
        display_currentHP = hp;
        isTargetable = true;
        if (damageRoutine == null)
            damageRoutine = StartCoroutine(ApplyDamageEndOfFrame());
    }


    private IEnumerator ApplyDamageEndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            ApplyPendingDamage();
            //if (!IsAlive())
            //{
            //    StartCoroutine(DeathRoutine());
            //    break;
            //}
        }
    }

    public void ApplyImmediateDamage()
    {
        if (pendingDamage <= 0f) return;

        OnTakeDamageEffect?.Invoke(transform);
        display_currentHP -= pendingDamage;
        OnHealthChanged?.Invoke(display_currentHP, display_maxHP);
    }

    public void ApplyPendingDamage()
    {
        if (pendingDamage > 0f && this.IsAlive())
        {
            logical_currentHP -= pendingDamage;
            pendingDamage = 0f;
            if (!IsAlive())
                StartCoroutine(DeathRoutine());
        }
    }
    public IEnumerator DeathRoutine()
    {
        isTargetable = false;
        var coll = GetComponent<Collider>();
        if (coll != null) coll.enabled = false;

        Debug.Log("Ondeath");
        OnDeath?.Invoke();
        Debug.Log("Ondeath Event is Called");
        OnDeathEffect?.Invoke(this.transform);

        var entity = GetComponent<Entity>();
        if (entity != null)
        {
            yield return new WaitForSeconds(deathAnimDuration);
            entity.UnbindEvent();
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            var core = GetComponent<Core>();
            if (core != null)
            {
                core.UnBindEvent();
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }
    public void RestoreHP(float hp)
    {
        logical_currentHP = Mathf.Clamp(hp, 0, logical_maxHP);
        OnHealthChanged?.Invoke(logical_currentHP, logical_maxHP);
    }

    public bool IsAlive() => logical_currentHP > 0f;
    public void RequestDamage(float dmg) => pendingDamage += dmg;
    public bool IsTargetable() => isTargetable;
    public float CurrentHp => logical_currentHP;
    public float MaxHp => logical_maxHP;
}