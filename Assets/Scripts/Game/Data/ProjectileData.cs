using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Scriptable Objects/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    [Header("�߻�ü ������")]
    public GameObject projectilePrefab;
    [Header("�߻� �� ���� ����Ʈ")]
    public GameObject FlightEffectPrefab;
    [Header("���� �̵��ӵ� ����ġ")]
    public float speedWeight;
    [Header("���� �̵��ӵ� ����ġ")]
    public float verticalSpeedWeight;
    
    public float lifeTime;
    public float detectionRadius;
    public float explosionRadius;
}
