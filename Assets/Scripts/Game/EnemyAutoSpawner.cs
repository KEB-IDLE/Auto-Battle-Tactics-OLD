using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맵의 "뒤쪽 절반(적 진영)" 그리드에 적 유닛을 자동 소환.
/// Map.cs의 hexMapSizeX, hexMapSizeZ, mapGridPositions를 그대로 사용.
/// - 소환 구역을 인스펙터에서 제한 가능(직사각형 범위 및/또는 화이트리스트)
/// - NavMesh 배치 가능 여부 선검사
/// - Gizmos로 허용/점유 슬롯 시각화
/// </summary>
public class EnemyAutoSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Map map;                         // 반드시 씬의 Map를 할당
    [SerializeField] private List<GameObject> enemyPrefabs;   // 소환할 적 유닛 프리팹 목록

    [Header("Spawn Rules")]
    [Tooltip("최대 동시 적 유닛 수(적 진영 슬롯 수를 넘기면 자연히 멈춤)")]
    [SerializeField] private int maxAlive = 8;

    [Tooltip("자동 스폰 사용 여부")]
    [SerializeField] private bool autoSpawn = true;

    [Header("Spawn Placement Source")]
    [SerializeField] private bool spawnAtSphereTriggers = true; // ← Sphere 위치 그대로 사용

    [Tooltip("첫 스폰 지연 (초)")]
    [SerializeField] private float firstDelay = 1.0f;

    [Tooltip("다음 스폰까지 간격 (초)")]
    [SerializeField] private float spawnInterval = 3.0f;

    [Tooltip("한 번에 소환할 개수(웨이브 느낌)")]
    [SerializeField] private int spawnPerTick = 1;

    [Header("Spawn Area Limit (Back Half Only)")]
    [Tooltip("뒤쪽 절반 내에서 직사각형 구역으로 제한")]
    [SerializeField] private bool limitToRect = true;

    [Tooltip("X 인덱스 범위(포함). 0 ~ Map.hexMapSizeX-1")]
    [SerializeField] private Vector2Int xRange = new Vector2Int(0, 9999); // Awake에서 실제 크기로 클램프

    [Tooltip("ZHalf 인덱스 범위(포함). 0 ~ (Map.hexMapSizeZ/2)-1")]
    [SerializeField] private Vector2Int zHalfRange = new Vector2Int(0, 9999); // Awake에서 실제 크기로 클램프

    [Tooltip("지정 슬롯만 허용(직사각형 제한과 AND 조건)")]
    [SerializeField] private List<Vector2Int> whitelistSlots = new List<Vector2Int>(); // (gx, gzHalf)

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private float gizmoRadius = 0.35f;

    // 내부 상태
    private GameObject[,] enemyGridHalf; // [x, zHalf]
    private int enemyZOffset;            // 뒤쪽 절반(zHalf -> 실제 z 변환용)

    private void Awake()
    {
        if (map == null)
        {
            Debug.LogError("[EnemyAutoSpawner] Map reference is missing.");
            enabled = false;
            return;
        }

        enemyZOffset = Map.hexMapSizeZ / 2; // 적 진영은 뒤쪽 절반
        int sizeX = Map.hexMapSizeX;
        int sizeZHalf = Map.hexMapSizeZ / 2;

        // 그리드 준비
        enemyGridHalf = new GameObject[sizeX, sizeZHalf];

        // 범위 클램프
        xRange = new Vector2Int(
            Mathf.Clamp(xRange.x, 0, sizeX - 1),
            Mathf.Clamp(xRange.y, 0, sizeX - 1)
        );
        if (xRange.x > xRange.y) { var t = xRange.x; xRange.x = xRange.y; xRange.y = t; }

        zHalfRange = new Vector2Int(
            Mathf.Clamp(zHalfRange.x, 0, sizeZHalf - 1),
            Mathf.Clamp(zHalfRange.y, 0, sizeZHalf - 1)
        );
        if (zHalfRange.x > zHalfRange.y) { var t = zHalfRange.x; zHalfRange.x = zHalfRange.y; zHalfRange.y = t; }

        // 초기값이 9999로 들어와도 자연스럽게 전체 범위가 되도록 보정
        if (xRange == new Vector2Int(0, 0)) xRange.y = sizeX - 1;
        if (zHalfRange == new Vector2Int(0, 0)) zHalfRange.y = sizeZHalf - 1;
    }

    private void Start()
    {
        if (autoSpawn)
            StartCoroutine(AutoSpawnLoop());
        CombatManager.OnRoundEnd += T;
    }

    public void T()
    {
        TrySpawnOne();
    }

    private IEnumerator AutoSpawnLoop()
    {
        yield return new WaitForSeconds(firstDelay);

        while (true)
        {
            for (int i = 0; i < spawnPerTick; i++)
                TrySpawnOne();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public bool TrySpawnOne()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("[EnemyAutoSpawner] enemyPrefabs is empty.");
            return false;
        }

        int alive = CountAlive();
        if (alive >= maxAlive) return false;

        if (!FindEmptySlot(out int gx, out int gzHalf))
            return false;

        // 1) 스폰 희망 좌표: Sphere 트리거 위치(옵션 켜짐) 또는 기존 좌표
        Vector3 desiredPos = GetSpawnWorldPos(gx, gzHalf);

        // 2) NavMesh 스냅(가능하면)
        Vector3 finalPos = desiredPos;
        bool canPlace = true;
        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        var agentOnPrefab = prefab.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agentOnPrefab != null)
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(desiredPos, out var hit, 2.0f, agentOnPrefab.areaMask))
                finalPos = hit.position;
            else
                canPlace = false;
        }

        if (!canPlace)
        {
            // 이 슬롯이 배치 불가면 한 번 더 다른 빈 슬롯을 찾아본다(선택).
            if (!FindEmptySlot(out gx, out gzHalf))
                return false;
            desiredPos = GetSpawnWorldPos(gx, gzHalf);
            finalPos = desiredPos;

            if (agentOnPrefab != null &&
                UnityEngine.AI.NavMesh.SamplePosition(desiredPos, out var hit2, 2.0f, agentOnPrefab.areaMask))
                finalPos = hit2.position;
        }

        // 3) 실제 생성/기록
        var go = Instantiate(prefab);
        go.transform.position = finalPos;                    // ★ 여기서도 헬퍼가 반영된 위치
        go.transform.rotation = Quaternion.identity;

        enemyGridHalf[gx, gzHalf] = go;

        var marker = go.AddComponent<EnemyGridMarker>();
        marker.Init(this, gx, gzHalf);

        InitializeSpawnedUnit(go);
        return true;
    }


    /// <summary>남아있는(배치된) 적 수</summary>
    private int CountAlive()
    {
        int c = 0;
        for (int x = 0; x < enemyGridHalf.GetLength(0); x++)
            for (int z = 0; z < enemyGridHalf.GetLength(1); z++)
                if (enemyGridHalf[x, z] != null) c++;
        return c;
    }

    private void InitializeSpawnedUnit(GameObject go)
    {
        // (선택) NavMesh 위로 정확히 스냅
        var agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(go.transform.position, out var hit, 2.0f, agent.areaMask))
                agent.Warp(hit.position);
            //agent.isStopped = true;
            //agent.updatePosition = true;
            //agent.updateRotation = true;

        }
        var rb = go.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX
                       | RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezePositionZ;

        // (권장) 팀/타깃팅 주입 — 네 프로젝트 API에 맞게 이름만 바꿔줘
        go.GetComponent<TeamComponent>()?.SetTeam(Team.Red);

       
    }

    private Vector3 GetSpawnWorldPos(int gx, int gzHalf)
    {
        if (spawnAtSphereTriggers && map != null && map.mapGridTriggerArray != null)
        {
            var info = map.mapGridTriggerArray[gx, gzHalf];   // TriggerInfo 컴포넌트
            if (info != null) return info.transform.position; // ★ Sphere 위치 그대로
        }

        // 폴백: 기존 방식(뒤쪽 절반 오프셋 포함)
        int gz = gzHalf + enemyZOffset;
        return map.mapGridPositions[gx, gz];
    }

    /// <summary>
    /// 직사각형 제한 + 화이트리스트를 만족하는 첫 빈 칸을 탐색.
    /// (원하면 난수 순회로 바꿔도 됨)
    /// </summary>
    private bool FindEmptySlot(out int gx, out int gzHalf)
    {
        int sizeX = enemyGridHalf.GetLength(0);
        int sizeZHalf = enemyGridHalf.GetLength(1);

        int startX = limitToRect ? xRange.x : 0;
        int endX = limitToRect ? xRange.y : (sizeX - 1);
        int startZ = limitToRect ? zHalfRange.x : 0;
        int endZ = limitToRect ? zHalfRange.y : (sizeZHalf - 1);

        for (int z = startZ; z <= endZ; z++)
        {
            for (int x = startX; x <= endX; x++)
            {
                if (enemyGridHalf[x, z] != null) continue;

                // 화이트리스트가 있으면 해당 슬롯만 허용
                if (whitelistSlots != null && whitelistSlots.Count > 0)
                {
                    bool whitelisted = false;
                    for (int i = 0; i < whitelistSlots.Count; i++)
                    {
                        var p = whitelistSlots[i];
                        if (p.x == x && p.y == z) { whitelisted = true; break; }
                    }
                    if (!whitelisted) continue;
                }

                gx = x;
                gzHalf = z;
                return true;
            }
        }

        gx = gzHalf = -1;
        return false;
    }

    /// <summary>유닛이 파괴될 때 슬롯을 비워준다.</summary>
    internal void ReleaseSlot(int gx, int gzHalf, GameObject dead)
    {
        if (gx < 0 || gzHalf < 0) return;
        if (gx >= enemyGridHalf.GetLength(0) || gzHalf >= enemyGridHalf.GetLength(1)) return;

        if (enemyGridHalf[gx, gzHalf] == dead)
            enemyGridHalf[gx, gzHalf] = null;
    }

    /// <summary>필요 시 모든 적 리셋</summary>
    public void ResetAllEnemies()
    {
        for (int x = 0; x < enemyGridHalf.GetLength(0); x++)
            for (int z = 0; z < enemyGridHalf.GetLength(1); z++)
            {
                if (enemyGridHalf[x, z] == null) continue;
                Destroy(enemyGridHalf[x, z]);
                enemyGridHalf[x, z] = null;
            }
    }

    

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        if (map == null || map.mapGridPositions == null) return;

        int sizeX = Map.hexMapSizeX;
        int sizeZHalf = Map.hexMapSizeZ / 2;

        // 현재 실행 중이 아니어도 안전하게 범위 계산
        int startX = limitToRect ? Mathf.Clamp(xRange.x, 0, sizeX - 1) : 0;
        int endX = limitToRect ? Mathf.Clamp(xRange.y, 0, sizeX - 1) : (sizeX - 1);
        int startZ = limitToRect ? Mathf.Clamp(zHalfRange.x, 0, sizeZHalf - 1) : 0;
        int endZ = limitToRect ? Mathf.Clamp(zHalfRange.y, 0, sizeZHalf - 1) : (sizeZHalf - 1);

        // 허용 슬롯: 연한 색, 점유 슬롯(플레이 모드): 진한 색
        Color allowed = new Color(0f, 1f, 0f, 0.25f);
        Color occupied = new Color(0f, 0.6f, 0f, 0.9f);

        // 허용 슬롯 그리기
        for (int z = startZ; z <= endZ; z++)
        {
            for (int x = startX; x <= endX; x++)
            {
                // 화이트리스트 체크(있다면 AND 조건)
                if (whitelistSlots != null && whitelistSlots.Count > 0)
                {
                    bool whitelisted = false;
                    for (int i = 0; i < whitelistSlots.Count; i++)
                    {
                        var p = whitelistSlots[i];
                        if (p.x == x && p.y == z) { whitelisted = true; break; }
                    }
                    if (!whitelisted) continue;
                }

                int gz = z + (Map.hexMapSizeZ / 2);
                if (gz < 0 || gz >= Map.hexMapSizeZ) continue;

                Vector3 pos = map.mapGridPositions[x, gz];
                Gizmos.color = allowed;
                Gizmos.DrawSphere(pos, gizmoRadius);

                // 점유 시(플레이 중일 때만 enemyGridHalf 참조 가능)
                if (Application.isPlaying && enemyGridHalf != null)
                {
                    if (enemyGridHalf.GetLength(0) == Map.hexMapSizeX &&
                        enemyGridHalf.GetLength(1) == (Map.hexMapSizeZ / 2))
                    {
                        if (enemyGridHalf[x, z] != null)
                        {
                            Gizmos.color = occupied;
                            Gizmos.DrawWireSphere(pos, gizmoRadius * 1.25f);
                        }
                    }
                }
            }
        }
    }
    #endregion
}


/// <summary>
/// 적 유닛 오브젝트에 붙여 슬롯 해제 전파.
/// 파괴시 OnDestroy에서 스포너에게 슬롯 반환.
/// </summary>
public class EnemyGridMarker : MonoBehaviour
{
    private EnemyAutoSpawner spawner;
    private int gx, gzHalf;

    public void Init(EnemyAutoSpawner s, int x, int zHalf)
    {
        spawner = s;
        gx = x;
        gzHalf = zHalf;
    }

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.ReleaseSlot(gx, gzHalf, this.gameObject);
    }
}
