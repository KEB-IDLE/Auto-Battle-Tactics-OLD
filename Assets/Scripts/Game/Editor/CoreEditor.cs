#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
[CustomEditor(typeof(ObjectData))]
public class CoreEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ObjectData data = (ObjectData)target;

        if (GUILayout.Button("이 데이터로 Core 생성"))
        {
            CreateCoreInScene(data);
        }
    }

    private void CreateCoreInScene(ObjectData data)
    {
        GameObject parent = new GameObject(data.name + "_core");
        GameObject child = null;

        if (data.objectPrefab != null)
        {
            child = (GameObject)PrefabUtility.InstantiatePrefab(data.objectPrefab);
            child.transform.SetParent(parent.transform, false); // 하위로 이동 (로컬 위치, 회전 유지)
            child.name = data.objectPrefab.name; // 원래 프리팹 이름 유지
        }

        var core = parent.GetComponent<Core>() ?? parent.AddComponent<Core>();
        // 필요하다면 CoreComponent에 데이터를 Set하는 메서드 호출(예시)
        core.SetData(data);

        // CapsuleCollider 세팅(옵션)
        var collider = parent.GetComponent<CapsuleCollider>();
        if (collider == null)
            collider = parent.AddComponent<CapsuleCollider>();

        collider.center = new Vector3(-3f, 4.4f, 0);
        collider.radius = 5f;
        collider.height = 11f;

        if (parent.transform.Find("HitPoint") == null)
        {
            GameObject hitPoint = new GameObject("HitPoint");
            hitPoint.transform.SetParent(parent.transform);
            hitPoint.transform.localPosition = new Vector3(0, 2f, 0); // 또는 원하는 값
        }

        if (parent.transform.Find("HealthBarCanvas") == null)
        {
            var healthBarCanvas = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasRenderer));
            healthBarCanvas.transform.SetParent(parent.transform);
            healthBarCanvas.transform.localPosition = new Vector3(-3f, 12f, 0);
            healthBarCanvas.transform.localRotation = Quaternion.Euler(0, 90, 0);

            var canvas = healthBarCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            healthBarCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.3f);
            healthBarCanvas.transform.localScale = Vector3.one * 0.01f;

            var bgGO = new GameObject("HealthBarBG", typeof(Image));
            bgGO.transform.SetParent(healthBarCanvas.transform, false);
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.color = Color.grey;
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(600, 80);

            var fillGO = new GameObject("HealthBarFill", typeof(Image));
            fillGO.transform.SetParent(bgGO.transform, false);
            var fillImg = fillGO.GetComponent<Image>();

            var fillSprite = AssetDatabase.FindAssets("t:Sprite UI_Fill_Red")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
                .FirstOrDefault();
            fillImg.sprite = fillSprite;

            fillImg.color = Color.white;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1.0f; // 처음엔 가득

            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var healthBar = fillGO.AddComponent<HealthBar>();
            healthBar.fillImage = fillImg;
        }
        AttachBillboardAndAssignCamera(parent.transform);

        parent.layer = LayerMask.NameToLayer("Core");
        Selection.activeGameObject = parent;
        Debug.Log($"{parent.name} Core 오브젝트 생성됨 (데이터 드리븐)");

    }
    private void AttachBillboardAndAssignCamera(Transform root)
    {
        var hbc = root.Find("HealthBarCanvas");
        if (hbc == null) return;

        // 1) Billboard 없으면 부착
        var bb = hbc.GetComponent<Billboard>();
        if (bb == null) bb = hbc.gameObject.AddComponent<Billboard>();

        // 2) CameraRig/PlayerCamera 탐색
        var cam = FindPlayerCameraInRig();
        if (cam == null) return;

        // 3) World Space Canvas에도 동일 카메라 설정(기존 Camera.main 대체)
        var canvas = hbc.GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            canvas.worldCamera = cam;

#if UNITY_EDITOR
        // 4) Billboard의 private serialized 필드(targetCamera)에 주입
        var so = new UnityEditor.SerializedObject(bb);
        var prop = so.FindProperty("targetCamera");
        if (prop != null)
        {
            prop.objectReferenceValue = cam;
            so.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(bb);
        }
#endif
    }

    private Camera FindPlayerCameraInRig()
    {
        // 우선 정확 경로 시도
        var rig = GameObject.Find("CameraRig");
        if (rig != null)
        {
            var pc = rig.transform.Find("PlayerCamera");
            if (pc != null)
            {
                var cam = pc.GetComponentInChildren<Camera>(true);
                if (cam != null) return cam;
            }
            // Rig 아래 첫 Camera
            var any = rig.GetComponentsInChildren<Camera>(true).FirstOrDefault();
            if (any != null) return any;
        }

        // 이름으로 전역 탐색(에디터에서 Prefab 포함될 수 있어 씬 객체만 필터)
        var all = Resources.FindObjectsOfTypeAll<Camera>()
            .Where(c => c.gameObject.scene.IsValid()) // 씬에 존재하는 객체만
            .ToArray();
        var byName = all.FirstOrDefault(c => c.name == "PlayerCamera");
        if (byName != null) return byName;

        // 마지막 폴백
        return Camera.main;
    }

}

#endif
