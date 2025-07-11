using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class APIService : MonoBehaviour
{
    public static APIService Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    private string baseUrl = "http://localhost:3000/api/user";


    public IEnumerator Post<TReq, TRes>(string endpoint, TReq request, Action<TRes> onSuccess, Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(request);
        UnityWebRequest req = new UnityWebRequest($"{baseUrl}{endpoint}", "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

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
}
