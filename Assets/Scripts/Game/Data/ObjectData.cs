using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "Scriptable Objects/ObjectData")]
public class ObjectData : ScriptableObject
{
    [Header("Entity Information")]
    public float maxHP;         // �ִ� ü��

    [Header("Entity Prefab")]
    public GameObject entityPrefab; // ��ƼƼ ������
    public GameObject summonEffectPrefab; // ��ȯ ����Ʈ ������
    public GameObject attackEffectPrefab; // ���� ����Ʈ ������
    public GameObject deathEffectPrefab; // ���� ����Ʈ ������

    [Header("Audio Settings")]
    public AudioBehaviour summonSound; // ��ȯ ����
    public AudioBehaviour attackSound; // ���� ����
    public AudioBehaviour deathSound;  // ���� ����
}
