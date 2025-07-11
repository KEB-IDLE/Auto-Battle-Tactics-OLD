using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    // 로그인 패널
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button openRegisterButton;
    public TMP_Text statusText;

    // 회원가입 패널
    public GameObject registerPanel;
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public Button registerSubmitButton;
    public Button registerCancelButton;
    public TMP_Text registerStatusText;

    void Start()
    {

    }

    // 로그인
    public void OnLoginClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Input required.";
            return;
        }

        StartCoroutine(Login(email, password));
    }

    // 회원가입창 열기
    public void OpenRegisterPopup()
    {
        registerPanel.SetActive(true);
    }

    // 회원가입 요청
    public void OnRegisterSubmit()
    {
        string email = registerEmailInput.text;
        string password = registerPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Input required.";
            return;
        }

        StartCoroutine(Register(email, password));
    }

    IEnumerator Login(string email, string password)
    {
        string url = "http://localhost:3000/api/user/login";
        yield return SendRequest(url, email, password, () =>
        {
            statusText.text = "Login Success!";
            SceneManager.LoadScene("MainScene");
        });
    }

    IEnumerator Register(string email, string password)
    {
        string url = "http://localhost:3000/api/user/register";
        yield return SendRequest(url, email, password, () =>
        {
            registerStatusText.text = "Register Success!";
        });
    }

    IEnumerator SendRequest(string url, string email, string password, System.Action onSuccess)
    {
        LoginRequest requestData = new LoginRequest { email = email, password = password };
        string json = JsonUtility.ToJson(requestData);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke();
        }
        else
        {
            string responseText = req.downloadHandler.text;
            ResponseData resData = JsonUtility.FromJson<ResponseData>(responseText);
            statusText.text = resData.message;
        }
    }
}

[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[System.Serializable]
public class ResponseData
{
    public bool success;
    public string message;
}