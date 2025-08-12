using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Camera mainCam;                  // 비워두면 Awake에서 자동 할당
    [SerializeField] bool useOrthographic = true;     // 직교 카메라일 때 권장 (오토배틀러 스타일)

    [Header("Pan / Move")]
    [SerializeField] float panSpeed = 30f;            // 키보드/엣지 스크롤 속도 (유닛/초)
    [SerializeField] float dragSpeed = 1.0f;          // 마우스 드래그 감도
    [SerializeField] KeyCode fastModifier = KeyCode.LeftShift;
    [SerializeField] float fastMultiplier = 2.0f;

    [Header("Mouse Drag")]
    [SerializeField] int dragMouseButton = 2;         // 0:좌, 1:우, 2:휠(권장)
    [SerializeField] bool invertDrag = true;

    [Header("Edge Scroll")]
    [SerializeField] bool edgeScroll = true;
    [SerializeField] int edgeThickness = 12;          // 화면 가장자리 픽셀
    [SerializeField] bool lockCursorWhileDrag = false;

    [Header("Zoom")]
    [SerializeField] float zoomSpeed = 8f;            // 마우스 휠 민감도
    [SerializeField] Vector2 orthoZoomRange = new Vector2(5f, 30f);   // orthographicSize 범위
    [SerializeField] Vector2 perspZoomDistance = new Vector2(10f, 80f); // 원근일 때 리그 기준 거리

    [Header("Rotation (optional)")]
    [SerializeField] bool allowRotate = false;
    [SerializeField] float rotateSpeed = 120f;
    [SerializeField] KeyCode rotateLeft = KeyCode.Q;
    [SerializeField] KeyCode rotateRight = KeyCode.E;

    [Header("Bounds")]
    [Tooltip("월드 좌표 기준 이동 가능 직사각형(중심/사이즈). 카메라가 이 박스 밖으로 나가지 않게 클램프.")]
    [SerializeField] Rect worldBounds = new Rect(-50, -50, 100, 100);

    [Header("Smoothing")]
    [SerializeField] float moveSmooth = 0.08f;        // 0=즉시, 0.1~0.2 부드러움
    [SerializeField] float zoomSmooth = 0.08f;

    // internal
    Vector3 targetPos;
    Vector3 moveVel;
    float targetOrtho;
    float targetRigDistance; // perspective용

    Transform rig;           // 이 스크립트가 달린 빈 오브젝트 (CameraRig)

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
            // 원근: 카메라를 rig의 자식으로 두고 Z축 전방으로 떨어진 거리 관리
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

        // 1) 키보드 (WASD / 화살표)
        float h = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
        float v = Input.GetAxisRaw("Vertical");   // W/S, ↑/↓
        move += new Vector3(h, 0f, v);

        // 2) 엣지 스크롤
        if (edgeScroll && Application.isFocused)
        {
            Vector3 mp = Input.mousePosition;
            if (mp.x <= edgeThickness) move += Vector3.left;
            else if (mp.x >= Screen.width - edgeThickness) move += Vector3.right;

            if (mp.y <= edgeThickness) move += Vector3.back;
            else if (mp.y >= Screen.height - edgeThickness) move += Vector3.forward;
        }

        // 3) 마우스 드래그(지면 기준 평면)
        if (Input.GetMouseButtonDown(dragMouseButton) && lockCursorWhileDrag) Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetMouseButtonUp(dragMouseButton) && lockCursorWhileDrag) Cursor.lockState = CursorLockMode.None;

        if (Input.GetMouseButton(dragMouseButton))
        {
            float dx = Input.GetAxisRaw("Mouse X");
            float dy = Input.GetAxisRaw("Mouse Y");
            Vector3 drag = new Vector3(dx, 0f, dy);
            if (invertDrag) drag *= -1f;

            // 카메라의 평면 기준으로 변환 (Y 회전만 고려)
            Vector3 right = Vector3.ProjectOnPlane(mainCam.transform.right, Vector3.up).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(mainCam.transform.forward, Vector3.up).normalized;
            Vector3 dragWorld = (right * drag.x + forward * drag.z) * dragSpeed;
            targetPos += dragWorld;
        }

        // 속도/가속
        float speed = panSpeed * (Input.GetKey(fastModifier) ? fastMultiplier : 1f);
        if (move.sqrMagnitude > 0.001f)
        {
            // 카메라의 Y 회전 기준으로 이동 방향 정규화
            Vector3 dir = CameraForwardOnPlane(move);
            targetPos += dir * speed * Time.unscaledDeltaTime;
        }

        // 경계 클램프
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
        // 위치 보간
        Vector3 newPos = Vector3.SmoothDamp(rig.position, targetPos, ref moveVel, moveSmooth);
        rig.position = newPos;

        // 줌 보간
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
        // 입력 벡터(좌/우/앞/뒤)를 카메라 Y회전 기준 평면 방향으로 변환
        Vector3 fwd = Vector3.ProjectOnPlane(mainCam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(mainCam.transform.right, Vector3.up).normalized;
        Vector3 dir = (right * input.x + fwd * input.z);
        return dir.normalized;
    }

    Vector3 ClampToBounds(Vector3 pos)
    {
        // 직교 카메라일 때는 화면 반폭/반높이를 고려해 더 타이트하게 클램프
        if (useOrthographic)
        {
            float halfH = mainCam.orthographicSize;
            float halfW = halfH * mainCam.aspect;

            float minX = worldBounds.xMin + halfW;
            float maxX = worldBounds.xMax - halfW;
            float minZ = worldBounds.yMin + halfH; // Rect.y를 Z로 사용
            float maxZ = worldBounds.yMax - halfH;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        }
        else
        {
            // 간단 버전: rig 위치만 직사각형 박스에 클램프 (프러스텀 미고려)
            pos.x = Mathf.Clamp(pos.x, worldBounds.xMin, worldBounds.xMax);
            pos.z = Mathf.Clamp(pos.z, worldBounds.yMin, worldBounds.yMax);
        }
        return pos;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Bounds 시각화 (XZ 평면)
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Vector3 center = new Vector3(worldBounds.center.x, transform.position.y, worldBounds.center.y);
        Vector3 size = new Vector3(worldBounds.size.x, 0.1f, worldBounds.size.y);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
