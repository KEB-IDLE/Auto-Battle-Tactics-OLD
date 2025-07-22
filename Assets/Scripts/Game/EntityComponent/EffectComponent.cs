using UnityEngine;
using static UnityEngine.UI.Image;

public class EffectComponent : MonoBehaviour
{
    private IEffectNotifier _notifier;
    private GameObject summonEffect;
    private GameObject attackEffect;
    private GameObject takeDamageEffect;
    private GameObject deathEffect;
    private GameObject projectileAttackingEffect;

    void Awake()
    {
        _notifier = GetComponent<IEffectNotifier>();
    }

    public void Initialize(EntityData data)
    {
        if (_notifier == null)
            Debug.LogError("_notifier is null!");

        if(data.summonEffectPrefab != null)
            this.summonEffect = data.summonEffectPrefab;
        if(data.attackEffectPrefab != null)
            this.attackEffect = data.attackEffectPrefab;
        if(data.takeDamageEffectPrefeb !=  null)
            this.takeDamageEffect = data.takeDamageEffectPrefeb;
        if(data.deathEffectPrefab != null)
            this.deathEffect = data.deathEffectPrefab;
    }


    public void PlayAttackEffect(Transform origin)
    {
        PlayEffect(attackEffect, origin, "attackEffect");
    }

    public void PlayTakeDamageEffect(Transform origin)
    {
        PlayEffect(takeDamageEffect, origin, "takeDamageEffect");
    }

    public void PlayDeathEffect(Transform origin)
    {
        PlayEffect(deathEffect, origin, "deathEffect");
    }

    private void PlayEffect(GameObject effectPrefab, Transform origin, string effectType)
    {
        if (effectPrefab != null && origin != null)
        {
            var pool = EffectPoolManager.Instance.GetPool(effectPrefab.name);
            if (pool != null)
                pool.GetEffect(origin.position, Quaternion.identity);
            else
                Debug.LogWarning($"[EffectComponent] No pool for {effectType}: {effectPrefab.name}");
        }
    }
}
