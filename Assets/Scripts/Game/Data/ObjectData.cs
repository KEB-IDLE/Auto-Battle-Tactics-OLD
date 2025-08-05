using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "Scriptable Objects/ObjectData")]
public class ObjectData : ScriptableObject
{
    [Header("Object Information")]
    public float maxHP;         // �ִ� ü��

    [Header("Object Prefab")]
    public GameObject objectPrefab; // ��ƼƼ ������
    public GameObject takeDamageEffectPrefab; // ���� ����Ʈ ������
    public GameObject deathEffectPrefab; // ���� ����Ʈ ������

    [Header("Audio Settings")]
    public AudioBehaviour takeDamageSound; // ���� ����
    public AudioBehaviour deathSound;  // ���� ����
}
