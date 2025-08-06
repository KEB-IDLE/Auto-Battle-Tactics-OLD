using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour, IObjectPool
{

    [SerializeField] private string poolName;
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();
    public string PoolName => poolName;


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (effectPrefab != null)
            poolName = effectPrefab.name;
    }
#endif


    private void Awake()
    {
        if (effectPrefab != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                var obj = Instantiate(effectPrefab, transform);
                if (obj.GetComponent<EffectAutoReturn>() == null)
                    obj.AddComponent<EffectAutoReturn>();
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }
    }

    /// <summary>
    /// 런타임에 EffectPool을 초기화합니다.
    /// </summary>
    public void Initialize(string poolName, GameObject effectPrefab, int poolSize)
    {
        this.poolName = poolName;
        this.effectPrefab = effectPrefab;
        this.poolSize = poolSize;

        // 기존 오브젝트들 정리
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        pool.Clear();

        // 새로운 풀 생성
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(effectPrefab, transform);
            if (obj.GetComponent<EffectAutoReturn>() == null)
                obj.AddComponent<EffectAutoReturn>();
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        Debug.Log($"✨ EffectPool '{poolName}' initialized with {poolSize} objects");
    }

    // IObjectPool ����
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;
        while (pool.Count > 0)
        {
            obj = pool.Dequeue();
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

        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }

        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null || obj.Equals(null)) return;
        var particle = obj.GetComponent<ParticleSystem>();
        if (particle != null)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        obj.SetActive(false);
        pool.Enqueue(obj);
    }


}