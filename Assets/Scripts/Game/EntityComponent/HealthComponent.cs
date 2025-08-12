using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier
{
    [HideInInspector] public float logical_maxHP;
    [HideInInspector] public float logical_currentHP;
    [HideInInspector] public float display_maxHP;
    [HideInInspector] public float display_currentHP;
    [HideInInspector] public float pendingDamage = 0f;
    private bool isTargetable;
    private bool isDead;
    private float deathAnimDuration;
    private bool isInitialized = false;

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
        isDead = false;
        if (damageRoutine == null)
            damageRoutine = StartCoroutine(ApplyDamageEndOfFrame());
    }

    public void Initialize(float hp)
    {
        if (isInitialized) return;

        logical_maxHP = hp;
        display_maxHP = hp;
        logical_currentHP = hp;
        display_currentHP = hp;
        isTargetable = true;
        isDead = false;
        if (damageRoutine == null)
            damageRoutine = StartCoroutine(ApplyDamageEndOfFrame());

        isInitialized = true;
    }

    private void Update()
    {
        if (!IsAlive() && isTargetable && !isDead)
        {
            isDead = true;
            StartCoroutine(DeathRoutine());
        }
    }

    private IEnumerator ApplyDamageEndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            ApplyPendingDamage();
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
        StopCoroutine(ApplyDamageEndOfFrame());
        isTargetable = false;
        var coll = GetComponent<Collider>();
        if (coll != null) coll.enabled = false;
        OnDeath?.Invoke();
        OnDeathEffect?.Invoke(this.transform);

        var entity = GetComponent<Entity>();
        var core = GetComponent<Core>();

        if (entity != null)
        {
            yield return new WaitForSeconds(deathAnimDuration);
            entity.UnbindEvent();
        }
        else if (core != null) 
            core.UnBindEvent();

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public void RestoreHP(float hp)
    {

    }

    public bool IsAlive() => logical_currentHP > 0f;
    public void RequestDamage(float dmg) => pendingDamage += dmg;
    public bool IsTargetable() => isTargetable;
    public float CurrentHp => logical_currentHP;
    public float MaxHp => logical_maxHP;
    public bool IsInitialized => isInitialized;

}