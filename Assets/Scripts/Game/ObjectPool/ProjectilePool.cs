using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour, IObjectPool
{
    //[SerializeField] private Projectile projectilePrefab;
    //[SerializeField] private int poolSize = 30;

    //private Queue<Projectile> pool = new Queue<Projectile>();

    //private void Awake()
    //{
    //    for (int i = 0; i < poolSize; i++)
    //    {
    //        var obj = Instantiate(projectilePrefab, transform);
    //        obj.gameObject.SetActive(false);
    //        pool.Enqueue(obj);
    //    }
    //}

    //public Projectile GetProjectile()
    //{
    //    if (pool.Count > 0)
    //    {
    //        var pobj = pool.Dequeue();
    //        pobj.gameObject.SetActive(true);
    //        return pobj;
    //    }
    //    else
    //    {
    //        var obj = Instantiate(projectilePrefab, transform);
    //        return obj;
    //    }

    //}

    //public void ReturnProjectile(Projectile obj)
    //{
    //    obj.gameObject.SetActive(false);
    //    pool.Enqueue(obj);
    //}

    [SerializeField] private string poolName;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private int poolSize = 30;

    private Queue<GameObject> pool = new Queue<GameObject>();
    public string PoolName => poolName;


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (projectilePrefab != null)
            poolName = projectilePrefab.name;
    }
#endif
    void Awake()
    {

        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(projectilePrefab, transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj.gameObject);
        }
    }

    public GameObject Get(Vector3 pos, Quaternion rot)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(projectilePrefab, transform).gameObject;
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        obj.SetActive(true);

        // Projectile 고유 초기화 필요시 여기에!

        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
