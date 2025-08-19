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

        // 핵심: 디스플레이 HP는 '미리보기'로 산출 (중복 차감 방지)
        float previewHp = Mathf.Max(0f, logical_currentHP - pendingDamage);
        display_currentHP = previewHp;

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
        {
            core.UnBindEvent();
            CombatManager.EndGame();
        }


        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public void RestoreHP(float hp)
    {
        // HealthComponent는 Core.Awake()에서 이미 Initialize(max)로 초기화됨.
        // 여기서 저장된 현재 HP만 덮어씁니다.
        if (!isInitialized)
        {
            // 혹시 Initialize가 아직 안 됐다면 안전하게 초기화 상태로 전환
            logical_maxHP = Mathf.Max(logical_maxHP, hp);
            display_maxHP = logical_maxHP;
            isInitialized = true;
        }

        logical_currentHP = Mathf.Clamp(hp, 0f, logical_maxHP);
        display_currentHP = logical_currentHP;

        // UI 갱신을 위해 이벤트 쏘기
        OnHealthChanged?.Invoke(display_currentHP, display_maxHP);
    }


    public bool IsAlive() => logical_currentHP > 0f;
    public void RequestDamage(float dmg) => pendingDamage += dmg;
    public bool IsTargetable() => isTargetable;
    public float CurrentHp => logical_currentHP;
    public float MaxHp => logical_maxHP;
    public bool IsInitialized => isInitialized;

}