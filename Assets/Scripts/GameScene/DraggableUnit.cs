using UnityEngine;

/// <summary>
/// 배치된 유닛을 마우스로 클릭/드래그해서 원하는 위치로 이동시키는 컴포넌트
/// </summary>
public class DraggableUnit : MonoBehaviour
{
    private Camera cam;
    private bool isDragging = false;
    private Vector3 offset;
    private Plane groundPlane;

    void Start()
    {
        cam = Camera.main;
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }

    void OnMouseDown()
    {
        if (!GameManager2.Instance.IsPlacementPhase) return;

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
    }
}
