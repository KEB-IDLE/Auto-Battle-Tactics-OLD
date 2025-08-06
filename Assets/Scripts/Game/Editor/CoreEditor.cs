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
        GameObject go;

        // �������� ������ Instantiate, �ƴϸ� �� ������Ʈ
        if (data.objectPrefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(data.objectPrefab);
            go.name = data.name + "_Core";
        }
        else
            go = new GameObject(data.name + "_Core");

        // CoreComponent �Ҵ�
        var core = go.GetComponent<Core>() ?? go.AddComponent<Core>();
        // �ʿ��ϴٸ� CoreComponent�� �����͸� Set�ϴ� �޼��� ȣ��(����)
        core.SetData(data);

        // CapsuleCollider ����(�ɼ�)
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
            hitPoint.transform.localPosition = collider.center; // �Ǵ� ���ϴ� ��
        }

        //HealthBar UI(�ʿ��)
        //if (go.transform.Find("HealthBarCanvas") == null)
        //{
        //    var healthBarCanvas = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasRenderer));
        //    healthBarCanvas.transform.SetParent(go.transform);
        //    healthBarCanvas.transform.localPosition = new Vector3(0, 3.0f, 0); // �ھ� �Ӹ� ����
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
        //    // �ʿ��ϴٸ� HealthBar ��ũ��Ʈ ����
        //    var healthBar = fillGO.AddComponent<HealthBar>();
        //    healthBar.fillImage = fillImg;
        //}

        if (go.transform.Find("HealthBarCanvas") == null)
        {
            var healthBarCanvas = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasRenderer));
            healthBarCanvas.transform.SetParent(go.transform);
            healthBarCanvas.transform.localPosition = new Vector3(0, 3.0f, 0); // �ھ� �Ӹ� ����

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

            // 1. Sprite ���� ã�� (������Ʈ �� "UI_Fill_Red" �̸��� Sprite ���)
            var fillSprite = AssetDatabase.FindAssets("t:Sprite UI_Fill_Red")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
                .FirstOrDefault();
            fillImg.sprite = fillSprite;

            // 2. ���� �� Ÿ�� ���� (������ ���Ѵٸ� ������, ��������Ʈ ��ü �÷��� white ����)
            fillImg.color = Color.white;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1.0f; // ó���� ����

            // 3. RectTransform ����
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // 4. ��ũ��Ʈ ���̱�
            var healthBar = fillGO.AddComponent<HealthBar>();
            healthBar.fillImage = fillImg;
        }


        go.layer = LayerMask.NameToLayer("Core");
        Selection.activeGameObject = go;
        Debug.Log($"{go.name} Core ������Ʈ ������ (������ �帮��)");
    }
}




#endif
