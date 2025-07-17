using UnityEngine;

public class CoreComponent : MonoBehaviour
{
    // 코어가 부서지면 실행..

    private void Start()
    {
        var hp = GetComponent<HealthComponent>();
        hp.OnDeath += OnCoreDestroyed;
    }

    private void OnCoreDestroyed()
    {
        // 게임 승패 처리, 코어 전용 연출 등
        Debug.Log("코어 파괴! 게임 종료 처리");
    }

}
