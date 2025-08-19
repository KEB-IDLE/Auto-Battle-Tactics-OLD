//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// Visual effect for champions
///// </summary>
//public class Effect : MonoBehaviour
//{
//    public GameObject effectPrefab;

//    /// How long the effect should last in secounds
//    public float duration;
//    private GameObject championGO;
//    private GameObject effectGO;

//    /// Update is called once per frame
//    void Update()
//    {
//        duration -= Time.deltaTime;

//        if (duration < 0)
//            championGO.GetComponent<ChampionController>().RemoveEffect(this);
//    }

//    /// <summary>
//    ///  Called when effect is created the first time
//    /// </summary>
//    /// <param name="_effectPrefab"></param>
//    /// <param name="_championGO"></param>
//    /// <param name="_duration"></param>
//    public void Init(GameObject _effectPrefab, GameObject _championGO, float _duration)
//    {
//        effectPrefab = _effectPrefab;
//        duration = _duration;
//        championGO = _championGO;

//        effectGO = Instantiate(effectPrefab);
//        effectGO.transform.SetParent(championGO.transform);
//        effectGO.transform.localPosition = Vector3.zero;
//    }

//    /// <summary>
//    /// Called when the effect expired
//    /// </summary>
//    public void Remove()
//    {
//        Destroy(effectGO);
//        Destroy(this);
//    }
//}

// Effect.cs (리팩터링 버전)



using UnityEngine;

/// <summary>
/// Visual effect for champions (pooled)
/// </summary>
public class Effect : MonoBehaviour
{
    public GameObject effectPrefab;

    /// How long the effect should last in seconds
    public float duration;

    private GameObject championGO;
    private GameObject effectGO;
    private float t;

    void OnEnable() => t = 0f;

    void Update()
    {
        t += Time.deltaTime;
        if (t >= duration)
        {
            // 기존 흐름 유지: 챔피언 쪽에서 RemoveEffect(this) 호출 → Effect.Remove() 실행
            var cc = championGO ? championGO.GetComponent<ChampionController>() : null;
            if (cc != null) cc.RemoveEffect(this);
            else Remove(); // 안전망
        }
    }

    /// <summary> Called when effect is created the first time </summary>
    public void Init(GameObject _effectPrefab, GameObject _championGO, float _duration)
    {
        effectPrefab = _effectPrefab;
        championGO = _championGO;
        duration = Mathf.Max(0f, _duration);
        t = 0f;

        // ⬇️ Instantiate → 컨트롤러 풀 사용
        if (GamePlayController.Instance != null)
        {
            effectGO = GamePlayController.Instance.SpawnFromPool(
                effectPrefab,
                championGO.transform.position,
                Quaternion.identity,
                championGO.transform  // 챔피언 하위로 붙여 "따라다니게" 유지
            );
            effectGO.transform.localPosition = Vector3.zero;
        }
        else
        {
            // 폴백(에디터 단독 실행 등)
            effectGO = Instantiate(effectPrefab, championGO.transform);
            effectGO.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary> Called when the effect expired </summary>
    public void Remove()
    {
        if (effectGO != null)
        {
            if (GamePlayController.Instance != null)
                GamePlayController.Instance.ReturnToPool(effectGO);
            else
                Destroy(effectGO); // 폴백
            effectGO = null;
        }

        // 기존 동작과 동일하게 이 컴포넌트는 제거
        Destroy(this);
    }
}

