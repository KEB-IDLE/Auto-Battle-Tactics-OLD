using UnityEngine;

public class CoreComponent : MonoBehaviour
{
    // 코어가 부서지면 실행..
    private void Start()
    {
        var hp = GetComponent<HealthComponent>();
        if (hp == null)
        {
            Debug.LogError("[CoreComponent] HealthComponent 없음!");
            return;
        }

        if (hp.IsAlive())
        {
            hp.OnDeath += OnCoreDestroyed;
        }
        else
        {
            // 혹시 체력이 이미 0이면 즉시 처리
            Debug.LogWarning("[CoreComponent] 코어가 이미 죽은 상태입니다. 즉시 게임 종료 처리.");
            OnCoreDestroyed();
        }
    }
    private void OnCoreDestroyed()
    {
        // 게임 승패 처리, 코어 전용 연출 등
        Debug.Log("코어 파괴! 게임 종료 처리");
    }

}