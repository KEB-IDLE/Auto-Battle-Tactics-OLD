using UnityEngine;

public class CoreComponent : MonoBehaviour
{
    // 코어가 부서지면 실행..
    public void Initialize()
    {

    }

    public void OnCoreDestroyed()
    {
        Debug.Log("코어 파괴! 게임 종료 처리");
        CombatManager.EndGame();
    }

}