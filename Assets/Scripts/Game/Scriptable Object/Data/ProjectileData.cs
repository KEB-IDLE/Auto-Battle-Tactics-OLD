using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Scriptable Objects/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    public float speed;
    public float lifeTime;
    public float detectionRadius;
    public float explosionRadius;
}
