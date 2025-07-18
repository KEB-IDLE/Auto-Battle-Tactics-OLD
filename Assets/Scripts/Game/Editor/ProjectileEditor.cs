#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(ProjectileData))]
public class ProjectileDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ProjectileData data = (ProjectileData)target;

        if (GUILayout.Button("이 데이터로 Projectile 오브젝트 생성"))
        {
            CreateProjectileFromData(data);
        }
    }

    private void CreateProjectileFromData(ProjectileData data)
    {
        GameObject go;

        if (data.projectilePrefab != null)
            go = (GameObject)PrefabUtility.InstantiatePrefab(data.projectilePrefab);
        else
            go = new GameObject(data.name + "_Projectile");

        var projectile = go.GetComponent<Projectile>() ?? go.AddComponent<Projectile>();

        Selection.activeGameObject = go;
        Debug.Log($"{go.name} Projectile 생성됨 (데이터 드리븐)");
    }
}
#endif
