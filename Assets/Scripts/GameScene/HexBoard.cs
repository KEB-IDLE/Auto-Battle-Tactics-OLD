// Assets/Scripts/GameScene/HexBoard.cs
using UnityEngine;

public class HexBoard : MonoBehaviour
{
    public static HexBoard Instance { get; private set; }
    public Map map;                       // 씬의 Map (없어도 자동으로 찾음)

    int sizeX;                            // Map.hexMapSizeX
    int sizeZ;                            // Map.hexMapSizeZ / 2  (내 진영 절반)
    GameObject[,] occ;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        if (!map) map = FindFirstObjectByType<Map>();
        sizeX = Map.hexMapSizeX;
        sizeZ = Map.hexMapSizeZ / 2;
        occ   = new GameObject[sizeX, sizeZ];
    }

    public bool FindFirstFreeCell(out int x, out int z)
    {
        x = z = -1;
        for (int zz = 0; zz < sizeZ; zz++)
            for (int xx = 0; xx < sizeX; xx++)
                if (IsFree(xx, zz)) { x = xx; z = zz; return true; }
        return false;
    }

    public bool IsFree(int x, int z)
        => x >= 0 && x < sizeX && z >= 0 && z < sizeZ && occ[x, z] == null;

    public Vector3 CellToWorld(int x, int z)
        => map.mapGridPositions[x, z]; // hexa 중앙 좌표 그대로 사용

    public void Occupy (int x, int z, GameObject go) { occ[x, z] = go; }
    public void Release(int x, int z, GameObject go)
    { if (x>=0 && x<sizeX && z>=0 && z<sizeZ && occ[x,z]==go) occ[x,z]=null; }
}
