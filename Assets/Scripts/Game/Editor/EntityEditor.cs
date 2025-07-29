#if UNITY_EDITOR
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[CustomEditor(typeof(EntityData))]
public class EntityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EntityData data = (EntityData)target;

        if (GUILayout.Button("이 데이터로 Entity 생성"))
            CreateEntityInScene(data);
    }

    private void CreateEntityInScene(EntityData data)
    {
        GameObject go;

        if (data.entityPrefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(data.entityPrefab);
            go.name = data.entityName + "_Entity";
        }
        else
            go = new GameObject(data.entityName + "_Entity");

        var entity = go.GetComponent<Entity>() ?? go.AddComponent<Entity>();
        entity.SetData(data);

        if (data.projectilePrefab != null && go.transform.Find("FirePoint") == null)
        {
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0, 1, 0);
        }

        // === HealthBar Canvas 생성 ===
        if(go.transform.Find("HealthBarCanvas") == null)
        {
            var healthBarCanvas = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasRenderer));
            healthBarCanvas.transform.SetParent(go.transform);
            healthBarCanvas.transform.localPosition = new Vector3(0, 2.0f, 0); // 머리 위에 위치(조정 가능)

            var canvas = healthBarCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main; // 씬에 카메라 하나 있다고 가정

            // Canvas 크기, 스케일 조정
            healthBarCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.3f);
            healthBarCanvas.transform.localScale = Vector3.one * 0.01f; // 크기 축소

            // === HealthBar(Background) ===
            var bgGO = new GameObject("HealthBarBG", typeof(Image));
            bgGO.transform.SetParent(healthBarCanvas.transform, false);
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.color = Color.grey; // 배경색
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(150, 20);

            // === HealthBar(Fill) ===
            var fillGO = new GameObject("HealthBarFill", typeof(Image));
            fillGO.transform.SetParent(bgGO.transform, false);
            var fillImg = fillGO.GetComponent<Image>();

            // 1. Sprite 에셋 찾기 (프로젝트 내 "UI_Fill_Green" 이름의 Sprite 사용)
            var fillSprite = AssetDatabase.FindAssets("t:Sprite UI_Fill_Green")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
                .FirstOrDefault();
            fillImg.sprite = fillSprite;

            // 2. 색상 및 타입 세팅
            fillImg.color = Color.green; // 혹시 필요하면 원하는 색상으로
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
        
        // EntityEditor.cs 내 CreateEntityInScene(data) 메서드 마지막에 추가
        var collider = go.GetComponent<CapsuleCollider>();
        collider.radius = 0.5f;
        collider.height = 1.8f;
        collider.center = new Vector3(0, 0.85f, 0); // 예시값

        var rb = go.AddComponent<Rigidbody>();
        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;

        Selection.activeGameObject = go;
    }
}
#endif
