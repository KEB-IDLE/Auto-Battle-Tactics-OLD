
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;


public class UnitCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string unitType;

    [Header("프리팹 설정")]
    public GameObject bluePrefab;
    public GameObject redPrefab;
    private GameObject dragIcon;
    private RectTransform canvasTransform;

    void Start()
    {
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
        if (dragIcon != null)
            Destroy(dragIcon);

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
        worldPos = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            worldPos = hit.point;
            Debug.Log($"✅ Raycast 성공! 맞은 대상: {hit.collider.name}, 위치: {worldPos}");
            return true;
        }

        Debug.LogWarning($"❌ Raycast 실패. 마우스 화면 좌표: {eventData.position}");
        return false;
    }

}
