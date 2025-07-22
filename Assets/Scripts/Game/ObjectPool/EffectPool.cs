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

        // ����Ʈ�� �ڵ����� ������ ���ϸ�, 
        // ��ƼŬ System�� Stop Action�� "Disable"�� �����ϰų�,
        // �Ʒ�ó�� ��ȯ �ڷ�ƾ�� Ȱ��
        // StartCoroutine(AutoReturn(obj, duration));
        return obj;
    }

    public void ReturnEffect(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    // �ڷ�ƾ ��ȯ ���� (�ʿ��ϸ� Ȱ��ȭ)
    // private IEnumerator AutoReturn(GameObject obj, float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //     ReturnEffect(obj);
    // }
}
