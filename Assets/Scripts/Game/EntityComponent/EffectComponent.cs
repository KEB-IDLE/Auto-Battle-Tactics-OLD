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

    public void Bind()
    {
        _notifier.OnAttackEffect += PlayAttackEffect;
        _notifier.OnTakeDamageEffect += PlayTakeDamageEffect;
        _notifier.OnDeathEffect += PlayDeathEffect;
    }

    public void Unbind()
    {
        _notifier.OnAttackEffect -= PlayAttackEffect;
        _notifier.OnTakeDamageEffect -= PlayTakeDamageEffect;
        _notifier.OnDeathEffect -= PlayDeathEffect;
    }

    void PlayAttackEffect(Transform origin)
    {
        if (attackEffect != null && origin != null)
        {
            Debug.Log("1111111111111");
            var pool = EffectPoolManager.Instance.GetPool(attackEffect.name);
            if (pool != null)
                pool.GetEffect(origin.position, Quaternion.identity);
            else
                Debug.LogWarning("[EffectComponent] No pool for effect: " + attackEffect.name);
        }
    }
    void PlayTakeDamageEffect(Transform origin)
    {
        Debug.Log("2222222222222");
        if (takeDamageEffect != null && origin != null)
        {
            var pool = EffectPoolManager.Instance.GetPool(takeDamageEffect.name);
            if (pool != null)
                pool.GetEffect(origin.position, Quaternion.identity);
            else
                Debug.LogWarning("[EffectComponent] No pool for effect: " + takeDamageEffect.name);
        }
    }
    void PlayDeathEffect(Transform origin)
    {
        Debug.Log("33333333333333");
        if (deathEffect != null && origin != null)
        {
            var pool = EffectPoolManager.Instance.GetPool(deathEffect.name);
            if (pool != null)
                pool.GetEffect(origin.position, Quaternion.identity);
            else
                Debug.LogWarning("[EffectComponent] No pool for effect: " + deathEffect.name);
        }
        Unbind();
    }

}
