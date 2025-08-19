using UnityEngine;

public class MouseRayGizmo : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;   // ���� PlayerCamera �Ҵ�
    [SerializeField] private LayerMask raycastMask = ~0;
    [SerializeField] private float maxDistance = 500f;

    private Vector3 lastHitPoint;
    private bool hasHit;

    void Update()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        // �� �信�� ���̴� �Ķ� ��
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.cyan);

        if (Physics.Raycast(ray, out var hit, maxDistance, raycastMask))
        {
            lastHitPoint = hit.point;
            hasHit = true;
        }
        else hasHit = false;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!hasHit) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(lastHitPoint, 0.1f); // ���̰� ���� ������ �ʷ� ��
    }
}
