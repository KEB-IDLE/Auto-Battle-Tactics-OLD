// DraggableUnit.cs
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class DraggableUnit : MonoBehaviour
{
    [Header("Pick")]
    [SerializeField] LayerMask pickableLayers = ~0;   // 유닛 레이어 선택
    [SerializeField] float pickRayDistance = 500f;

    [Header("Ray & Snap")]
    [SerializeField] float snapPrimary = 1.2f;
    [SerializeField] float snapFallback = 3f;
    [SerializeField] bool debugLog = false;

    [Tooltip("드래그 투영 평면의 Y. NaN이면 드래그 시작 시 유닛 Y 사용")]
    [SerializeField] float placementPlaneY = float.NaN;

    [Header("UI")]
    [SerializeField] string sellPanelName = "CardPanel";

    Camera cam;

    // ── 드래그 상태 ───────────────────────────────
    bool isDragging;
    Transform target;
    Entity targetEntity;
    NavMeshAgent targetAgent;
    Rigidbody targetRb;

    Vector2 offsetXZ;
    float dragDepth;
    Vector3 lastValidPos;
    bool agentWasEnabled;
    bool rbWasKinematic;

    // ── 판매 존 캐시 ──────────────────────────────
    static RectTransform sellZone;
    static Canvas sellCanvas;
    static Camera uiCam;

    void Awake() { cam = Camera.main; }

    void Update()
    {
        // 배치 단계 아닐 땐 드래그 강제 종료
        if (!GameManager2.Instance || !GameManager2.Instance.IsPlacementPhase)
        {
            if (isDragging) CancelDrag();
            return;
        }

        if (Input.GetMouseButtonDown(0)) TryBeginDrag();
        if (isDragging && Input.GetMouseButton(0)) DoDrag();
        if (isDragging && Input.GetMouseButtonUp(0)) EndDrag();
    }

    void TryBeginDrag()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // 화면 클릭 → 물리 레이로 유닛 선택
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, pickRayDistance, pickableLayers,
                       QueryTriggerInteraction.Collide)) return;

        var entity = hit.collider.GetComponentInParent<Entity>();
        if (!entity) return;

        target = entity.transform;
        targetEntity = entity;
        targetAgent = target.GetComponent<NavMeshAgent>();
        targetRb = target.GetComponent<Rigidbody>();

        if (float.IsNaN(placementPlaneY)) placementPlaneY = target.position.y;

        // z-깊이 고정 후 1:1 스크린→월드 매핑
        dragDepth = cam.WorldToScreenPoint(target.position).z;
        var pick = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragDepth));
        offsetXZ = new Vector2(target.position.x - pick.x, target.position.z - pick.z);

        if (debugLog) Debug.Log($"[Drag] DOWN {entity.name} pick={pick} off=({offsetXZ.x:F2},{offsetXZ.y:F2})");

        // 드래그동안 에이전트/물리 잠금
        if (targetAgent) { agentWasEnabled = targetAgent.enabled; targetAgent.enabled = false; }
        if (targetRb) { rbWasKinematic = targetRb.isKinematic; targetRb.isKinematic = true; targetRb.angularVelocity = Vector3.zero; }

        lastValidPos = target.position;
        isDragging = true;
    }

    void DoDrag()
    {
        if (cam == null || target == null) return;

        // 카드패널 위에선 위치 갱신 멈춤(판매만 판정)
        if (PointerInsideSellZone()) return;

        var pick = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragDepth));
        Vector3 desired = new Vector3(pick.x + offsetXZ.x, target.position.y, pick.z + offsetXZ.y);

        Vector3 snapped = desired;
        bool ok;
        NavMeshHit navHit;

        // 같은 로직: 팀 스폰 NavMesh + 에이전트 타입으로 스냅
        int agentTypeId = -1;
        if (targetEntity && UnitManager.Instance)
            agentTypeId = UnitManager.Instance.GetAgentTypeId(targetEntity.UnitType);

        int areaMask = UnitManager.GetTeamAreaMask();
        if (agentTypeId >= 0)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = areaMask };
            ok = NavMesh.SamplePosition(desired, out navHit, snapPrimary, filter)
              || NavMesh.SamplePosition(desired, out navHit, snapFallback, filter);
        }
        else
        {
            ok = NavMesh.SamplePosition(desired, out navHit, snapPrimary, areaMask)
              || NavMesh.SamplePosition(desired, out navHit, snapFallback, areaMask);
        }

        if (ok)
        {
            snapped = navHit.position;
            lastValidPos = snapped;
            if (debugLog)
            {
                float d = Vector3.Distance(desired, snapped);
                Debug.Log($"[Drag] snap ok dist={d:F2} → {snapped}");
            }
        }
        else
        {
            if (debugLog) Debug.LogWarning("[Drag] snap fail");
            snapped = lastValidPos; // 실패 시 이전 정상 위치 유지
        }

        target.position = snapped;
    }

    void EndDrag()
    {
        if (!isDragging) return;

        // 에이전트/물리 원상복귀
        if (targetAgent && (agentWasEnabled || GameManager2.Instance.BattleStarted)) targetAgent.enabled = true;
        agentWasEnabled = false;
        if (targetRb) targetRb.isKinematic = rbWasKinematic;

        // 판매 판정
        if (PointerInsideSellZone()) TrySell(targetEntity);

        if (debugLog) Debug.Log($"[Drag] UP {target?.name}");

        // 상태 해제
        target = null; targetEntity = null; targetAgent = null; targetRb = null;
        isDragging = false;
    }

    void CancelDrag()
    {
        if (targetAgent && agentWasEnabled) targetAgent.enabled = true;
        if (targetRb) targetRb.isKinematic = rbWasKinematic;
        target = null; targetEntity = null; targetAgent = null; targetRb = null;
        isDragging = false;
    }

    // ───────────────────────── 판매 관련 ─────────────────────────
    bool PointerInsideSellZone()
    {
        CacheSellZone();
        return sellZone &&
               RectTransformUtility.RectangleContainsScreenPoint(sellZone, Input.mousePosition, uiCam);
    }
    void CacheSellZone()
    {
        if (sellZone) return;
        var go = GameObject.Find(sellPanelName);
        if (!go) return;
        sellZone = go.GetComponent<RectTransform>();
        sellCanvas = go.GetComponentInParent<Canvas>();
        uiCam = (sellCanvas && sellCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null
             : sellCanvas ? sellCanvas.worldCamera : null;
    }

    void TrySell(Entity entity)
    {
        if (!entity) return;
        var data = UnitManager.Instance?.GetEntityData(entity.UnitType);
        int price = data ? data.gold : 0;
        int cur = GoldManager.Instance?.GetCurrentGold() ?? 0;
        GoldManager.Instance?.SetGold(cur + price);

        GameManager2.Instance?.Unregister(entity);
        GameManager2.Instance?.RemoveInitMessageByUnitId(entity.UnitId);
        Destroy(entity.gameObject);

        Debug.Log($"🪙 판매 완료: {entity.UnitType} (+{price})");
    }
}