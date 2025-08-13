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
        collider.radius = 4f;
        collider.height = 11f;

        if (parent.transform.Find("HitPoint") == null)
        {
            GameObject hitPoint = new GameObject("HitPoint");
            hitPoint.transform.SetParent(parent.transform);
            hitPoint.transform.localPosition = collider.center; // 또는 원하는 값
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


        parent.layer = LayerMask.NameToLayer("Core");
        Selection.activeGameObject = parent;
        Debug.Log($"{parent.name} Core 오브젝트 생성됨 (데이터 드리븐)");
    }
}

#endif
