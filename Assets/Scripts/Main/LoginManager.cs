using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button openRegisterButton;
    public TMP_Text statusText;

    public GameObject registerPanel;
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public Button registerSubmitButton;
    public Button registerCancelButton;
    public TMP_Text registerStatusText;
    public TMP_InputField registerNicknameInput;


    void Start()
    {

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
                if (!string.IsNullOrEmpty(res.token))
                {
                    GameManager.Instance.accessToken = res.token;
                    statusText.text = "Login Success!";
                    SceneManager.LoadScene(1);
                }
                else
                {
                    statusText.text = "Login failed: No token received";
                }
            },
           err =>
            {
                statusText.text = err.Contains("User not found") ? "User not found." :
                 err.Contains("Incorrect password") ? "Incorrect password." : "Login failed: " + err;
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
            registerStatusText.text = "모든 항목을 입력해주세요.";
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
                if (res.success)
                {
                    registerStatusText.text = "Register Success!";
                }
                else
                {
                    registerStatusText.text = res.message;
                }
            },
            err =>
            {
                registerStatusText.text = err.Contains("Email already exists") ? "Email already exists." :
                err.Contains("Nickname already exists") ? "Nickname already exists." : "Register failed: " + err;
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
