using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Camera mainCam;                  // ���� Awake���� Camera.main �ڵ� �Ҵ�
    [SerializeField] bool useOrthographic = true;     // �����Ʋ�� ��Ÿ���̸� ���� ����

    [Header("Pan / Move (Keyboard only)")]
    [SerializeField] float panSpeed = 30f;            // �̵� �ӵ�(����/��)
    [SerializeField] KeyCode fastModifier = KeyCode.LeftShift;
    [SerializeField] float fastMultiplier = 2.0f;

    [Header("Zoom")]
    [SerializeField] float zoomSpeed = 8f;            // ���콺 �� �ΰ���
    [SerializeField] Vector2 orthoZoomRange = new Vector2(5f, 30f);     // orthographicSize ����
    [SerializeField] Vector2 perspZoomDistance = new Vector2(10f, 80f); // ������ �� rig-ī�޶� �Ÿ�

    [Header("Rotation (Q/E)")]
    [SerializeField] bool allowRotate = true;
    [SerializeField] float rotateSpeed = 120f;
    [SerializeField] KeyCode rotateLeft = KeyCode.Q;
    [SerializeField] KeyCode rotateRight = KeyCode.E;

    [Header("Bounds")]
    [Tooltip("���� ��ǥ ���� �̵� ���� ���簢��(�߽�/������). Rect.y�� Z�� ���.")]
    [SerializeField] Rect worldBounds = new Rect(-50, -50, 100, 100);

    [Header("Zoom confinement")]
    [SerializeField] bool confineZoomToBounds = true; // ������ �� ���� ������ �ʵ��� ����(����)
    [SerializeField] float edgePadding = 0.5f;        // ȭ�� �����ڸ� ����(���� ����)

    [Header("Smoothing")]
    [SerializeField] float moveSmooth = 0.08f;        // 0=���, 0.1~0.2 �ε巯��
    [SerializeField] float zoomSmooth = 0.08f;

    // internal state
    Transform rig;           // �� ��ũ��Ʈ�� �޸� �� ������Ʈ(CameraRig)
    Vector3 targetPos;
    Vector3 moveVel;
    float targetOrtho;
    float targetRigDistance; // perspective��

    void Awake()
    {
        rig = transform;
        if (!mainCam) mainCam = Camera.main;

        if (useOrthographic)
        {
            if (!mainCam.orthographic) mainCam.orthographic = true;
            targetOrtho = mainCam.orthographicSize;
        }
        else
        {
            if (mainCam.orthographic) mainCam.orthographic = false;
            // ī�޶� rig�� �ڽ����� �ΰ� Z-����(-localZ) �Ÿ��� ����
            float cur = -Vector3.Dot(mainCam.transform.localPosition, Vector3.forward);
            targetRigDistance = (cur > 0f) ? cur : 30f;
        }

        targetPos = rig.position;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        HandleRotate(dt);
        HandleMove(dt);
        HandleZoom(dt);
        ApplySmoothing(dt);
    }

    void HandleMove(float dt)
    {
        Vector3 move = Vector3.zero;

        // Ű���� (WASD / ����Ű)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        move += new Vector3(h, 0f, v);

        // �ӵ�/����
        float speed = panSpeed * (Input.GetKey(fastModifier) ? fastMultiplier : 1f);
        if (move.sqrMagnitude > 0.001f)
        {
            Vector3 dir = CameraForwardOnPlane(move);
            targetPos += dir * speed * Time.unscaledDeltaTime;
        }

        // ��ġ ��� Ŭ����
        targetPos = ClampToBounds(targetPos);

        // ���� ī�޶�: ���� ��ġ���� ������ �ִ� �� ���� ũ�� �ʰ� ����
        if (useOrthographic && confineZoomToBounds)
            targetOrtho = Mathf.Min(targetOrtho, ComputeMaxOrthoAt(targetPos));
    }

    void HandleZoom(float dt)
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.0001f) return;

        if (useOrthographic)
        {
            targetOrtho = Mathf.Clamp(
                mainCam.orthographicSize - scroll * zoomSpeed,
                orthoZoomRange.x, orthoZoomRange.y
            );

            if (confineZoomToBounds)
                targetOrtho = Mathf.Min(targetOrtho, ComputeMaxOrthoAt(targetPos));
        }
        else
        {
            // ����: ������ ȭ�� ��� ����� ����?Cinemachine Confiner ����
            targetRigDistance = Mathf.Clamp(
                targetRigDistance - scroll * zoomSpeed,
                perspZoomDistance.x, perspZoomDistance.y
            );
        }
    }

    void HandleRotate(float dt)
    {
        if (!allowRotate) return;

        float rot = 0f;
        if (Input.GetKey(rotateLeft)) rot -= 1f;
        if (Input.GetKey(rotateRight)) rot += 1f;

        if (Mathf.Abs(rot) > 0.01f)
            rig.Rotate(Vector3.up, rot * rotateSpeed * dt, Space.World);
    }

    void ApplySmoothing(float dt)
    {
        // ��ġ ����
        Vector3 newPos = Vector3.SmoothDamp(rig.position, targetPos, ref moveVel, moveSmooth);
        rig.position = newPos;

        // �� ����
        if (useOrthographic)
        {
            mainCam.orthographicSize = Mathf.Lerp(
                mainCam.orthographicSize,
                targetOrtho,
                1f - Mathf.Exp(-dt / Mathf.Max(zoomSmooth, 0.0001f))
            );
        }
        else
        {
            Vector3 local = mainCam.transform.localPosition;
            float curDist = -Vector3.Dot(local, Vector3.forward);
            float next = Mathf.Lerp(
                curDist,
                targetRigDistance,
                1f - Mathf.Exp(-dt / Mathf.Max(zoomSmooth, 0.0001f))
            );
            mainCam.transform.localPosition = new Vector3(local.x, local.y, -next);
        }
    }

    // ===== Helpers =====

    // �Է� ����(��/��/��/��)�� ī�޶� Yȸ�� ���� ��� �������� ��ȯ
    Vector3 CameraForwardOnPlane(Vector3 input)
    {
        Vector3 fwd = Vector3.ProjectOnPlane(mainCam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(mainCam.transform.right, Vector3.up).normalized;
        Vector3 dir = (right * input.x + fwd * input.z);
        return dir.sqrMagnitude > 0f ? dir.normalized : Vector3.zero;
    }

    // ���� Ÿ�� ��ġ���� ȭ���� �� ������ ������ �ʰ� ��� ������ �ִ� orthographicSize
    float ComputeMaxOrthoAt(Vector3 pos)
    {
        float xMin = worldBounds.xMin + edgePadding;
        float xMax = worldBounds.xMax - edgePadding;
        float zMin = worldBounds.yMin + edgePadding; // Rect.y�� Z�� ���
        float zMax = worldBounds.yMax - edgePadding;

        float availX = Mathf.Max(0f, Mathf.Min(pos.x - xMin, xMax - pos.x));
        float availZ = Mathf.Max(0f, Mathf.Min(pos.z - zMin, zMax - pos.z));

        float byZ = availZ; // halfH
        float byX = availX / Mathf.Max(0.0001f, mainCam.aspect); // halfW = halfH * aspect <= availX

        float maxOrthoByBounds = Mathf.Min(byZ, byX);
        float clampedToRange = Mathf.Clamp(maxOrthoByBounds, orthoZoomRange.x, orthoZoomRange.y);
        return clampedToRange;
    }

    // ī�޶� �߽�(rig.position)�� ��踦 ���� �ʵ��� Ŭ����
    Vector3 ClampToBounds(Vector3 pos)
    {
        if (useOrthographic)
        {
            float effectiveOrtho = confineZoomToBounds ? targetOrtho : mainCam.orthographicSize;
            float halfH = effectiveOrtho;
            float halfW = halfH * mainCam.aspect;

            float minX = worldBounds.xMin + halfW + edgePadding;
            float maxX = worldBounds.xMax - halfW - edgePadding;
            float minZ = worldBounds.yMin + halfH + edgePadding; // Rect.y�� Z�� ���
            float maxZ = worldBounds.yMax - halfH - edgePadding;

            // ���� ȭ�麸�� ���� �ش� ��Ȳ ����
            if (minX > maxX) { float midX = (worldBounds.xMin + worldBounds.xMax) * 0.5f; minX = maxX = midX; }
            if (minZ > maxZ) { float midZ = (worldBounds.yMin + worldBounds.yMax) * 0.5f; minZ = maxZ = midZ; }

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        }
        else
        {
            pos.x = Mathf.Clamp(pos.x, worldBounds.xMin + edgePadding, worldBounds.xMax - edgePadding);
            pos.z = Mathf.Clamp(pos.z, worldBounds.yMin + edgePadding, worldBounds.yMax - edgePadding);
        }
        return pos;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Bounds �ð�ȭ (XZ ���)
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Vector3 center = new Vector3(worldBounds.center.x, transform.position.y, worldBounds.center.y);
        Vector3 size = new Vector3(worldBounds.size.x, 0.1f, worldBounds.size.y);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
