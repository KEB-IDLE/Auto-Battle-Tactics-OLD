using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private int poolSize = 30;

    private Queue<Projectile> pool = new Queue<Projectile>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(projectilePrefab, transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public Projectile GetProjectile()
    {
        if (pool.Count > 0)
        {
            var pobj = pool.Dequeue();
            pobj.gameObject.SetActive(true);
            return pobj;
        }
        else
        {
            // 필요시 풀을 확장 (Optional)
            var obj = Instantiate(projectilePrefab, transform);
            return obj;
        }
    }

    public void ReturnProjectile(Projectile obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
