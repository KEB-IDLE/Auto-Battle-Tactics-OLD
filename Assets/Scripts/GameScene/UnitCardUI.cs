using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using System.Linq;

public class UnitCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string unitType;

    [Header("프리팹 설정")]
    public GameObject bluePrefab;
    public GameObject redPrefab;

    [Header("배치 필터")]
    [SerializeField] private LayerMask placementMask;           // Ground/Default/Terrain/Floor/Map만 허용
    [SerializeField] private float raycastMaxDistance = 1500f;
    [SerializeField] private float navSampleMaxDist = 2.5f;

    [Tooltip("배치 허용 레이어 이름(화이트리스트)")]
    [SerializeField]
    private string[] allowedLayerNames =
        { "Ground", "Default", "Terrain", "Floor", "Map" };

    [Tooltip("배치 금지/무시 레이어 이름(블랙리스트)")]
    [SerializeField]
    private string[] blockedLayerNames =
        { "Structure", "Wall", "Obstacle", "Cliff" };

    private GameObject dragIcon;
    private RectTransform canvasTransform;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("❌ Camera.main 없음 (메인 카메라에 MainCamera 태그 확인)");
            return;
        }

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"❌ [UnitCardUI] 상위에 Canvas가 없습니다! 오브젝트: {gameObject.name}");
            return;
        }
        canvasTransform = canvas.GetComponent<RectTransform>();

        // 인스펙터 미설정이면 화이트리스트로 마스크 구성
        if (placementMask.value == 0)
        {
            // placementMask를 모든 레이어 허용으로 설정
            placementMask = ~0; // 모든 레이어 허용
        }

        // 혹시라도 섞여 들어간 블랙리스트 레이어는 강제 제거
        foreach (var bn in blockedLayerNames)
        {
            int li = LayerMask.NameToLayer(bn);
            if (li >= 0) placementMask.value &= ~(1 << li);
        }

        // 자동 감지(중앙 레이) 시 화이트리스트에 해당할 때만 추가
        AutoDetectPlacementLayer();
        Debug.Log($"🧰 placementMask 준비 완료 (value={placementMask.value})");
    }

    void AutoDetectPlacementLayer()
    {
        var centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(centerRay, out var hitAny, raycastMaxDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            string ln = LayerMask.LayerToName(hitAny.collider.gameObject.layer);
            // 화이트리스트만 추가, 블랙리스트는 무시
            if (allowedLayerNames.Contains(ln))
            {
                placementMask.value |= (1 << hitAny.collider.gameObject.layer);
                Debug.Log($"🔎 자동 감지: '{ln}' 레이어를 placementMask에 추가 (value={placementMask.value})");
            }
            else
            {
                Debug.Log($"🔎 자동 감지 스킵: '{ln}'는 배치 허용 레이어가 아님");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ 자동 감지 실패: 중앙 레이로 아무것도 맞지 않음");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!(GameManager2.Instance?.CanPlaceUnits ?? false))
        {
            Debug.LogWarning("❌ 아직 배치 금지 상태(상대 미접속 혹은 카운트다운 전)");
            return; // 아이콘도 만들지 않음
        }
        dragIcon = new GameObject("DragIcon");
        dragIcon.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
        dragIcon.transform.SetParent(canvasTransform, false);
        dragIcon.transform.SetAsLastSibling();

        var image = dragIcon.AddComponent<UnityEngine.UI.Image>();
        var srcImg = GetComponent<UnityEngine.UI.Image>();
        if (srcImg != null)
        {
            image.sprite = srcImg.sprite;
            image.raycastTarget = false;
            dragIcon.GetComponent<RectTransform>().sizeDelta = srcImg.rectTransform.sizeDelta;
        }

        // 드래그 시작 시 UI 크기 줄이기
        var dragIconRect = dragIcon.GetComponent<RectTransform>();
        if (dragIconRect != null)
        {
            dragIconRect.localScale = new Vector3(0.5f, 0.5f, 1); // 크기 반으로 줄이기
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null) Destroy(dragIcon);

        if (!(GameManager2.Instance?.CanPlaceUnits ?? false))
        {
            Debug.LogWarning("❌ 아직 배치 금지 상태(상대 미접속 혹은 카운트다운 전)");
            return;
        }
        if (!GameManager2.Instance.IsPlacementPhase)
        {
            Debug.LogWarning("❌ 배치 시간이 끝났습니다. 유닛을 소환할 수 없습니다.");
            return;
        }

        var entityData = UnitManager.Instance.GetEntityData(unitType);
        if (entityData == null)
        {
            Debug.LogError($"❌ EntityData 없음 또는 프리팹 누락. unitType: {unitType}");
            return;
        }

        // 1) 배치 좌표 계산
        if (!TryGetWorldPosition(eventData, out Vector3 worldPos))
            return;

        // 2) 골드 차감
        if (!GoldManager.Instance.TrySpendGold((int)entityData.gold))
        {
            Debug.LogWarning($"❌ 골드 부족! 필요: {entityData.gold}");
            return;
        }

        // 3) 스폰
        UnitManager.Instance.SpawnUnits(unitType, worldPos, UserNetwork.Instance.MyId);
    }

    private bool IsBlocked(int layer)
    {
        string ln = LayerMask.LayerToName(layer);
        return blockedLayerNames.Contains(ln);
    }

    private bool IsAllowed(int layer)
    {
        string ln = LayerMask.LayerToName(layer);
        return allowedLayerNames.Contains(ln);
    }

    private bool TryGetWorldPosition(PointerEventData eventData, out Vector3 worldPos)
    {
        worldPos = default;

        if (cam == null)
        {
            Debug.LogError("❌ Camera 참조 사라짐");
            return false;
        }

        var ray = cam.ScreenPointToRay(eventData.position);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.yellow, 2f);

        // A) 허용 레이어 마스크로 1차 시도 (Structure 등은 아예 무시되므로 통과)
        if (Physics.Raycast(ray, out var hit, raycastMaxDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (NavMesh.SamplePosition(hit.point, out var navHit, navSampleMaxDist, NavMesh.AllAreas))
            {
                worldPos = navHit.position;
                Debug.Log($"✅ Raycast 성공 → {hit.collider.name} @ {worldPos}");
                return true;
            }
            else
            {
               Debug.LogWarning($"❌ NavMesh 없음: {hit.collider.name} @ {hit.point}");
                // 계속 폴백
            }
        }
        else
        {
            Debug.LogWarning($"❌ Raycast 실패(mask:{placementMask.value}) @ {eventData.position}");
        }

        // B) RaycastAll로 ‘먼저 Structure를 맞았더라도’ 뒤쪽 Ground를 찾기
        var hits = Physics.RaycastAll(ray, raycastMaxDistance, ~0, QueryTriggerInteraction.Ignore);
        if (hits != null && hits.Length > 0)
        {
            // 가까운 것부터 순회
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var h in hits)
            {
                int layer = h.collider.gameObject.layer;
                string ln = LayerMask.LayerToName(layer);

                if (!IsAllowed(layer)) continue;   // 화이트리스트 외 레이어는 패스
                if (IsBlocked(layer)) continue;    // 블랙리스트면 패스

                if (NavMesh.SamplePosition(h.point, out var navHit2, navSampleMaxDist, NavMesh.AllAreas))
                {
                    worldPos = navHit2.position;
                    Debug.Log($"✅ 폴백 성공(Allowed:{ln}) @ {worldPos}");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"❌ 폴백 NavMesh 미히트(Allowed:{ln}) @ {h.point}");
                }
            }
        }
        else
        {
            Debug.LogWarning("❌ RaycastAll에서도 아무것도 안 맞음");
        }

        return false;
    }
}
