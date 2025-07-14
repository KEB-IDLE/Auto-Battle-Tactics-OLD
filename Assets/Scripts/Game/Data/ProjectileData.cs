using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Scriptable Objects/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    public IDamageable target;
    public float speed;
    public float lifeTime;
    public float damage;
    public Entity owner;
}
