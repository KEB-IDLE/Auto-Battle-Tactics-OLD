
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
}



//network
/*
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IDeathNotifier, IEffectNotifier
{

    private float currentHP;
    public float CurrentHp => currentHP;

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
*/