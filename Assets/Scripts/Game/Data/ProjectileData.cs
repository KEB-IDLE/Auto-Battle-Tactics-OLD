using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Scriptable Objects/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    public GameObject projectilePrefab;
    public GameObject projectileAttackingEffectPrefab;
    public float speed;
    public float verticalSpeed;
    public float lifeTime;
    public float detectionRadius;
    public float explosionRadius;
}
