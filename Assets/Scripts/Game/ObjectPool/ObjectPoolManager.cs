using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] private List<MonoBehaviour> pools; // EffectPool, ProjectilePool �� ȥ�� ����
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

    /// <summary>
    /// 런타임에 새로운 Projectile Pool을 등록합니다.
    /// </summary>
    public void RegisterProjectilePool(string poolName, GameObject projectilePrefab, int poolSize = 10)
    {
        if (poolDict.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool {poolName} already exists!");
            return;
        }

        // 새로운 ProjectilePool 생성
        GameObject poolObj = new GameObject($"RuntimePool_{poolName}");
        poolObj.transform.SetParent(transform);
        
        var projectilePool = poolObj.AddComponent<ProjectilePool>();
        projectilePool.Initialize(poolName, projectilePrefab, poolSize);
        
        poolDict.Add(poolName, projectilePool);
        Debug.Log($"✅ Registered ProjectilePool: {poolName}");
    }

    /// <summary>
    /// 런타임에 새로운 Effect Pool을 등록합니다.
    /// </summary>
    public void RegisterEffectPool(string poolName, GameObject effectPrefab, int poolSize = 10)
    {
        if (poolDict.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool {poolName} already exists!");
            return;
        }

        // 새로운 EffectPool 생성
        GameObject poolObj = new GameObject($"RuntimePool_{poolName}");
        poolObj.transform.SetParent(transform);
        
        var effectPool = poolObj.AddComponent<EffectPool>();
        effectPool.Initialize(poolName, effectPrefab, poolSize);
        
        poolDict.Add(poolName, effectPool);
        Debug.Log($"✅ Registered EffectPool: {poolName}");
    }

    /// <summary>
    /// 풀이 등록되어 있는지 확인합니다.
    /// </summary>
    public bool HasPool(string poolName)
    {
        return poolDict.ContainsKey(poolName);
    }
}
