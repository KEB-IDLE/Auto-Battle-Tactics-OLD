using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Information")]
    public float damage;
    public float speed;
    public float LifeTime;
    public GameObject weaponPrefab;
    public float attackRange;

}
