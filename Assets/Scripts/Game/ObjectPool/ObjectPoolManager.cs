using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] private List<MonoBehaviour> pools; // EffectPool, ProjectilePool µî È¥ÇÕ °¡´É
    private Dictionary<string, IObjectPool> poolDict = new Dictionary<string, IObjectPool>();

    void Awake()
    {
        Instance = this;
        foreach (var pool in pools)
        {
            if (pool is IObjectPool iPool)
                poolDict.Add(iPool.PoolName, iPool);
        }
    }

    public GameObject Get(string poolName, Vector3 pos, Quaternion rot)
    {
        if (poolDict.TryGetValue(poolName, out var pool))
            return pool.Get(pos, rot);
        Debug.LogError($"Pool {poolName} not found");
        return null;
    }

    public IObjectPool GetPool(string poolName)
    {
        if (poolDict.TryGetValue(poolName, out var pool))
            return pool;
        return null;
    }

    public void Return(string poolName, GameObject obj)
    {
        if (poolDict.TryGetValue(poolName, out var pool))
            pool.Return(obj);
        else
            Destroy(obj);
    }
}
