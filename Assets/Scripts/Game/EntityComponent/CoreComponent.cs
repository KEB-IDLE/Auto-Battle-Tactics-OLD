using UnityEngine;
using System;

public class CoreComponent : MonoBehaviour
{
    public static event Action<Team> OnAnyCoreDestroyed;

    // 코어가 부서지면 실행..
    public void Initialize()
    {

    }

    public void OnCoreDestroyed()
    {
        Debug.Log("💥 [CoreComponent] 코어 파괴! 게임 종료 처리");
        CombatManager.EndGame();
        var teamComp = GetComponent<TeamComponent>();

        Team team = teamComp.Team;
        Debug.Log($"💥 [CoreComponent] {team} 코어 파괴 → 게임 종료 처리");

        // ✅ 정적 이벤트로 외부에 알림
        OnAnyCoreDestroyed?.Invoke(team);
    }
}