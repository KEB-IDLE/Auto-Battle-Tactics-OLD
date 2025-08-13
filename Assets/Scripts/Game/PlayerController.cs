using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Camera mainCam;                  // ����θ� Awake���� �ڵ� �Ҵ�
    [SerializeField] bool useOrthographic = true;     // ���� ī�޶��� �� ���� (�����Ʋ�� ��Ÿ��)

    [Header("Placement View")]
    [SerializeField] float placementPitch = 60f;      // 위에서 내려다보는 각도
    [SerializeField] float orthoCameraDistance = 40f; // 정사영일 때 카메라-리그 거리(충돌 회피용 기준)

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

    [Header("Collision (카메라 내부 비침 방지)")]
    [SerializeField] LayerMask collisionMask = ~0;    // Ground/Default/Environment 등
    [SerializeField] float collisionRadius = 0.6f; // 스피어캐스트 반경
    [SerializeField] float collisionBuffer = 0.25f;// 히트 지점에서 살짝 띄우기

    [Header("Smoothing")]
    [SerializeField] float moveSmooth = 0.08f;        // 0=���, 0.1~0.2 �ε巯��
    [SerializeField] float zoomSmooth = 0.08f;

    [Header("Start Pose (처음 위치/확대 고정)")]
    [SerializeField] bool useCustomStartPose = true;
    [SerializeField] Vector3 startRigPosition = new Vector3(0, 0, 0);
    [SerializeField] float startYaw = 0f;         // Yaw(좌우 회전)
    [SerializeField] float startOrthoSize = 18f;  // 정사영 시작 확대
    [SerializeField] float startDistance = 35f;  // 원근 시작 거리


    // internal

    Vector3 targetPos;
    Vector3 moveVel;
    float targetOrtho;
    float targetRigDistance; // perspective��
    float targetPitch;

    Transform rig;           // �� ��ũ��Ʈ�� �޸� �� ������Ʈ (CameraRig)

    void Awake()
    {
        rig = transform;
        if (!mainCam) mainCam = Camera.main;

        // ★ 리그의 피치/롤 제거 (수평 회전만 남김)
        Vector3 e = rig.localEulerAngles;
        rig.localRotation = Quaternion.Euler(0f, e.y, 0f);

        // 배치 뷰 기본 세팅
        targetPitch = placementPitch;
        mainCam.nearClipPlane = 0.2f;

        if (useOrthographic)
        {
            mainCam.orthographic = true;
            // ★ 시작 줌을 중간값으로 강제 (처음 과확대 방지)
            if (mainCam.orthographicSize <= 0.01f)
                mainCam.orthographicSize = Mathf.Lerp(orthoZoomRange.x, orthoZoomRange.y, 0.5f);
            targetOrtho = Mathf.Clamp(mainCam.orthographicSize, orthoZoomRange.x, orthoZoomRange.y);

            // 정사영이라도 충돌/간섭 계산용 거리 기준
            targetRigDistance = orthoCameraDistance;
        }
        else
        {
            mainCam.orthographic = false;
            float cur = -Vector3.Dot(mainCam.transform.localPosition, Vector3.forward);
            targetRigDistance = Mathf.Clamp(cur <= 0 ? 30f : cur, perspZoomDistance.x, perspZoomDistance.y);
        }
        if (useCustomStartPose)
        {
            rig.position = startRigPosition;
            rig.rotation = Quaternion.Euler(0f, startYaw, 0f);

            if (useOrthographic)
            {
                mainCam.orthographic = true;
                mainCam.orthographicSize = Mathf.Clamp(startOrthoSize, orthoZoomRange.x, orthoZoomRange.y);
                targetOrtho = mainCam.orthographicSize;
                targetRigDistance = orthoCameraDistance; // 정사영은 거리 고정(충돌계산용)
            }
            else
            {
                mainCam.orthographic = false;
                targetRigDistance = Mathf.Clamp(startDistance, perspZoomDistance.x, perspZoomDistance.y);
            }
        }

        // ★ 처음부터 피치 적용 + 그 피치에 맞는 카메라 위치로 "즉시" 배치
        mainCam.transform.localRotation = Quaternion.Euler(targetPitch, 0f, 0f);
        {
            float rad = targetPitch * Mathf.Deg2Rad;
            float dist = useOrthographic ? orthoCameraDistance : targetRigDistance;
            Vector3 local = new Vector3(0f, Mathf.Sin(rad) * dist, -Mathf.Cos(rad) * dist);
            mainCam.transform.localPosition = local;
        }

        targetPos = rig.position;
        ApplySmoothingAndCollision(Time.unscaledDeltaTime, true); // 첫 프레임 즉시 반영
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        HandleRotate(dt);
        HandleMove(dt);
        HandleZoom(dt);
        ApplySmoothingAndCollision(dt);
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

            // ī�޶��� ��� �������� ��ȯ (Y ȸ���� ����)
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
            targetPos += dir * speed * dt;
        }

        // ��� Ŭ����
        targetPos = ClampToBounds(targetPos);
    }

    void HandleZoom(float dt)
    {
        // 휠 입력 통합: mouseScrollDelta → (대체) Mouse ScrollWheel axis
        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) < 0.0001f)
            wheel = Input.GetAxis("Mouse ScrollWheel") * 120f; // 폴백

        if (Mathf.Abs(wheel) < 0.0001f) return;

        // 체감 빠르게: target 값을 직접 크게 움직임
        if (useOrthographic)
        {
            // 줌 스케일 보정 (줌이 커질수록 조금 더 움직이게)
            float scale = Mathf.Max(0.5f, mainCam.orthographicSize * 0.06f);
            targetOrtho = Mathf.Clamp(
                targetOrtho - wheel * zoomSpeed * scale,
                orthoZoomRange.x, orthoZoomRange.y
            );
        }
        else
        {
            float scale = Mathf.Max(0.5f, targetRigDistance * 0.04f);
            targetRigDistance = Mathf.Clamp(
                targetRigDistance - wheel * zoomSpeed * scale,
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

    void ApplySmoothingAndCollision(float dt, bool instant = false)
    {
        // 리그 위치 스무딩
        Vector3 newPos = Vector3.SmoothDamp(rig.position, targetPos, ref moveVel, moveSmooth);
        rig.position = newPos;

        // 카메라 목표 로컬 위치(피치/거리로 계산)
        float t = 1f - Mathf.Exp(-dt / Mathf.Max(zoomSmooth, 0.0001f));
        if (useOrthographic)
            mainCam.orthographicSize = Mathf.Lerp(mainCam.orthographicSize, targetOrtho, t);
        float rad = targetPitch * Mathf.Deg2Rad;
        float dist = useOrthographic ? orthoCameraDistance : targetRigDistance;

        Vector3 desiredLocal =
            new Vector3(0f, Mathf.Sin(rad) * dist, -Mathf.Cos(rad) * dist);

        // 스피어캐스트로 충돌 체크(리그 원점 -> 목표 카메라 위치)
        Vector3 origin = rig.position;
        Vector3 desiredWorld = rig.TransformPoint(desiredLocal);
        Vector3 dir = desiredWorld - origin;
        float maxDist = dir.magnitude;

        if (maxDist > 0.0001f) dir /= maxDist;

        Vector3 finalWorld = desiredWorld;
        if (Physics.SphereCast(origin, collisionRadius, dir, out RaycastHit hit, maxDist, collisionMask, QueryTriggerInteraction.Ignore))
        {
            finalWorld = hit.point - dir * collisionBuffer; // 살짝 앞으로 당겨서 박힘 방지
        }

        Vector3 finalLocal = rig.InverseTransformPoint(finalWorld);

        // 위치/회전 적용
        if (instant)
            mainCam.transform.localPosition = finalLocal;
        else
            mainCam.transform.localPosition = Vector3.Lerp(mainCam.transform.localPosition, finalLocal, t);

        Quaternion targetRot = Quaternion.Euler(targetPitch, 0f, 0f);
        mainCam.transform.localRotation = instant
            ? targetRot
            : Quaternion.Slerp(mainCam.transform.localRotation, targetRot, t);

        // 내부가 보이는 현상 줄이기
        mainCam.nearClipPlane = 0.2f;
    }


    Vector3 CameraForwardOnPlane(Vector3 input)
    {
        // �Է� ����(��/��/��/��)�� ī�޶� Yȸ�� ���� ��� �������� ��ȯ
        Vector3 fwd = Vector3.ProjectOnPlane(mainCam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(mainCam.transform.right, Vector3.up).normalized;
        Vector3 dir = (right * input.x + fwd * input.z);
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.zero;
    }

    Vector3 ClampToBounds(Vector3 pos)
    {
        // ���� ī�޶��� ���� ȭ�� ����/�ݳ��̸� ������ �� Ÿ��Ʈ�ϰ� Ŭ����
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
            // ���� ����: rig ��ġ�� ���簢�� �ڽ��� Ŭ���� (�������� �̰���)
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
