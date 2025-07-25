using UnityEngine;

public interface IObjectPool
{
    string PoolName { get; }
    GameObject Get(Vector3 pos, Quaternion rot);
    void Return(GameObject obj);
}
