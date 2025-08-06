using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "Scriptable Objects/ObjectData")]
public class ObjectData : ScriptableObject
{
    [Header("Object Information")]
    public float maxHP;         // 최대 체력

    [Header("Object Prefab")]
    public GameObject objectPrefab; // 엔티티 프리팹
    public GameObject takeDamageEffectPrefab; // 공격 이펙트 프리팹
    public GameObject deathEffectPrefab; // 죽음 이펙트 프리팹

    [Header("Audio Settings")]
    public AudioBehaviour takeDamageSound; // 공격 사운드
    public AudioBehaviour deathSound;  // 죽음 사운드
}
