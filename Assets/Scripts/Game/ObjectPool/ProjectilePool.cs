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
        if (projectilePrefab != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                var obj = Instantiate(projectilePrefab, transform);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj.gameObject);
            }
        }
    }

    /// <summary>
    /// 런타임에 ProjectilePool을 초기화합니다.
    /// </summary>
    public void Initialize(string poolName, GameObject projectilePrefab, int poolSize)
    {
        this.poolName = poolName;
        this.projectilePrefab = projectilePrefab.GetComponent<Projectile>();
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
            var obj = Instantiate(projectilePrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        Debug.Log($"🚀 ProjectilePool '{poolName}' initialized with {poolSize} objects");
    }

    public GameObject Get(Vector3 pos, Quaternion rot)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(projectilePrefab, transform).gameObject;
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        obj.SetActive(true);

        // Projectile ���� �ʱ�ȭ �ʿ�� ���⿡!

        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
