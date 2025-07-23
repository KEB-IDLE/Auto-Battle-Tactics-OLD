using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour
{
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(effectPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetEffect(Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;
        while (pool.Count > 0)
        {
            obj = pool.Dequeue();
            //Debug.Log($"[EffectPool] GetEffect Dequeue: {obj?.name}, id: {obj?.GetInstanceID()}, activeSelf: {obj?.activeSelf}");
            if (obj != null && !obj.Equals(null)) break;
            else obj = null;
        }
        if (obj == null)
        {
            obj = Instantiate(effectPrefab, transform);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        // ParticleSystem 초기화 코드 추가 (필요하면)
        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }

        return obj;
    }

    public void ReturnEffect(GameObject obj)
    {
        if (obj == null || obj.Equals(null)) return;
        // ParticleSystem initialize
        var particle = obj.GetComponent<ParticleSystem>();
        if (particle != null)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}