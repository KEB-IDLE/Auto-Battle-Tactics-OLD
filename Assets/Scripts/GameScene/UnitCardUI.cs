using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif


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

        // Ground 인덱스/마스크 확정
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0)
        {
            Debug.LogError("Ground 레이어가 Project Settings > Tags and Layers에 없음!");
        }
        else
        {
            placementMask = 1 << groundLayer;
            Debug.Log($"[Place] Ground layerIndex={groundLayer}, mask={placementMask.value}");
        }
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
#if UNITY_EDITOR
        {
            var ray = cam.ScreenPointToRay(e.position);
            if (Physics.Raycast(ray, out var anyHit, raycastMaxDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                Debug.LogWarning($"[DIAG] 아래 첫 히트: {anyHit.collider.name} " +
                                 $"layer={LayerMask.LayerToName(anyHit.collider.gameObject.layer)} " +
                                 $"trigger={anyHit.collider.isTrigger}");
            }
            else
            {
                Debug.LogWarning("[DIAG] 아래에 콜라이더가 아예 없음");
            }
        }
#endif
    }

    public void OnDrag(PointerEventData e)
    {
        if (dragIcon) dragIcon.transform.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        LogRayDiag(e.position);

        if (dragIcon) Destroy(dragIcon);
        if (!(GameManager2.Instance?.CanPlaceUnits ?? false)) return;
        if (!GameManager2.Instance.IsPlacementPhase) return;
        if (!cam) cam = Camera.main;
        if (!cam) return;

        // 1) 레이 쏘기 + 거리순 정렬
        var ray = cam.ScreenPointToRay(e.position);
        var hits = Physics.RaycastAll(ray, raycastMaxDistance, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // ───── 기존 로직: 첫 Ground + 옆면 방지 ─────
        RaycastHit? groundHit = null;
        foreach (var h in hits)
        {
            int l = h.collider.gameObject.layer;
            bool isGround = (placementMask.value & (1 << l)) != 0;
            if (!isGround) continue;

            if (h.normal.y < 0.5f) continue; // 옆면/급경사 제외
            groundHit = h;
            break; // 가장 가까운 Ground
        }

        // ───── 추가(Fallback) 로직: 가장 '위쪽'을 향한 Ground 선택 ─────
        if (!groundHit.HasValue)
        {
            RaycastHit? best = null;
            float bestNy = -1f;

            foreach (var h in hits)
            {
                int l = h.collider.gameObject.layer;
                bool isGround = ((1 << l) & placementMask.value) != 0;
                if (!isGround) continue;

                float ny = h.normal.y;     // -1..1
                if (ny < 0f) continue;     // 뒤집힌 면/밑면 제외

                if (ny > bestNy || (Mathf.Approximately(ny, bestNy) && best.HasValue && h.distance < best.Value.distance))
                {
                    best = h;
                    bestNy = ny;
                }
            }

            if (best.HasValue)
            {
                groundHit = best;
                if (debugLog) Debug.Log($"[Place] 1차 필터 실패 → 보정으로 {best.Value.collider.gameObject.name} 선택 (n.y={bestNy:F2}, dist={best.Value.distance:F2})");
            }
        }

        if (!groundHit.HasValue)
        {
            Debug.LogWarning("[Place] Raycast 실패 (Ground 후보 없음)");
            return;
        }

        var hit = groundHit.Value; // ← 아래 동일

        // 2) 프리팹의 agentTypeID 읽기
        int agentTypeId = GetAgentTypeIdFromPrefabs();

        // 3) NavMesh 스냅
        if (!TrySnapToNavMesh(hit.point, agentTypeId, out var spawnPos))
        {
            if (debugLog) Debug.LogWarning("[Place] 유효한 배치 지점 없음");
            return;
        }

        // 4) 골드 차감
        var data = UnitManager.Instance.GetEntityData(unitType);
        if (data == null) return;
        if (!GoldManager.Instance.TrySpendGold(data.gold))
        {
            if (debugLog) Debug.LogWarning($"❌ 골드 부족: 필요 {data.gold}, 보유 {GoldManager.Instance.GetCurrentGold()}");
            return;
        }

        // 5) 소환
        string owner = UserNetwork.Instance?.userId ?? "local";
        UnitManager.Instance.SpawnUnits(unitType, spawnPos, owner);

        if (debugLog) Debug.Log($"[Place] {unitType} 소환 → {spawnPos}");
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
            var filter = new NavMeshQueryFilter
            {
                agentTypeID = agentTypeId,
                areaMask = UnitManager.GetTeamAreaMask()   // 팀 스폰 구역만 허용
            };
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
    void LogRayDiag(Vector2 screenPos)
    {
        if (!cam) return;

        var ray = cam.ScreenPointToRay(screenPos);
        var hits = Physics.RaycastAll(ray, raycastMaxDistance, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        if (hits.Length == 0)
        {
            Debug.LogWarning("[DIAG] 아래에 콜라이더가 아예 없음");
            return;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            string path = GetPath(h.collider.transform);
            Debug.Log($"[ALL #{i}] {path}  dist={h.distance:F2}  layer={LayerMask.LayerToName(h.collider.gameObject.layer)}  trigger={h.collider.isTrigger}");
        }

        // 첫 번째 히트를 하이라키에서 하이라이트(에디터 전용)
#if UNITY_EDITOR
        Selection.activeGameObject = hits[0].collider.gameObject;
        EditorGUIUtility.PingObject(hits[0].collider.gameObject);
#endif
    }

    string GetPath(Transform t)
    {
        var names = new System.Collections.Generic.List<string>();
        while (t != null) { names.Add(t.name); t = t.parent; }
        names.Reverse();
        return string.Join("/", names);
    }


}
