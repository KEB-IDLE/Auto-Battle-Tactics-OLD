using UnityEngine;
using System;

public class CoreComponent : MonoBehaviour
{
    public static event Action<Team> OnAnyCoreDestroyed;

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
        Debug.Log("💥 [CoreComponent] 코어 파괴! 게임 종료 처리");

        var teamComp = GetComponent<TeamComponent>();

        Team team = teamComp.Team;
        Debug.Log($"💥 [CoreComponent] {team} 코어 파괴 → 게임 종료 처리");

        // ✅ 정적 이벤트로 외부에 알림
        OnAnyCoreDestroyed?.Invoke(team);
    }
}