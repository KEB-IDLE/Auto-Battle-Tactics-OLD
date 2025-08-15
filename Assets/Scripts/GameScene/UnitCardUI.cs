using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;

public class UnitCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("유닛 타입/프리팹")]
    public string unitType;
    public GameObject bluePrefab;
    public GameObject redPrefab;

    [Header("배치 레이어(화이트리스트)")]
    public LayerMask placementMask;                // Ground
    public float raycastMaxDistance = 500f;

    [Header("NavMesh 스냅 반경(배치씬)")]
    public float snapRadiusPrimary = 0.6f;         // 거의 제자리
    public float snapRadiusFallback = 3.0f;        // 근거리 보정

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

        // 에디터에서 비워두면 Ground 기본값
        if (placementMask.value == 0) placementMask = LayerMask.GetMask("Ground");
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

        // 1) Ground 등 허용 레이어에만 레이캐스트
        var ray = cam.ScreenPointToRay(e.position);
        if (!Physics.Raycast(ray, out var hit, raycastMaxDistance, placementMask, QueryTriggerInteraction.Ignore))
        {
            if (debugLog) Debug.LogWarning("[Place] Raycast 실패");
            return;
        }

        // 2) 프리팹의 agentTypeID 읽기
        int agentTypeId = GetAgentTypeIdFromPrefabs();

        // 3) NavMesh 스냅
        if (!TrySnapToNavMesh(hit.point, agentTypeId, out var spawnPos))
        {
            if (debugLog) Debug.LogWarning("[Place] 유효한 배치 지점 없음");
            return;
        }

        // 4) 골드 차감 (GoldManager 사용)
        var data = UnitManager.Instance.GetEntityData(unitType);
        if (data == null) return;

        if (!GoldManager.Instance.TrySpendGold(data.gold))
        {
            if (debugLog) Debug.LogWarning($"❌ 골드 부족: 필요 {data.gold}, 보유 {GoldManager.Instance.GetCurrentGold()}");
            return;
        }

        // 5) 소환
        string owner = UserNetwork.Instance?.MyId ?? "local";
        UnitManager.Instance.SpawnUnits(unitType, spawnPos, owner);

        if (debugLog) Debug.Log($"[Place] {unitType} 소환 + 골드 {data.gold} 차감 → {spawnPos}");
    }

    int GetAgentTypeIdFromPrefabs()
    {
        GameObject sample = bluePrefab ? bluePrefab : redPrefab;
        if (!sample) return -1;
        var ag = sample.GetComponent<NavMeshAgent>();
        return ag ? ag.agentTypeID : -1;
    }

    bool TrySnapToNavMesh(Vector3 from, int agentTypeId, out Vector3 pos)
    {
        // 1차: 거의 제자리
        if (agentTypeId >= 0)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = NavMesh.AllAreas };
            if (NavMesh.SamplePosition(from, out var hit, snapRadiusPrimary, filter))
            { pos = new Vector3(from.x, hit.position.y, from.z); return true; }
            if (NavMesh.SamplePosition(from, out hit, snapRadiusFallback, filter))
            { pos = hit.position; return true; }
        }
        else
        {
            if (NavMesh.SamplePosition(from, out var hit, snapRadiusPrimary, NavMesh.AllAreas))
            { pos = new Vector3(from.x, hit.position.y, from.z); return true; }
            if (NavMesh.SamplePosition(from, out hit, snapRadiusFallback, NavMesh.AllAreas))
            { pos = hit.position; return true; }
        }

        pos = default;
        return false;
    }
}
