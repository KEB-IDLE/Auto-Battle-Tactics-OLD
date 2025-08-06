using UnityEngine;
using UnityEditor;

public class VFXAssignmentTool : EditorWindow
{
    [MenuItem("Tools/Assign VFX Trail Fire to Arrow")]
    public static void AssignVFXTrailFireToArrow()
    {
        // ArrowData 로드
        ProjectileData arrowData = AssetDatabase.LoadAssetAtPath<ProjectileData>(
            "Assets/Scripts/Game/Scriptable Object/Projectile/ArrowData.asset");
        
        if (arrowData == null)
        {
            Debug.LogError("ArrowData를 찾을 수 없습니다!");
            return;
        }

        // VFX_Trail_Fire 프리팹 로드
        GameObject vfxTrailFire = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Vefects/Trails VFX URP/VFX/Particles/VFX_Trail_Fire.prefab");
        
        if (vfxTrailFire == null)
        {
            Debug.LogError("VFX_Trail_Fire 프리팹을 찾을 수 없습니다!");
            return;
        }

        // Flight Effect 할당
        arrowData.FlightEffectPrefab = vfxTrailFire;
        
        // 변경사항 저장
        EditorUtility.SetDirty(arrowData);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✅ VFX_Trail_Fire가 ArrowData의 Flight Effect로 성공적으로 할당되었습니다!");
    }

    [MenuItem("Tools/Auto Setup Projectile Data References")]
    public static void AutoSetupProjectileDataReferences()
    {
        // 모든 EntityData 찾기
        string[] entityDataGuids = AssetDatabase.FindAssets("t:EntityData");
        int updatedCount = 0;

        foreach (string guid in entityDataGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EntityData entityData = AssetDatabase.LoadAssetAtPath<EntityData>(path);

            if (entityData != null && entityData.attackType == AttackType.Ranged && 
                entityData.projectilePrefab != null && entityData.projectileData == null)
            {
                // ProjectileData 찾기
                string projectileDataName = entityData.projectilePoolName + "Data";
                string[] projectileDataGuids = AssetDatabase.FindAssets(projectileDataName + " t:ProjectileData");
                
                if (projectileDataGuids.Length > 0)
                {
                    string projectileDataPath = AssetDatabase.GUIDToAssetPath(projectileDataGuids[0]);
                    ProjectileData projectileData = AssetDatabase.LoadAssetAtPath<ProjectileData>(projectileDataPath);
                    
                    if (projectileData != null)
                    {
                        entityData.projectileData = projectileData;
                        EditorUtility.SetDirty(entityData);
                        updatedCount++;
                        
                        Debug.Log($"✅ {entityData.name}에 {projectileData.name} 연결됨");
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"🎯 총 {updatedCount}개의 EntityData에 ProjectileData가 자동 설정되었습니다!");
    }
}