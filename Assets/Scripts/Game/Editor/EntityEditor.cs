//#if UNITY_EDITOR
//using System.Linq;
//using System.Net.Mail;
//using Unity.VisualScripting;
//using UnityEditor;
//#endif
//using UnityEngine;
//using UnityEngine.UI;

//#if UNITY_EDITOR
//[CustomEditor(typeof(EntityData))]
//public class EntityEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        EntityData data = (EntityData)target;

//        if (GUILayout.Button("이 데이터로 Entity 생성"))
//            CreateEntityInScene(data);
//    }

//    private void CreateEntityInScene(EntityData data)
//    {
//        GameObject go;
//        // 에디터 환경/런타임 분기
//        Team myTeam;
//#if UNITY_EDITOR
//        if (!Application.isPlaying)
//        {
//            // 에디터 환경에서는 강제 RED로 지정 (원하면 Blue 등으로 변경 가능)
//            myTeam = Team.Red;
//        }
//        else
//#endif
//        {
//            // 런타임에서는 실제 네트워크 값 사용
//            myTeam = UserNetwork.Instance != null ? UserNetwork.Instance.MyTeam : Team.Red;
//        }

//        // ✅ 내 팀 가져오기
//        //Team myTeam = UserNetwork.Instance.MyTeam;
//        // ✅ 내 팀 프리팹 가져오기
//        GameObject prefab = myTeam == Team.Red ? data.redPrefab : data.bluePrefab;

//        if (prefab != null)
//        {
//            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
//            go.name = data.entityName + "_Entity";
//        }
//        else
//            go = new GameObject(data.entityName + "_Entity");

//        var entity = go.GetComponent<Entity>() ?? go.AddComponent<Entity>();
//        entity.SetData(data);

//        // EntityEditor.cs 내 CreateEntityInScene(data) 메서드 마지막에 추가
//        var collider = go.GetComponent<CapsuleCollider>();
//        if (collider == null)
//            collider = go.AddComponent<CapsuleCollider>();
//        collider.radius = 0.5f;
//        collider.height = 1.8f;
//        collider.center = new Vector3(0, 0.85f, 0); // 예시값

//        if (go.transform.Find("HitPoint") == null)
//        {
//            GameObject hitPoint = new GameObject("HitPoint");
//            hitPoint.transform.SetParent(go.transform);
//            // 기본 위치 (예: 몸통 중앙에 오도록) → 필요에 따라 조정
//            hitPoint.transform.localPosition = collider.center; // 또는 원하는 값
//        }

//        if (data.projectilePrefab != null && go.transform.Find("FirePoint") == null)
//        {
//            GameObject firePoint = new GameObject("FirePoint");
//            firePoint.transform.SetParent(go.transform);
//            firePoint.transform.localPosition = new Vector3(0, 1, 0);
//        }

//        // === HealthBar Canvas 생성 ===
//        if (go.transform.Find("HealthBarCanvas") == null)
//        {
//            var healthBarCanvas = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasRenderer));
//            healthBarCanvas.transform.SetParent(go.transform);

//            // ▼ 여기 분기 삽입
//            float y = data.isMounted ? 3.2f : 2.0f;   // EntityData에 isMounted 플래그가 있다면
//            healthBarCanvas.transform.localPosition = new Vector3(0, y, 0);

//            var canvas = healthBarCanvas.GetComponent<Canvas>();
//            canvas.renderMode = RenderMode.WorldSpace;
//            canvas.worldCamera = Camera.main;

//            // Canvas 크기, 스케일 조정
//            healthBarCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.3f);
//            healthBarCanvas.transform.localScale = Vector3.one * 0.01f; // 크기 축소

//            // === HealthBar(Background) ===
//            var bgGO = new GameObject("HealthBarBG", typeof(Image));
//            bgGO.transform.SetParent(healthBarCanvas.transform, false);
//            var bgImg = bgGO.GetComponent<Image>();
//            bgImg.color = Color.grey; // 배경색
//            var bgRect = bgGO.GetComponent<RectTransform>();
//            bgRect.sizeDelta = new Vector2(150, 20);

//            // === HealthBar(Fill) ===
//            var fillGO = new GameObject("HealthBarFill", typeof(Image));
//            fillGO.transform.SetParent(bgGO.transform, false);
//            var fillImg = fillGO.GetComponent<Image>();

//            // 1. Sprite 에셋 찾기 (프로젝트 내 "UI_Fill_Green" 이름의 Sprite 사용)
//            var fillSprite = AssetDatabase.FindAssets("t:Sprite UI_Fill_Green")
//                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
//                .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
//                .FirstOrDefault();
//            fillImg.sprite = fillSprite;

//            // 2. 색상 및 타입 세팅
//            fillImg.color = Color.green; // 혹시 필요하면 원하는 색상으로
//            fillImg.type = Image.Type.Filled;
//            fillImg.fillMethod = Image.FillMethod.Horizontal;
//            fillImg.fillAmount = 1.0f; // 처음엔 가득

//            // 3. RectTransform 맞춤
//            var fillRect = fillGO.GetComponent<RectTransform>();
//            fillRect.anchorMin = new Vector2(0, 0);
//            fillRect.anchorMax = new Vector2(1, 1);
//            fillRect.offsetMin = Vector2.zero;
//            fillRect.offsetMax = Vector2.zero;

//            // 4. 스크립트 붙이기
//            var healthBar = fillGO.AddComponent<HealthBar>();
//            healthBar.fillImage = fillImg;
//        }
//        AttachBillboardAndAssignCamera(go.transform);

//        var rb = go.GetComponent<Rigidbody>();
//        if (rb == null)
//            rb = go.AddComponent<Rigidbody>();

//        rb.freezeRotation = false;
//        rb.constraints = RigidbodyConstraints.FreezeRotationX
//                       | RigidbodyConstraints.FreezeRotationY
//                       | RigidbodyConstraints.FreezeRotationZ;

//        Selection.activeGameObject = go;
//    }

//    private void AttachBillboardAndAssignCamera(Transform root)
//    {
//        var hbc = root.Find("HealthBarCanvas");
//        if (hbc == null) return;

//        // 1) Billboard 없으면 부착
//        var bb = hbc.GetComponent<Billboard>();
//        if (bb == null) bb = hbc.gameObject.AddComponent<Billboard>();

//        // 2) CameraRig/PlayerCamera 탐색
//        var cam = FindPlayerCameraInRig();
//        if (cam == null) return;

//        // 3) World Space Canvas에도 동일 카메라 설정(기존 Camera.main 대체)
//        var canvas = hbc.GetComponent<Canvas>();
//        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
//            canvas.worldCamera = cam;

//#if UNITY_EDITOR
//        var so = new UnityEditor.SerializedObject(bb);
//        var prop = so.FindProperty("targetCamera");
//        if (prop != null)
//        {
//            prop.objectReferenceValue = cam;
//            so.ApplyModifiedProperties();
//            UnityEditor.EditorUtility.SetDirty(bb);
//        }
//#endif
//    }

//    private Camera FindPlayerCameraInRig()
//    {
//        // 우선 정확 경로 시도
//        var rig = GameObject.Find("CameraRig");
//        if (rig != null)
//        {
//            var pc = rig.transform.Find("PlayerCamera");
//            if (pc != null)
//            {
//                var cam = pc.GetComponentInChildren<Camera>(true);
//                if (cam != null) return cam;
//            }
//            // Rig 아래 첫 Camera
//            var any = rig.GetComponentsInChildren<Camera>(true).FirstOrDefault();
//            if (any != null) return any;
//        }

//        var all = Resources.FindObjectsOfTypeAll<Camera>()
//            .Where(c => c.gameObject.scene.IsValid()) 
//            .ToArray();
//        var byName = all.FirstOrDefault(c => c.name == "PlayerCamera");
//        if (byName != null) return byName;

//        return Camera.main;
//    }

//}
//#endif

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(EntityData))]
public class EntityEditor : Editor
{
    // 인스펙터에서 선택할 팀(싱글용)
    private static Team _spawnTeam = Team.Red;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // 싱글 게임: 팀 선택 UI
        _spawnTeam = (Team)EditorGUILayout.EnumPopup("Spawn as team (Single)", _spawnTeam);

        var data = (EntityData)target;
        if (GUILayout.Button("이 데이터로 Entity 생성"))
            CreateEntityInScene(data, _spawnTeam);
    }

    private void CreateEntityInScene(EntityData data, Team team)
    {
        // ✅ 싱글 전용: 네트워크 의존성 제거, 선택한 팀으로 프리팹 선택
        GameObject prefab = team == Team.Red ? data.redPrefab : data.bluePrefab;

        GameObject go;
        if (prefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = data.entityName + "_Entity";
        }
        else
        {
            go = new GameObject(data.entityName + "_Entity");
        }

        var entity = go.GetComponent<Entity>() ?? go.AddComponent<Entity>();
        entity.SetData(data);

        // Collider 기본값
        var collider = go.GetComponent<CapsuleCollider>() ?? go.AddComponent<CapsuleCollider>();
        collider.radius = 0.5f;
        collider.height = 1.8f;
        collider.center = new Vector3(0, 0.85f, 0);

        // HitPoint
        if (go.transform.Find("HitPoint") == null)
        {
            var hitPoint = new GameObject("HitPoint");
            hitPoint.transform.SetParent(go.transform);
            hitPoint.transform.localPosition = collider.center;
        }

        // FirePoint (원거리만)
        if (data.projectilePrefab != null && go.transform.Find("FirePoint") == null)
        {
            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0, 1, 0);
        }

        // === HealthBar Canvas 생성 ===
        if (go.transform.Find("HealthBarCanvas") == null)
        {
            var healthBarCanvas = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasRenderer));
            healthBarCanvas.transform.SetParent(go.transform);

            float y = data.isMounted ? 3.2f : 2.0f;
            healthBarCanvas.transform.localPosition = new Vector3(0, y, 0);

            var canvas = healthBarCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            healthBarCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.3f);
            healthBarCanvas.transform.localScale = Vector3.one * 0.01f;

            // BG
            var bgGO = new GameObject("HealthBarBG", typeof(Image));
            bgGO.transform.SetParent(healthBarCanvas.transform, false);
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.color = Color.grey;
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(150, 20);

            // Fill
            var fillGO = new GameObject("HealthBarFill", typeof(Image));
            fillGO.transform.SetParent(bgGO.transform, false);
#if UNITY_EDITOR
            var fillSprite = AssetDatabase.FindAssets("t:Sprite UI_Fill_Green")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
                .FirstOrDefault();
#else
            Sprite fillSprite = null;
#endif
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.sprite = fillSprite;
            fillImg.color = Color.green;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1.0f;

            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var healthBar = fillGO.AddComponent<HealthBar>();
            healthBar.fillImage = fillImg;
        }

        AttachBillboardAndAssignCamera(go.transform);

        var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;

        Selection.activeGameObject = go;
    }

    private void AttachBillboardAndAssignCamera(Transform root)
    {
        var hbc = root.Find("HealthBarCanvas");
        if (hbc == null) return;

        var bb = hbc.GetComponent<Billboard>() ?? hbc.gameObject.AddComponent<Billboard>();

        var cam = FindPlayerCameraInRig();
        if (cam == null) return;

        var canvas = hbc.GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            canvas.worldCamera = cam;

        var so = new SerializedObject(bb);
        var prop = so.FindProperty("targetCamera");
        if (prop != null)
        {
            prop.objectReferenceValue = cam;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(bb);
        }
    }

    private Camera FindPlayerCameraInRig()
    {
        var rig = GameObject.Find("CameraRig");
        if (rig != null)
        {
            var pc = rig.transform.Find("PlayerCamera");
            if (pc != null)
            {
                var cam = pc.GetComponentInChildren<Camera>(true);
                if (cam != null) return cam;
            }
            var any = rig.GetComponentsInChildren<Camera>(true).FirstOrDefault();
            if (any != null) return any;
        }

        var all = Resources.FindObjectsOfTypeAll<Camera>()
            .Where(c => c.gameObject.scene.IsValid())
            .ToArray();
        var byName = all.FirstOrDefault(c => c.name == "PlayerCamera");
        if (byName != null) return byName;

        return Camera.main;
    }
}
#endif
