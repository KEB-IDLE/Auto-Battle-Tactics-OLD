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
        GameObject go;

        // 프리팹이 있으면 Instantiate, 아니면 새 오브젝트
        if (data.objectPrefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(data.objectPrefab);
            go.name = data.name + "_Core";
        }
        else
            go = new GameObject(data.name + "_Core");

        // CoreComponent 할당
        var core = go.GetComponent<Core>() ?? go.AddComponent<Core>();
        // 필요하다면 CoreComponent에 데이터를 Set하는 메서드 호출(예시)
        core.SetData(data);

        // CapsuleCollider 세팅(옵션)
        var collider = go.GetComponent<CapsuleCollider>();
        if (collider == null)
            collider = go.AddComponent<CapsuleCollider>();
        collider.radius = 0.6f;
        collider.height = 2.5f;
        collider.center = new Vector3(0, 0, 0);

        if (go.transform.Find("HitPoint") == null)
        {
            GameObject hitPoint = new GameObject("HitPoint");
            hitPoint.transform.SetParent(go.transform);
            hitPoint.transform.localPosition = collider.center; // 또는 원하는 값
        }

        //HealthBar UI(필요시)
        //if (go.transform.Find("HealthBarCanvas") == null)
        //{
        //    var healthBarCanvas = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasRenderer));
        //    healthBarCanvas.transform.SetParent(go.transform);
        //    healthBarCanvas.transform.localPosition = new Vector3(0, 3.0f, 0); // 코어 머리 위로
        //    var canvas = healthBarCanvas.GetComponent<Canvas>();
        //    canvas.renderMode = RenderMode.WorldSpace;
        //    canvas.worldCamera = Camera.main;
        //    healthBarCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(2.0f, 0.5f);
        //    healthBarCanvas.transform.localScale = Vector3.one * 0.01f;

        //    var bgGO = new GameObject("HealthBarBG", typeof(UnityEngine.UI.Image));
        //    bgGO.transform.SetParent(healthBarCanvas.transform, false);
        //    var bgImg = bgGO.GetComponent<UnityEngine.UI.Image>();
        //    bgImg.color = Color.grey;
        //    var bgRect = bgGO.GetComponent<RectTransform>();
        //    bgRect.sizeDelta = new Vector2(200, 30);

        //    var fillGO = new GameObject("HealthBarFill", typeof(UnityEngine.UI.Image));
        //    fillGO.transform.SetParent(bgGO.transform, false);
        //    var fillImg = fillGO.GetComponent<UnityEngine.UI.Image>();
        //    fillImg.color = Color.red;
        //    fillImg.type = UnityEngine.UI.Image.Type.Filled;
        //    fillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        //    fillImg.fillAmount = 1.0f;
        //    var fillRect = fillGO.GetComponent<RectTransform>();
        //    fillRect.anchorMin = new Vector2(0, 0);
        //    fillRect.anchorMax = new Vector2(1, 1);
        //    fillRect.offsetMin = Vector2.zero;
        //    fillRect.offsetMax = Vector2.zero;
        //    // 필요하다면 HealthBar 스크립트 부착
        //    var healthBar = fillGO.AddComponent<HealthBar>();
        //    healthBar.fillImage = fillImg;
        //}

        if (go.transform.Find("HealthBarCanvas") == null)
        {
            var healthBarCanvas = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasRenderer));
            healthBarCanvas.transform.SetParent(go.transform);
            healthBarCanvas.transform.localPosition = new Vector3(0, 3.0f, 0); // 코어 머리 위로

            var canvas = healthBarCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            healthBarCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.3f);
            healthBarCanvas.transform.localScale = Vector3.one * 0.01f;

            // === HealthBar(Background) ===
            var bgGO = new GameObject("HealthBarBG", typeof(Image));
            bgGO.transform.SetParent(healthBarCanvas.transform, false);
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.color = Color.grey;
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(150, 20);

            // === HealthBar(Fill) ===
            var fillGO = new GameObject("HealthBarFill", typeof(Image));
            fillGO.transform.SetParent(bgGO.transform, false);
            var fillImg = fillGO.GetComponent<Image>();

            // 1. Sprite 에셋 찾기 (프로젝트 내 "UI_Fill_Red" 이름의 Sprite 사용)
            var fillSprite = AssetDatabase.FindAssets("t:Sprite UI_Fill_Red")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
                .FirstOrDefault();
            fillImg.sprite = fillSprite;

            // 2. 색상 및 타입 세팅 (색상은 원한다면 빨간색, 스프라이트 자체 컬러면 white 권장)
            fillImg.color = Color.white;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1.0f; // 처음엔 가득

            // 3. RectTransform 맞춤
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // 4. 스크립트 붙이기
            var healthBar = fillGO.AddComponent<HealthBar>();
            healthBar.fillImage = fillImg;
        }


        go.layer = LayerMask.NameToLayer("Core");
        Selection.activeGameObject = go;
        Debug.Log($"{go.name} Core 오브젝트 생성됨 (데이터 드리븐)");
    }
}




#endif
