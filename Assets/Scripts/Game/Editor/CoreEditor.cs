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

        if (GUILayout.Button("�� �����ͷ� Core ����"))
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
            child.transform.SetParent(parent.transform, false); // ������ �̵� (���� ��ġ, ȸ�� ����)
            child.name = data.objectPrefab.name; // ���� ������ �̸� ����
        }

        var core = parent.GetComponent<Core>() ?? parent.AddComponent<Core>();
        // �ʿ��ϴٸ� CoreComponent�� �����͸� Set�ϴ� �޼��� ȣ��(����)
        core.SetData(data);

        // CapsuleCollider ����(�ɼ�)
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
            hitPoint.transform.localPosition = new Vector3(0, 2f, 0); // �Ǵ� ���ϴ� ��
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
            fillImg.fillAmount = 1.0f; // ó���� ����

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
        Debug.Log($"{parent.name} Core ������Ʈ ������ (������ �帮��)");

    }
    private void AttachBillboardAndAssignCamera(Transform root)
    {
        var hbc = root.Find("HealthBarCanvas");
        if (hbc == null) return;

        // 1) Billboard ������ ����
        var bb = hbc.GetComponent<Billboard>();
        if (bb == null) bb = hbc.gameObject.AddComponent<Billboard>();

        // 2) CameraRig/PlayerCamera Ž��
        var cam = FindPlayerCameraInRig();
        if (cam == null) return;

        // 3) World Space Canvas���� ���� ī�޶� ����(���� Camera.main ��ü)
        var canvas = hbc.GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            canvas.worldCamera = cam;

#if UNITY_EDITOR
        // 4) Billboard�� private serialized �ʵ�(targetCamera)�� ����
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
        // �켱 ��Ȯ ��� �õ�
        var rig = GameObject.Find("CameraRig");
        if (rig != null)
        {
            var pc = rig.transform.Find("PlayerCamera");
            if (pc != null)
            {
                var cam = pc.GetComponentInChildren<Camera>(true);
                if (cam != null) return cam;
            }
            // Rig �Ʒ� ù Camera
            var any = rig.GetComponentsInChildren<Camera>(true).FirstOrDefault();
            if (any != null) return any;
        }

        // �̸����� ���� Ž��(�����Ϳ��� Prefab ���Ե� �� �־� �� ��ü�� ����)
        var all = Resources.FindObjectsOfTypeAll<Camera>()
            .Where(c => c.gameObject.scene.IsValid()) // ���� �����ϴ� ��ü��
            .ToArray();
        var byName = all.FirstOrDefault(c => c.name == "PlayerCamera");
        if (byName != null) return byName;

        // ������ ����
        return Camera.main;
    }

}

#endif
