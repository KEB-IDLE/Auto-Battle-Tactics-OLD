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
        GameObject obj;
        if (pool.Count > 0)
            obj = pool.Dequeue();
        else
            obj = Instantiate(effectPrefab, transform);

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        // 이펙트가 자동으로 꺼지길 원하면, 
        // 파티클 System의 Stop Action을 "Disable"로 설정하거나,
        // 아래처럼 반환 코루틴을 활용
        // StartCoroutine(AutoReturn(obj, duration));
        return obj;
    }

    public void ReturnEffect(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    // 코루틴 반환 예시 (필요하면 활성화)
    // private IEnumerator AutoReturn(GameObject obj, float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //     ReturnEffect(obj);
    // }
}
