
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.AI;

public class UnitCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string unitType;

    [Header("프리팹 설정")]
    public GameObject bluePrefab;
    public GameObject redPrefab;

    [Header("배치 필터")]
    [SerializeField] private LayerMask placementMask;     // ← Ground만 체크
    [SerializeField] private float raycastMaxDistance = 500f;
    [SerializeField] private float navSampleMaxDist = 1.0f;

    private GameObject dragIcon;
    private RectTransform canvasTransform;

    void Start()
    {
        if (placementMask.value == 0)
            placementMask = LayerMask.GetMask("Ground");
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"❌ [UnitCardUI] 상위에 Canvas가 없습니다! 오브젝트: {gameObject.name}");
            return;
        }

        canvasTransform = canvas.GetComponent<RectTransform>();
        if (canvasTransform == null)
        {
            Debug.LogError($"❌ [UnitCardUI] Canvas에 RectTransform이 없습니다!");
            return;
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시 임시 아이콘 생성 (선택 사항)
        dragIcon = new GameObject("DragIcon");
        dragIcon.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
        dragIcon.transform.SetParent(canvasTransform, false);
        dragIcon.transform.SetAsLastSibling();

        var image = dragIcon.AddComponent<UnityEngine.UI.Image>();
        image.sprite = GetComponent<UnityEngine.UI.Image>().sprite;
        image.raycastTarget = false;

        var rect = dragIcon.GetComponent<RectTransform>();
        rect.sizeDelta = GetComponent<RectTransform>().sizeDelta;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null) Destroy(dragIcon);

        // ❗ 전투 시작되었으면 무시
        if (!GameManager2.Instance.IsPlacementPhase)
        {
            Debug.LogWarning("❌ 배치 시간이 끝났습니다. 유닛을 소환할 수 없습니다.");
            return;
        }
        // entityData 가져오기
        EntityData entityData = UnitManager.Instance.GetEntityData(unitType);
        if (entityData == null)
        {
            Debug.LogError($"❌ EntityData 없음 또는 프리팹 누락. unitType: {unitType}");
            return;
        }

        // 💰 골드 체크
        if (!GoldManager.Instance.TrySpendGold((int)entityData.gold))
        {
            Debug.LogWarning($"❌ 골드 부족! 필요: {entityData.gold}");
            return;
        }

        if (TryGetWorldPosition(eventData, out Vector3 worldPos))
        {
            UnitManager.Instance.SpawnUnits(unitType, worldPos, UserNetwork.Instance.MyId);
        }
    }

    private bool TryGetWorldPosition(PointerEventData eventData, out Vector3 worldPos)
    {
        worldPos = default;

        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("❌ Camera.main 없음");
            return false;
        }

        var ray = cam.ScreenPointToRay(eventData.position);

        // 1) Ground 레이어에만 맞추기
        if (Physics.Raycast(ray, out var hit, raycastMaxDistance, placementMask, QueryTriggerInteraction.Ignore))
        {
            // 2) 맞은 점을 NavMesh 내부 좌표로 스냅
            if (NavMesh.SamplePosition(hit.point, out var navHit, navSampleMaxDist, NavMesh.AllAreas))
            {
                worldPos = navHit.position;
                Debug.Log($"✅ Raycast 성공(Ground+NavMesh): {hit.collider.name}, 위치: {worldPos}");
                return true;
            }
            else
            {
                Debug.LogWarning($"❌ NavMesh 없음: {hit.collider.name} @ {hit.point}");
                return false;
            }
        }

        Debug.LogWarning($"❌ Raycast 실패(placementMask={placementMask.value}). 화면좌표: {eventData.position}");
        return false;
    }

}
