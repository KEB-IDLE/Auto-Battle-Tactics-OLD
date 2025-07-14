using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "Scriptable Objects/ObjectData")]
public class ObjectData : ScriptableObject
{
    [Header("Entity Information")]
    public float maxHP;         // 최대 체력

    [Header("Entity Prefab")]
    public GameObject entityPrefab; // 엔티티 프리팹
    public GameObject summonEffectPrefab; // 소환 이펙트 프리팹
    public GameObject attackEffectPrefab; // 공격 이펙트 프리팹
    public GameObject deathEffectPrefab; // 죽음 이펙트 프리팹

    [Header("Audio Settings")]
    public AudioBehaviour summonSound; // 소환 사운드
    public AudioBehaviour attackSound; // 공격 사운드
    public AudioBehaviour deathSound;  // 죽음 사운드
}
