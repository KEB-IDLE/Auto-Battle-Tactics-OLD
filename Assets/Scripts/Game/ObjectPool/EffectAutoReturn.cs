using UnityEngine;
using System.Collections;

public class EffectAutoReturn : MonoBehaviour
{
    private IObjectPool pool;
    private Coroutine returnRoutine;

    // ����Ʈ�� ����� �� ȣ��
    public void PlayAndReturn(IObjectPool pool, float duration)
    {
        this.pool = pool;
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnAfter(duration));
    }

    private IEnumerator ReturnAfter(float time)
    {
        yield return new WaitForSeconds(time);
        pool.Return(gameObject);
    }

    private void OnDisable()
    {
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);
    }
}
