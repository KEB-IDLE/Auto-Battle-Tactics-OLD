using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button openRegisterButton;
    public TMP_Text statusText;

    public GameObject registerPanel;
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerNicknameInput;
    public Button registerSubmitButton;
    public Button registerCancelButton;
    public TMP_Text registerStatusText;

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

    public void OnLoginClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Input required.";
            return;
        }

        var loginReq = new LoginRequest { email = email, password = password };

        StartCoroutine(APIService.Instance.Post<LoginRequest, LoginResponse>(
            APIEndpoints.Login,
            loginReq,
            res =>
            {
                if (string.IsNullOrEmpty(res.token))
                {
                    statusText.text = "Login failed: No token received.";
                    return;
                }

                SessionManager.Instance.accessToken = res.token;
                statusText.text = "Login Success!";
                SceneManager.LoadScene("1-MainScene");
            },
            err =>
            {
                if (err.Contains("User not found"))
                    statusText.text = "User not found.";
                else if (err.Contains("Incorrect password"))
                    statusText.text = "Incorrect password.";
                else
                    statusText.text = "Login failed: " + err;
            }
        ));
    }

    public void OnRegisterSubmit()
    {
        string email = registerEmailInput.text;
        string password = registerPasswordInput.text;
        string nickname = registerNicknameInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(nickname))
        {
            registerStatusText.text = "Please fill in all fields.";
            return;
        }

        var registerReq = new RegisterRequest
        {
            email = email,
            password = password,
            nickname = nickname
        };

        StartCoroutine(APIService.Instance.Post<RegisterRequest, ResponseData>(
            APIEndpoints.Register,
            registerReq,
            res =>
            {
                registerStatusText.text = res.success ? "Register Success!" : res.message;
            },
            err =>
            {
                if (err.Contains("Email already exists"))
                    registerStatusText.text = "Email already exists.";
                else if (err.Contains("Nickname already exists"))
                    registerStatusText.text = "Nickname already exists.";
                else
                    registerStatusText.text = "Register failed: " + err;
            }
        ));
    }
}

[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[System.Serializable]
public class RegisterRequest
{
    public string email;
    public string password;
    public string nickname;
}

[System.Serializable]
public class LoginResponse
{
    public string token;
}

[System.Serializable]
public class ResponseData
{
    public bool success;
    public string message;
}
