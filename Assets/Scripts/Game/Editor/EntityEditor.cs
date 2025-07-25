#if UNITY_EDITOR
using System.Linq;
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

        if (GUILayout.Button("�� �����ͷ� Entity ����"))
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

        if (data.projectilePrefab != null)
        {
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0, 1, 0);
        }

        // === HealthBar Canvas ���� ===
        var healthBarCanvas = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasRenderer));
        healthBarCanvas.transform.SetParent(go.transform);
        healthBarCanvas.transform.localPosition = new Vector3(0, 2.0f, 0); // �Ӹ� ���� ��ġ(���� ����)

        var canvas = healthBarCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main; // ���� ī�޶� �ϳ� �ִٰ� ����

        // Canvas ũ��, ������ ����
        healthBarCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.3f);
        healthBarCanvas.transform.localScale = Vector3.one * 0.01f; // ũ�� ���

        // === HealthBar(Background) ===
        var bgGO = new GameObject("HealthBarBG", typeof(Image));
        bgGO.transform.SetParent(healthBarCanvas.transform, false);
        var bgImg = bgGO.GetComponent<Image>();
        bgImg.color = Color.grey; // ����
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(150, 20);

        // === HealthBar(Fill) ===
        var fillGO = new GameObject("HealthBarFill", typeof(Image));
        fillGO.transform.SetParent(bgGO.transform, false);
        var fillImg = fillGO.GetComponent<Image>();

        // 1. Sprite ���� ã�� (������Ʈ �� "UI_Fill_Green" �̸��� Sprite ���)
        var fillSprite = AssetDatabase.FindAssets("t:Sprite UI_Fill_Green")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
            .FirstOrDefault();
        fillImg.sprite = fillSprite;

        // 2. ���� �� Ÿ�� ����
        fillImg.color = Color.green; // Ȥ�� �ʿ��ϸ� ���ϴ� ��������
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 1.0f; // ó���� ����

        // 3. RectTransform ����
        var fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // EntityEditor.cs �� CreateEntityInScene(data) �޼��� �������� �߰�
        var collider = go.GetComponent<CapsuleCollider>();
        collider.radius = 0.5f;
        collider.height = 1.8f;
        collider.center = new Vector3(0, 0.85f, 0); // ���ð�

        // 4. ��ũ��Ʈ ���̱�
        var healthBar = fillGO.AddComponent<HealthBar>();
        healthBar.fillImage = fillImg;

        var rb = go.AddComponent<Rigidbody>();
        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;

        Selection.activeGameObject = go;
    }
}
#endif
