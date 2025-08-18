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
        // 에디터 환경/런타임 분기
        Team myTeam;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 에디터 환경에서는 강제 RED로 지정 (원하면 Blue 등으로 변경 가능)
            myTeam = Team.Red;
        }
        else
#endif
        {
            // 런타임에서는 실제 네트워크 값 사용
            myTeam = UserNetwork.Instance != null ? UserNetwork.Instance.MyTeam : Team.Red;
        }

        // ✅ 내 팀 가져오기
        //Team myTeam = UserNetwork.Instance.MyTeam;
        // ✅ 내 팀 프리팹 가져오기
        GameObject prefab = myTeam == Team.Red ? data.redPrefab : data.bluePrefab;

        if (prefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = data.entityName + "_Entity";
        }   
        else
            go = new GameObject(data.entityName + "_Entity");

        var entity = go.GetComponent<Entity>() ?? go.AddComponent<Entity>();
        entity.SetData(data);

        // EntityEditor.cs 내 CreateEntityInScene(data) 메서드 마지막에 추가
        var collider = go.GetComponent<CapsuleCollider>();
        if (collider == null)
            collider = go.AddComponent<CapsuleCollider>();
        collider.radius = 0.5f;
        collider.height = 1.8f;
        collider.center = new Vector3(0, 0.85f, 0); // 예시값

        if (go.transform.Find("HitPoint") == null)
        {
            GameObject hitPoint = new GameObject("HitPoint");
            hitPoint.transform.SetParent(go.transform);
            // 기본 위치 (예: 몸통 중앙에 오도록) → 필요에 따라 조정
            hitPoint.transform.localPosition = collider.center; // 또는 원하는 값
        }

        if (data.projectilePrefab != null && go.transform.Find("FirePoint") == null)
        {
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0, 1, 0);
        }

        // === HealthBar Canvas 생성 ===
        if (go.transform.Find("HealthBarCanvas") == null)
        {
            var healthBarCanvas = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasRenderer));
            healthBarCanvas.transform.SetParent(go.transform);

            // ▼ 여기 분기 삽입
            float y = data.isMounted ? 3.2f : 2.0f;   // EntityData에 isMounted 플래그가 있다면
            healthBarCanvas.transform.localPosition = new Vector3(0, y, 0);

            var canvas = healthBarCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

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

        var rb = go.GetComponent<Rigidbody>();
        if (rb == null)
            rb = go.AddComponent<Rigidbody>();

        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;

        Selection.activeGameObject = go;
    }
}
#endif