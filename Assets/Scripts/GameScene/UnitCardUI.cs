using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class UnitCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("유닛 타입/프리팹")]
    public string unitType;
    public GameObject bluePrefab;
    public GameObject redPrefab;

    [Header("레이 기본값")]
    public float raycastMaxDistance = 500f;

    [Header("NavMesh 스냅(팀 스폰 영역만)")]
    public float snapRadiusPrimary = 0.6f;
    public float snapRadiusFallback = 2.0f;

    [Header("스폰 볼륨 레이어(없어도 동작함)")]
    [SerializeField] private LayerMask spawnVolumeMask; // 비우면 "SpawnVolume" 레이어 사용

    [Header("디버그")]
    public bool debugLog = true;

    Camera cam;
    GameObject dragIcon;
    RectTransform rootCanvas;

    void Awake()
    {
        cam = Camera.main;
        var canvas = GetComponentInParent<Canvas>();
        rootCanvas = canvas ? canvas.GetComponent<RectTransform>() : null;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (!(GameManager2.Instance?.CanPlaceUnits ?? false)) return;
        if (!GameManager2.Instance.IsPlacementPhase) return;

        var srcImg = GetComponent<UnityEngine.UI.Image>();
        if (srcImg != null && rootCanvas != null)
        {
            dragIcon = new GameObject("DragIcon");
            dragIcon.transform.SetParent(rootCanvas, false);
            dragIcon.transform.SetAsLastSibling();
            var img = dragIcon.AddComponent<UnityEngine.UI.Image>();
            img.sprite = srcImg.sprite;
            img.raycastTarget = false;
            var rt = dragIcon.GetComponent<RectTransform>();
            rt.sizeDelta = srcImg.rectTransform.sizeDelta;
            rt.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (dragIcon) dragIcon.transform.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (dragIcon) Destroy(dragIcon);
        if (!(GameManager2.Instance?.CanPlaceUnits ?? false)) return;
        if (!GameManager2.Instance.IsPlacementPhase) return;
        if (!cam) cam = Camera.main;
        if (!cam) return;

        int teamMask = UnitManager.GetTeamAreaMask();
        int agentType = UnitManager.Instance?.GetAgentTypeId(unitType) ?? GetAgentTypeIdFromPrefabs();
        var ray = cam.ScreenPointToRay(e.position);
        var probe = ray.GetPoint(10f);
        DebugNavSample(probe, agentType);

        if (!TryGetSpawnPosition(ray, agentType, teamMask, out var spawnPos))
        {
            Debug.LogWarning("❌ 팀 스폰 영역 밖이거나 NavMesh 스냅 실패");
            return;
        }

        var data = UnitManager.Instance.GetEntityData(unitType);
        if (data == null) return;

        if (!GoldManager.Instance.TrySpendGold(data.gold))
        {
            if (debugLog) Debug.LogWarning($"❌ 골드 부족: 필요 {data.gold}, 보유 {GoldManager.Instance.GetCurrentGold()}");
            return;
        }

        string owner = UserNetwork.Instance?.MyId ?? "local";
        UnitManager.Instance.SpawnUnits(unitType, spawnPos, owner);
        if (debugLog) Debug.Log($"[Place] {unitType} 소환 → {spawnPos}");
    }

    // ─────────────────────────────────────────────

    static bool IsMyTeamVolume(NavMeshModifierVolume v, int teamAreaMask)
        => v != null && ((1 << v.area) & teamAreaMask) != 0;

    bool TryGetSpawnPosition(Ray ray, int agentTypeId, int teamAreaMask, out Vector3 pos)
    {
        // 인스펙터에서 비워두면 "SpawnVolume" 레이어 자동 사용
        int volMask = (spawnVolumeMask.value != 0)
            ? spawnVolumeMask.value
            : LayerMask.GetMask("SpawnVolume");

        // 1) 스폰 볼륨만 대상으로 1차 레이
        if (volMask != 0 &&
            Physics.Raycast(ray, out var hit, raycastMaxDistance, volMask, QueryTriggerInteraction.Collide))
        {
            var vol = hit.collider.GetComponentInParent<NavMeshModifierVolume>();
            if (IsMyTeamVolume(vol, teamAreaMask))
            {
                // 콜라이더 바닥 가까운 곳에서 스냅 시도
                var b = hit.collider.bounds;
                Vector3 p = hit.point;
                p.x = Mathf.Clamp(p.x, b.min.x + 0.05f, b.max.x - 0.05f);
                p.z = Mathf.Clamp(p.z, b.min.z + 0.05f, b.max.z - 0.05f);
                p.y = b.min.y + 0.2f;

                if (SampleToAnyNavMesh(p, agentTypeId, out pos)) return true;

                // 위에서 아래로 재시도(바닥 높이 오차 보정만)
                if (Physics.Raycast(new Vector3(p.x, b.max.y + 5f, p.z), Vector3.down,
                                    out var gh, 200f, ~volMask, QueryTriggerInteraction.Ignore))
                {
                    var g = gh.point + Vector3.up * 0.2f;
                    if (SampleToAnyNavMesh(gh.point + Vector3.up * 0.2f, agentTypeId, out pos)) return true;
                }
            }
        }

        // 2) 혹시 1차에서 못 맞춘 경우: RaycastAll로 "스폰볼륨만" 다시 스캔 (다른 지면/클리프 무시)
        var hits = Physics.RaycastAll(ray, raycastMaxDistance, ~0, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            var vol = h.collider.GetComponentInParent<NavMeshModifierVolume>();
            if (!IsMyTeamVolume(vol, teamAreaMask)) continue;

            var b = h.collider.bounds;
            var p = h.point;
            p.x = Mathf.Clamp(p.x, b.min.x + 0.05f, b.max.x - 0.05f);
            p.z = Mathf.Clamp(p.z, b.min.z + 0.05f, b.max.z - 0.05f);
            p.y = b.min.y + 0.2f;

            if (SampleToTeamNavMesh(p, agentTypeId, teamAreaMask, out pos)) return true;
        }

        pos = default;
        return false;
    }
    bool SampleToAnyNavMesh(Vector3 src, int agentTypeId, out Vector3 snapped)
    {
        var filter = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = NavMesh.AllAreas };
        float[] radii = { snapRadiusPrimary, snapRadiusFallback, 8f, 16f };
        foreach (var r in radii)
            if (NavMesh.SamplePosition(src, out var nh, r, filter))
            { snapped = nh.position; return true; }
        snapped = default; return false;
    }


    bool SampleToTeamNavMesh(Vector3 src, int agentTypeId, int teamAreaMask, out Vector3 snapped)
    {
        var filter = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = teamAreaMask };
        float[] radii = { snapRadiusPrimary, snapRadiusFallback, 6f };

        foreach (var r in radii)
        {
            if (NavMesh.SamplePosition(src, out var nh, r, filter))
            {
                snapped = nh.position;
                return true;
            }
        }
        snapped = default;
        return false;
    }

    int GetAgentTypeIdFromPrefabs()
    {
        GameObject sample = bluePrefab ? bluePrefab : redPrefab;
        if (!sample) return -1;
        var ag = sample.GetComponent<NavMeshAgent>();
        return ag ? ag.agentTypeID : -1;
    }
    void DebugNavSample(Vector3 p, int agentTypeId)
    {
        string[] names = { "BlueSpawn", "RedSpawn", "Walkable" };
        foreach (var nm in names)
        {
            int idx = NavMesh.GetAreaFromName(nm);
            if (idx < 0) { Debug.Log($"{nm}: ❌ 존재하지 않는 Area"); continue; }
            var f = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = 1 << idx };
            bool ok = NavMesh.SamplePosition(p, out var h, 3f, f);
            Debug.Log($"{nm}: {(ok ? "✅ OK" : "❌ NG")}");
        }
        // 모든 영역에서도 안 붙는지 확인
        var all = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = NavMesh.AllAreas };
        Debug.Log($"AllAreas: {(NavMesh.SamplePosition(p, out _, 3f, all) ? "✅" : "❌")}");
    }

}
