using UnityEngine;

public class CoreComponent : MonoBehaviour
{
    // 코어가 부서지면 실행..
    public void Initialize()
    {

    }

    public void OnCoreDestroyed()
    {
        // 게임 승패 처리, 코어 전용 연출 등
        gameObject.GetComponent<Core>().UnBindEvent();
        Debug.Log("코어 파괴! 게임 종료 처리");
    }

}