using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Camera mainCam;                  // ����θ� Awake���� �ڵ� �Ҵ�
    [SerializeField] bool useOrthographic = true;     // ���� ī�޶��� �� ���� (�����Ʋ�� ��Ÿ��)

    [Header("Pan / Move (Keyboard only)")]
    [SerializeField] float panSpeed = 30f;            // Ű���� �̵� �ӵ� (����/��)
    [SerializeField] KeyCode fastModifier = KeyCode.LeftShift;
    [SerializeField] float fastMultiplier = 2.0f;

    [Header("Zoom")]
    [SerializeField] float zoomSpeed = 8f;            // ���콺 �� �ΰ��� (�ܸ� ����)
    [SerializeField] Vector2 orthoZoomRange = new Vector2(5f, 30f);   // orthographicSize ����
    [SerializeField] Vector2 perspZoomDistance = new Vector2(10f, 80f); // ������ �� ���� ���� �Ÿ�

    [Header("Rotation (Q/E)")]
    [SerializeField] bool allowRotate = true;
    [SerializeField] float rotateSpeed = 120f;
    [SerializeField] KeyCode rotateLeft = KeyCode.Q;
    [SerializeField] KeyCode rotateRight = KeyCode.E;

    [Header("Zoom confinement")]
    [SerializeField] bool confineZoomToBounds = true; // �� �� �� �� ���� ����
    [SerializeField] float edgePadding = 0.5f;        // ����(���� ����)


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
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        move += new Vector3(h, 0f, v);

        float speed = panSpeed * (Input.GetKey(fastModifier) ? fastMultiplier : 1f);
        if (move.sqrMagnitude > 0.001f)
        {
            Vector3 dir = CameraForwardOnPlane(move);
            targetPos += dir * speed * Time.unscaledDeltaTime;
        }

        targetPos = ClampToBounds(targetPos);

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
            // (���� ī�޶�� Cinemachine Confiner ��� ����)
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
            pos.x = Mathf.Clamp(pos.x, worldBounds.xMin, worldBounds.xMax);
            pos.z = Mathf.Clamp(pos.z, worldBounds.yMin, worldBounds.yMax);
        }
        return pos;
    }
    float ComputeMaxOrthoAt(Vector3 pos)
    {
        // ī�޶� ��ȭ��: halfH = orthoSize, halfW = halfH * aspect
        // ȭ���� �ڽ� ������ ������ ��������:
        //   halfW <= min(pos.x - xMin, xMax - pos.x) - padding
        //   halfH <= min(pos.z - yMin, yMax - pos.z) - padding  (Rect.y�� Z�� ���)
        float xMin = worldBounds.xMin + edgePadding;
        float xMax = worldBounds.xMax - edgePadding;
        float zMin = worldBounds.yMin + edgePadding;
        float zMax = worldBounds.yMax - edgePadding;

        float availX = Mathf.Max(0f, Mathf.Min(pos.x - xMin, xMax - pos.x));
        float availZ = Mathf.Max(0f, Mathf.Min(pos.z - zMin, zMax - pos.z));

        // halfH�� availZ ���Ͽ��� �ϰ�, halfW(=halfH*aspect)�� availX ���Ͽ��� ��
        float byZ = availZ;
        float byX = availX / Mathf.Max(0.0001f, mainCam.aspect);

        // �� �� ���� ���� ������ �ִ� halfH(=orthographicSize)
        float maxOrtho = Mathf.Max(orthoZoomRange.x, Mathf.Min(byZ, byX));
        // ��ü ���ѵ� �Բ� ���
        return Mathf.Min(maxOrtho, orthoZoomRange.y);
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
