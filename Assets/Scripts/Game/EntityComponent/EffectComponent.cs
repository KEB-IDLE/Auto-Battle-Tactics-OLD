using UnityEngine;
using static UnityEngine.UI.Image;
using System.Collections;

public class EffectComponent : MonoBehaviour
{
    private GameObject summonEffect;
    private GameObject attackEffect;
    private GameObject takeDamageEffect;
    private GameObject deathEffect;
    private GameObject projectileAttackingEffect;

    public void Initialize(EntityData data)
    {
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
            var pool = ObjectPoolManager.Instance.GetPool(effectPrefab.name);

            if (pool != null)
            {
                var obj = pool.Get(origin.position, Quaternion.identity);

                var ps = obj.GetComponent<ParticleSystem>();
                float duration = ps != null ? ps.main.duration : 1f;

                //if (ps != null)
                //    StartCoroutine(ReturnEffectWhenDone(pool, obj, ps.main.duration));
                //else
                //    StartCoroutine(ReturnEffectAfterDelay(pool, obj, 1.0f)); // 기본 1초(수동 조절)

                var autoReturn = obj.GetComponent<EffectAutoReturn>();
                if (autoReturn == null)
                    autoReturn = obj.AddComponent<EffectAutoReturn>();
                autoReturn.PlayAndReturn(pool, duration);

            }
            else
            {
                Debug.LogWarning($"[EffectComponent] No pool for {effectType}: {effectPrefab.name}");
            }
        }
    }

    private IEnumerator ReturnEffectWhenDone(IObjectPool pool, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Return(obj);
    }

    private IEnumerator ReturnEffectAfterDelay(IObjectPool pool, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Return(obj);
    }

}
