using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 서버와의 HTTP 통신을 담당하는 싱글턴 클래스입니다.
/// POST, GET, PUT 메서드를 통해 서버 API를 호출합니다.
/// </summary>
public class APIService : MonoBehaviour
{
    public static APIService Instance { get; private set; }

    //private string baseUrl = "http://localhost:3000/api";   // 로컬 개발용 API 기본 URL
    private string baseUrl = "https://jamsik.p-e.kr/api"; // 배포 서버용 API 기본 URL

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// POST 요청을 보내고 응답을 처리합니다.
    /// </summary>
    public IEnumerator Post<TReq, TRes>(string endpoint, TReq request, Action<TRes> onSuccess, Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(request);
        UnityWebRequest req = new UnityWebRequest($"{baseUrl}{endpoint}", "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        SetAuthHeader(req);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            TRes res = JsonUtility.FromJson<TRes>(req.downloadHandler.text);
            onSuccess?.Invoke(res);
        }
        else
        {
            onError?.Invoke(req.downloadHandler.text);
        }
    }

    /// <summary>
    /// GET 요청을 보내고 응답을 처리합니다.
    /// </summary>
    public IEnumerator Get<TRes>(string endpoint, Action<TRes> onSuccess, Action<string> onError = null)
    {
        UnityWebRequest req = UnityWebRequest.Get($"{baseUrl}{endpoint}");
        SetAuthHeader(req);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            TRes res = JsonUtility.FromJson<TRes>(req.downloadHandler.text);
            onSuccess?.Invoke(res);
        }
        else
        {
            onError?.Invoke(req.downloadHandler.text);
        }
    }

    /// <summary>
    /// GET 요청을 보내고 JSON 배열 응답을 리스트로 변환하여 처리합니다.
    /// </summary>
    public IEnumerator GetList<T>(string endpoint, Action<List<T>> onSuccess, Action<string> onError = null)
    {
        UnityWebRequest req = UnityWebRequest.Get($"{baseUrl}{endpoint}");
        SetAuthHeader(req);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = req.downloadHandler.text;
            string wrappedJson = "{\"list\":" + json + "}"; // JSON 배열을 객체로 래핑

            var wrapper = JsonUtility.FromJson<ListWrapper<T>>(wrappedJson);
            onSuccess?.Invoke(wrapper.list);
        }
        else
        {
            onError?.Invoke(req.downloadHandler.text);
        }
    }

    [System.Serializable]
    private class ListWrapper<T>
    {
        public List<T> list;
    }

    /// <summary>
    /// PUT 요청을 보내고 응답을 처리합니다.
    /// </summary>
    public IEnumerator Put<TReq, TRes>(string endpoint, TReq request, Action<TRes> onSuccess, Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(request);
        UnityWebRequest req = new UnityWebRequest($"{baseUrl}{endpoint}", "PUT");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        SetAuthHeader(req);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            TRes res = JsonUtility.FromJson<TRes>(req.downloadHandler.text);
            onSuccess?.Invoke(res);
        }
        else
        {
            onError?.Invoke(req.downloadHandler.text);
        }
    }

    /// <summary>
    /// Authorization 헤더에 JWT 토큰을 설정합니다.
    /// </summary>
    private void SetAuthHeader(UnityWebRequest req)
    {
        string token = SessionManager.Instance.accessToken;
        if (!string.IsNullOrEmpty(token))
        {
            req.SetRequestHeader("Authorization", $"Bearer {token}");
        }
    }
}
