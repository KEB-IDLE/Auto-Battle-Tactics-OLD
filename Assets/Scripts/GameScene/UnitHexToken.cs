using UnityEngine;

public class UnitHexToken : MonoBehaviour
{
    public int cx, cz;

    // hexa 보드 점유 기록
    public void Bind(int x, int z)
    {
        cx = x; cz = z;
        HexBoard.Instance?.Occupy(cx, cz, gameObject);
    }

    // 이 유닛이 파괴될 때 점유 해제
    void OnDestroy()
    {
        HexBoard.Instance?.Release(cx, cz, gameObject);
    }
}
