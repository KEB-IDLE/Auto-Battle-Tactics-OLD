using UnityEngine;

public class PooledAutoReturn : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    private float _t;

    void OnEnable() { _t = 0f; }

    void Update()
    {
        _t += Time.deltaTime;
        if (_t >= lifeTime) Return();
    }

    public void Return()
    {
        if (GamePlayController.Instance != null)
            GamePlayController.Instance.ReturnToPool(gameObject);
        else
            Destroy(gameObject);
    }
}
