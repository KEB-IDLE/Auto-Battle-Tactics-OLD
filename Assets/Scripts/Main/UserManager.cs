using System;
using System.Collections;
using UnityEngine;


public class UserManager : MonoBehaviour
{
    public static UserManager Instance;

    public GameObject[] characterPrefabs;  // ����ĳ���� ������
    private GameObject currentCharacterInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ����ĳ���� ���� ����
    public IEnumerator SpawnCharacterCoroutine(int charId)
    {
        int prefabIndex = charId - 1;

        if (prefabIndex < 0 || prefabIndex >= characterPrefabs.Length)
        {
            Debug.LogWarning("Invalid character ID: " + charId);
            yield break;
        }

        if (currentCharacterInstance != null)
            Destroy(currentCharacterInstance);

        GameObject prefab = characterPrefabs[prefabIndex];
        currentCharacterInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        yield return null;  // �ڷ�ƾ ���� ����
    }

    // ĳ���� ���� �� UI ����
    public IEnumerator ChangeProfileCharacterCoroutine(int charId, Action onComplete = null)
    {
        yield return UserService.Instance.ChangeProfileCharacter(
            charId,
            profile =>
            {
                GameManager.Instance.profile = profile;
            },
            err => Debug.LogWarning("Main champion update failed: " + err)
        );

        yield return SpawnCharacterCoroutine(charId);

        onComplete?.Invoke();
    }



    // ���� ����
    public IEnumerator ChangeLevelCoroutine(int deltaLevel, Action onComplete = null)
    {
        int newLevel = GameManager.Instance.profile.level + deltaLevel;
        yield return UserService.Instance.ChangeLevel(
            newLevel,
            profile =>
            {
                GameManager.Instance.profile = profile;
                onComplete?.Invoke();
            },
            err => Debug.LogWarning("Level update failed: " + err)
        );
    }

    // ����ġ ����
    public IEnumerator ChangeExpCoroutine(int deltaExp, Action onComplete = null)
    {
        int newExp = GameManager.Instance.profile.exp + deltaExp;
        yield return UserService.Instance.ChangeExp(
            newExp,
            profile =>
            {
                GameManager.Instance.profile = profile;
                onComplete?.Invoke();
            },
            err => Debug.LogWarning("Exp update failed: " + err)
        );
    }

    // ��� ����
    public IEnumerator ChangeGoldCoroutine(int deltaGold, Action onComplete = null)
    {
        int newGold = GameManager.Instance.profile.gold + deltaGold;
        yield return UserService.Instance.ChangeGold(
            newGold,
            profile =>
            {
                GameManager.Instance.profile = profile;
                onComplete?.Invoke();
            },
            err => Debug.LogWarning("Gold update failed: " + err)
        );
    }




}