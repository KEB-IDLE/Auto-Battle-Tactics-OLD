using UnityEngine;

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
        if(data.projectileAttackingEffectPrefab != null)
            this.projectileAttackingEffect = data.projectileAttackingEffectPrefab;
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
        // 파티클/사운드 등 실행
        if (attackEffect != null && origin != null)
            Instantiate(attackEffect, transform.position, Quaternion.identity);
    }
    void PlayTakeDamageEffect(Transform origin)
    {
        if (attackEffect != null && origin != null)
            Instantiate(takeDamageEffect, transform.position, Quaternion.identity);
    }
    void PlayDeathEffect(Transform origin)
    {
        if (attackEffect != null && origin != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        Unbind();
    }
}
