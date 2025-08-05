using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APIService : MonoBehaviour
{
    public static APIService Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    //private string baseUrl = "http://localhost:3000/api";
    private string baseUrl = "https://jamsik.p-e.kr/api";

    public IEnumerator Post<TReq, TRes>(string endpoint, TReq request, Action<TRes> onSuccess, Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(request);
        UnityWebRequest req = new UnityWebRequest($"{baseUrl}{endpoint}", "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        SetAuthHeader(req); // 토큰 설정

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

    public IEnumerator Get<TRes>(string endpoint, Action<TRes> onSuccess, Action<string> onError = null)
    {
        UnityWebRequest req = UnityWebRequest.Get($"{baseUrl}{endpoint}");
        SetAuthHeader(req); // 토큰 설정

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

    public IEnumerator GetList<T>(string endpoint, Action<List<T>> onSuccess, Action<string> onError = null)
    {
        UnityWebRequest req = UnityWebRequest.Get($"{baseUrl}{endpoint}");
        SetAuthHeader(req); // 토큰 설정

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



    public IEnumerator Put<TReq, TRes>(string endpoint, TReq request, Action<TRes> onSuccess, Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(request);
        UnityWebRequest req = new UnityWebRequest($"{baseUrl}{endpoint}", "PUT");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        SetAuthHeader(req); // JWT 토큰 추가

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

    private void SetAuthHeader(UnityWebRequest req)
    {
        string token = GameManager.Instance.accessToken;
        if (!string.IsNullOrEmpty(token))
        {
            req.SetRequestHeader("Authorization", $"Bearer {token}");
        }
    }

}

