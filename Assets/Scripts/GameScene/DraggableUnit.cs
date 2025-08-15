using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// 배치된 유닛을 드래그해서 옮기고, 판매존에 올리면 판매
public class DraggableUnit : MonoBehaviour
{
    [Header("Ray & Snap")]
    [SerializeField] LayerMask groundMask;   // 평면/지형/바닥 레이어들(유닛 레이어 아님)
    [SerializeField] float rayMax = 600f;
    [SerializeField] float snapPrimary = 1.2f;
    [SerializeField] float snapFallback = 3.0f;

    Camera cam;
    bool isDragging;
    Vector2 offsetXZ;              // 마우스-유닛 간 XZ 오프셋
    Plane backupPlane;             // 지형을 못 맞출 때 쓸, 현재 높이의 수평 평면

    // ── 판매 존 캐시 ──────────────────────────────
    static RectTransform sellZone; // CardPanel
    static Canvas sellCanvas;
    static Camera uiCam;

    void OnEnable()
    {
        cam = Camera.main;
        // 유닛 현재 높이를 지나는 수평 평면
        backupPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
    }

    void OnMouseDown()
    {
        if (!GameManager2.Instance || !GameManager2.Instance.IsPlacementPhase) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        var r = cam.ScreenPointToRay(Input.mousePosition);

        // 바닥을 먼저 맞춘다(유닛 레이어가 아님)
        if (Physics.Raycast(r, out var hit, rayMax, groundMask, QueryTriggerInteraction.Ignore))
        {
            var p = hit.point;
            offsetXZ = new Vector2(transform.position.x - p.x, transform.position.z - p.z);
        }
        // 바닥을 못 맞추면 현재 높이 평면으로
        else if (backupPlane.Raycast(r, out float enter))
        {
            var p = r.GetPoint(enter);
            offsetXZ = new Vector2(transform.position.x - p.x, transform.position.z - p.z);
        }
        else return;

        isDragging = true;

        // 드래그 중에는 NavMeshAgent 비활성화(원치 않는 이동 방지)
        var agent = GetComponent<NavMeshAgent>();
        if (agent) agent.enabled = false;
    }

    void OnMouseDrag()
    {
        if (!isDragging || !GameManager2.Instance || !GameManager2.Instance.IsPlacementPhase) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        var r = cam.ScreenPointToRay(Input.mousePosition);

        // 마우스가 가리키는 바닥 위치 계산
        Vector3 desired;
        if (Physics.Raycast(r, out var hit, rayMax, groundMask, QueryTriggerInteraction.Ignore))
        {
            desired = new Vector3(
                hit.point.x + offsetXZ.x,
                transform.position.y,             // y는 나중에 NavMesh로 맞춤
                hit.point.z + offsetXZ.y          // ← Vector2는 .y 사용(이전 코드의 .z 오류 수정)
            );
        }
        else if (backupPlane.Raycast(r, out float enter))
        {
            var p = r.GetPoint(enter);
            desired = new Vector3(
                p.x + offsetXZ.x,
                transform.position.y,
                p.z + offsetXZ.y
            );
        }
        else return;

        // 해당 유닛의 AgentType으로 NavMesh 스냅
        Vector3 snapped = desired;
        var entity = GetComponent<Entity>();
        int agentTypeId = (entity && UnitManager.Instance)
            ? UnitManager.Instance.GetAgentTypeId(entity.UnitType)
            : -1;

        bool ok = false;
        NavMeshHit navHit;

        if (agentTypeId >= 0)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = NavMesh.AllAreas };
            ok = NavMesh.SamplePosition(desired, out navHit, snapPrimary, filter)
              || NavMesh.SamplePosition(desired, out navHit, snapFallback, filter);
        }
        else
        {
            ok = NavMesh.SamplePosition(desired, out navHit, snapPrimary, NavMesh.AllAreas)
              || NavMesh.SamplePosition(desired, out navHit, snapFallback, NavMesh.AllAreas);
        }

        if (ok) snapped = navHit.position;

        transform.position = snapped;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // 전투가 시작돼 있으면 NavMeshAgent 다시 활성화
        var agent = GetComponent<NavMeshAgent>();
        if (agent && GameManager2.Instance.BattleStarted) agent.enabled = true;

        // 판매존 확인
        CacheSellZone();
        if (sellZone != null &&
            RectTransformUtility.RectangleContainsScreenPoint(sellZone, Input.mousePosition, uiCam))
        {
            TrySell();
        }
    }

    // ───────────────────────── 판매 관련 ─────────────────────────
    void CacheSellZone()
    {
        if (sellZone) return;
        var go = GameObject.Find("CardPanel");
        if (!go) return;

        sellZone = go.GetComponent<RectTransform>();
        sellCanvas = go.GetComponentInParent<Canvas>();
        uiCam = (sellCanvas && sellCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
              ? null : sellCanvas ? sellCanvas.worldCamera : null;
    }

    void TrySell()
    {
        var entity = GetComponent<Entity>();
        if (!entity) return;

        var data = UnitManager.Instance?.GetEntityData(entity.UnitType);
        int price = data ? data.gold : 0;

        int cur = GoldManager.Instance?.GetCurrentGold() ?? 0;
        GoldManager.Instance?.SetGold(cur + price);

        GameManager2.Instance?.Unregister(entity);
        GameManager2.Instance?.RemoveInitMessageByUnitId(entity.UnitId);
        Destroy(gameObject);

        Debug.Log($"🪙 판매 완료: {entity.UnitType} (+{price})");
    }
}
