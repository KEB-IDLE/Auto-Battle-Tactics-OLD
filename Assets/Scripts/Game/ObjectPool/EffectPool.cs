using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour, IObjectPool
{
    //[SerializeField] private GameObject effectPrefab;
    //[SerializeField] private int poolSize = 20;

    //private Queue<GameObject> pool = new Queue<GameObject>();

    //private void Awake()
    //{
    //    for (int i = 0; i < poolSize; i++)
    //    {
    //        var obj = Instantiate(effectPrefab, transform);
    //        obj.SetActive(false);
    //        pool.Enqueue(obj);
    //    }
    //}

    //public GameObject GetEffect(Vector3 position, Quaternion rotation)
    //{
    //    GameObject obj = null;
    //    while (pool.Count > 0)
    //    {
    //        obj = pool.Dequeue();

    //        if (obj != null && !obj.Equals(null)) break;
    //        else obj = null;
    //    }
    //    if (obj == null)
    //    {
    //        obj = Instantiate(effectPrefab, transform);
    //    }

    //    obj.transform.position = position;
    //    obj.transform.rotation = rotation;
    //    obj.SetActive(true);

    //    var ps = obj.GetComponent<ParticleSystem>();
    //    if (ps != null)
    //    {
    //        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    //        ps.Play(true);
    //    }

    //    return obj;
    //}

    //public void ReturnEffect(GameObject obj)
    //{
    //    if (obj == null || obj.Equals(null)) return;
    //    // ParticleSystem initialize
    //    var particle = obj.GetComponent<ParticleSystem>();
    //    if (particle != null)
    //    {
    //        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    //    }

    //    obj.SetActive(false);
    //    pool.Enqueue(obj);
    //}

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

        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(effectPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    // IObjectPool ±¸Çö
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