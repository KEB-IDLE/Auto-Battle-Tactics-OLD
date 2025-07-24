#if UNITY_EDITOR
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
        fillImg.color = Color.green; // ü�¹� ��
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

        Selection.activeGameObject = go;
    }
}
#endif
