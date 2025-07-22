using System.Collections.Generic;
using UnityEngine;

public class EffectPoolManager : MonoBehaviour
{
    public static EffectPoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolInfo
    {
        [Tooltip("Must be exactly the same as the effect prefab's name.\n" +
             "This name is used as the key to find the pool in runtime.\n" +
             "If it does not match the prefab name, the effect may not show up.")]
        public string poolName;
        [Tooltip("The EffectPool for this effect prefab. Drag your EffectPool object here.\n" +
             "Its effect prefab's name must match the 'poolName' above.")]
        public EffectPool pool;
    }

    [SerializeField] private List<PoolInfo> pools = new List<PoolInfo>();
    private Dictionary<string, EffectPool> poolDict;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        poolDict = new Dictionary<string, EffectPool>();
        foreach (var p in pools)
        {
            if (!poolDict.ContainsKey(p.poolName))
                poolDict.Add(p.poolName, p.pool);
        }
    }

    public EffectPool GetPool(string poolName)
    {
        if (poolDict.TryGetValue(poolName, out var pool))
            return pool;
        Debug.LogError($"[EffectPoolManager] Pool '{poolName}' not found!");
        return null;
    }
}
