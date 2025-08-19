using UnityEngine;

public class Temp : MonoBehaviour
{
    void Die()
    {
        UIManager.Instance.ShowGameEndPanel();
    }

    void End()
    {
        UIManager.Instance.EndGameScene();
    }
}
