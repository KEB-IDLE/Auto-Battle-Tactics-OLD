#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

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

        if (data.projectilePrefab != null)
        {
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0, 1, 0);
        }
        Selection.activeGameObject = go;
    }
}
#endif
