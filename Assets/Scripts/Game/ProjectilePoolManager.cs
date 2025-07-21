using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolInfo
    {
        public string poolName;           // ex: "Arrow", "Fireball"
        public ProjectilePool pool;
    }

    [SerializeField] private List<PoolInfo> pools = new List<PoolInfo>();
    private Dictionary<string, ProjectilePool> poolDict;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }


        poolDict = new Dictionary<string, ProjectilePool>();
        foreach (var p in pools)
        {
            if (!poolDict.ContainsKey(p.poolName))
                poolDict.Add(p.poolName, p.pool);
        }
    }

    public ProjectilePool GetPool(string poolName)
    {
        if (poolDict.TryGetValue(poolName, out var pool))
            return pool;
        Debug.LogError($"[ProjectilePoolManager] Pool '{poolName}' not found!");
        return null;
    }
}
