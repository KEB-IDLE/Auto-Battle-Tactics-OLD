using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Scriptable Objects/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    [Header("발사체 프리팹")]
    public GameObject projectilePrefab;
    [Header("발사 중 보일 이펙트")]
    public GameObject FlightEffectPrefab;
    [Header("수평 이동속도 가중치")]
    public float speedWeight;
    [Header("수직 이동속도 가중치")]
    public float verticalSpeedWeight;
    //[Header("발사체 유도 정도 조절")]

    [Header("회전 보정값(로컬 오프셋, 필요 시 사용)")]
    public Vector3 localRotationOffset = Vector3.zero;   // (X, Y, Z)


    public float detectionRadius;
    public float explosionRadius;
}