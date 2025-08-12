using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Camera mainCam;                  // ����θ� Awake���� �ڵ� �Ҵ�
    [SerializeField] bool useOrthographic = true;     // ���� ī�޶��� �� ���� (�����Ʋ�� ��Ÿ��)

    [Header("Pan / Move")]
    [SerializeField] float panSpeed = 30f;            // Ű����/���� ��ũ�� �ӵ� (����/��)
    [SerializeField] float dragSpeed = 1.0f;          // ���콺 �巡�� ����
    [SerializeField] KeyCode fastModifier = KeyCode.LeftShift;
    [SerializeField] float fastMultiplier = 2.0f;

    [Header("Mouse Drag")]
    [SerializeField] int dragMouseButton = 2;         // 0:��, 1:��, 2:��(����)
    [SerializeField] bool invertDrag = true;

    [Header("Edge Scroll")]
    [SerializeField] bool edgeScroll = true;
    [SerializeField] int edgeThickness = 12;          // ȭ�� �����ڸ� �ȼ�
    [SerializeField] bool lockCursorWhileDrag = false;

    [Header("Zoom")]
    [SerializeField] float zoomSpeed = 8f;            // ���콺 �� �ΰ���
    [SerializeField] Vector2 orthoZoomRange = new Vector2(5f, 30f);   // orthographicSize ����
    [SerializeField] Vector2 perspZoomDistance = new Vector2(10f, 80f); // ������ �� ���� ���� �Ÿ�

    [Header("Rotation (optional)")]
    [SerializeField] bool allowRotate = false;
    [SerializeField] float rotateSpeed = 120f;
    [SerializeField] KeyCode rotateLeft = KeyCode.Q;
    [SerializeField] KeyCode rotateRight = KeyCode.E;

    [Header("Bounds")]
    [Tooltip("���� ��ǥ ���� �̵� ���� ���簢��(�߽�/������). ī�޶� �� �ڽ� ������ ������ �ʰ� Ŭ����.")]
    [SerializeField] Rect worldBounds = new Rect(-50, -50, 100, 100);

    [Header("Smoothing")]
    [SerializeField] float moveSmooth = 0.08f;        // 0=���, 0.1~0.2 �ε巯��
    [SerializeField] float zoomSmooth = 0.08f;

    // internal
    Vector3 targetPos;
    Vector3 moveVel;
    float targetOrtho;
    float targetRigDistance; // perspective��

    Transform rig;           // �� ��ũ��Ʈ�� �޸� �� ������Ʈ (CameraRig)

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
            // ����: ī�޶� rig�� �ڽ����� �ΰ� Z�� �������� ������ �Ÿ� ����
            targetRigDistance = Vector3.Dot(mainCam.transform.localPosition, Vector3.forward) * -1f;
            if (targetRigDistance <= 0f) targetRigDistance = 30f;
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

        // 1) Ű���� (WASD / ȭ��ǥ)
        float h = Input.GetAxisRaw("Horizontal"); // A/D, ��/��
        float v = Input.GetAxisRaw("Vertical");   // W/S, ��/��
        move += new Vector3(h, 0f, v);

        // 2) ���� ��ũ��
        if (edgeScroll && Application.isFocused)
        {
            Vector3 mp = Input.mousePosition;
            if (mp.x <= edgeThickness) move += Vector3.left;
            else if (mp.x >= Screen.width - edgeThickness) move += Vector3.right;

            if (mp.y <= edgeThickness) move += Vector3.back;
            else if (mp.y >= Screen.height - edgeThickness) move += Vector3.forward;
        }

        // 3) ���콺 �巡��(���� ���� ���)
        if (Input.GetMouseButtonDown(dragMouseButton) && lockCursorWhileDrag) Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetMouseButtonUp(dragMouseButton) && lockCursorWhileDrag) Cursor.lockState = CursorLockMode.None;

        if (Input.GetMouseButton(dragMouseButton))
        {
            float dx = Input.GetAxisRaw("Mouse X");
            float dy = Input.GetAxisRaw("Mouse Y");
            Vector3 drag = new Vector3(dx, 0f, dy);
            if (invertDrag) drag *= -1f;

            // ī�޶��� ��� �������� ��ȯ (Y ȸ���� ���)
            Vector3 right = Vector3.ProjectOnPlane(mainCam.transform.right, Vector3.up).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(mainCam.transform.forward, Vector3.up).normalized;
            Vector3 dragWorld = (right * drag.x + forward * drag.z) * dragSpeed;
            targetPos += dragWorld;
        }

        // �ӵ�/����
        float speed = panSpeed * (Input.GetKey(fastModifier) ? fastMultiplier : 1f);
        if (move.sqrMagnitude > 0.001f)
        {
            // ī�޶��� Y ȸ�� �������� �̵� ���� ����ȭ
            Vector3 dir = CameraForwardOnPlane(move);
            targetPos += dir * speed * Time.unscaledDeltaTime;
        }

        // ��� Ŭ����
        targetPos = ClampToBounds(targetPos);
    }

    void HandleZoom(float dt)
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.0001f) return;

        if (useOrthographic)
        {
            targetOrtho = Mathf.Clamp(mainCam.orthographicSize - scroll * zoomSpeed, orthoZoomRange.x, orthoZoomRange.y);
        }
        else
        {
            targetRigDistance = Mathf.Clamp(targetRigDistance - scroll * zoomSpeed, perspZoomDistance.x, perspZoomDistance.y);
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
            mainCam.orthographicSize = Mathf.Lerp(mainCam.orthographicSize, targetOrtho, 1f - Mathf.Exp(-dt / Mathf.Max(zoomSmooth, 0.0001f)));
        }
        else
        {
            Vector3 local = mainCam.transform.localPosition;
            float curDist = -Vector3.Dot(local, Vector3.forward);
            float next = Mathf.Lerp(curDist, targetRigDistance, 1f - Mathf.Exp(-dt / Mathf.Max(zoomSmooth, 0.0001f)));
            mainCam.transform.localPosition = new Vector3(local.x, local.y, -next);
        }
    }

    Vector3 CameraForwardOnPlane(Vector3 input)
    {
        // �Է� ����(��/��/��/��)�� ī�޶� Yȸ�� ���� ��� �������� ��ȯ
        Vector3 fwd = Vector3.ProjectOnPlane(mainCam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(mainCam.transform.right, Vector3.up).normalized;
        Vector3 dir = (right * input.x + fwd * input.z);
        return dir.normalized;
    }

    Vector3 ClampToBounds(Vector3 pos)
    {
        // ���� ī�޶��� ���� ȭ�� ����/�ݳ��̸� ����� �� Ÿ��Ʈ�ϰ� Ŭ����
        if (useOrthographic)
        {
            float halfH = mainCam.orthographicSize;
            float halfW = halfH * mainCam.aspect;

            float minX = worldBounds.xMin + halfW;
            float maxX = worldBounds.xMax - halfW;
            float minZ = worldBounds.yMin + halfH; // Rect.y�� Z�� ���
            float maxZ = worldBounds.yMax - halfH;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        }
        else
        {
            // ���� ����: rig ��ġ�� ���簢�� �ڽ��� Ŭ���� (�������� �̰��)
            pos.x = Mathf.Clamp(pos.x, worldBounds.xMin, worldBounds.xMax);
            pos.z = Mathf.Clamp(pos.z, worldBounds.yMin, worldBounds.yMax);
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
