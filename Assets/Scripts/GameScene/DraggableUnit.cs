using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 배치된 유닛을 마우스로 클릭/드래그해서 원하는 위치로 이동시키는 컴포넌트
/// </summary>
public class DraggableUnit : MonoBehaviour
{
    private Camera cam;
    private bool isDragging = false;
    private Vector3 offset;
    private Plane groundPlane;

    // ── 판매 존 캐시 ──────────────────────────────
    private static RectTransform sellZone;      // CardPanel
    private static Canvas sellCanvas;
    private static Camera uiCam;

    void Start()
    {
        cam = Camera.main;
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }
    void OnEnable()  // 씬 돌아올 때 카메라 다시 잡기
    {
        cam = Camera.main;
    }


    void OnMouseDown()
    {
        if (!GameManager2.Instance.IsPlacementPhase) return;

        if (cam == null) cam = Camera.main;      
        if (cam == null) return;                

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            offset = transform.position - hitPoint;
            isDragging = true;

            // 선택 시 NavMeshAgent 끄기 (움직임 방지)
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging || !GameManager2.Instance.IsPlacementPhase) return;

        if (cam == null) cam = Camera.main;      // ★ 추가
        if (cam == null) return;                 // ★ 추가

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            transform.position = hitPoint + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // 드래그 끝나면 NavMeshAgent 다시 켜기 (선택)
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && GameManager2.Instance.BattleStarted)
        {
            agent.enabled = true;
        }
        CacheSellZone();
        if (sellZone != null &&
            RectTransformUtility.RectangleContainsScreenPoint(sellZone, Input.mousePosition, uiCam))
        {
            TrySell();
            return;
        }
    }
    private void CacheSellZone()
    {
        if (sellZone != null) return;

        var go = GameObject.Find("CardPanel");      
        if (go == null) return;

        sellZone = go.GetComponent<RectTransform>();
        sellCanvas = go.GetComponentInParent<Canvas>();
        uiCam = (sellCanvas != null && sellCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                ? null : sellCanvas?.worldCamera;
    }
    private void TrySell()
    {
        var entity = GetComponent<Entity>();
        if (entity == null) return;

        // 유닛 가격 얻기
        var data = UnitManager.Instance?.GetEntityData(entity.UnitType);
        int price = (data != null) ? data.gold : 0;

        // 골드 환급
        int cur = GoldManager.Instance?.GetCurrentGold() ?? 0;
        GoldManager.Instance?.SetGold(cur + price);  

        // 등록 해제 & 오브젝트 제거
        GameManager2.Instance?.Unregister(entity);
        GameManager2.Instance?.RemoveInitMessageByUnitId(entity.UnitId);
        Destroy(gameObject);

        Debug.Log($"🪙 판매 완료: {entity.UnitType} (+{price})");
    }
}
