using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 로그인 및 회원가입 UI를 관리하고, 서버와의 인증 통신을 수행하는 매니저 클래스입니다.
/// </summary>
public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    // === 로그인 UI 요소 ===
    public TMP_InputField emailInput;        // 로그인 이메일 입력 필드
    public TMP_InputField passwordInput;     // 로그인 비밀번호 입력 필드
    public TMP_Text statusText;              // 로그인 상태 메시지 출력 텍스트

    // === 회원가입 UI 요소 ===
    public TMP_InputField registerEmailInput;    // 회원가입 이메일 입력 필드
    public TMP_InputField registerPasswordInput; // 회원가입 비밀번호 입력 필드
    public TMP_InputField registerNicknameInput; // 회원가입 닉네임 입력 필드
    public TMP_Text registerStatusText;          // 회원가입 상태 메시지 출력 텍스트

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
    /// 로그인 버튼 클릭 시 호출됩니다. 입력값을 검증하고 서버에 로그인 요청을 보냅니다.
    /// 성공 시 토큰을 저장하고 메인 씬으로 전환합니다.
    /// </summary>
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

    /// <summary>
    /// 회원가입 제출 버튼 클릭 시 호출됩니다. 입력값을 검증하고 서버에 회원가입 요청을 보냅니다.
    /// 성공 여부에 따라 상태 메시지를 갱신합니다.
    /// </summary>
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

        StartCoroutine(APIService.Instance.Post<RegisterRequest, RegisterResponse>(
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
public class LoginResponse
{
    public string token;
}

[System.Serializable]
public class RegisterRequest
{
    public string email;
    public string password;
    public string nickname;
}

[System.Serializable]
public class RegisterResponse
{
    public bool success;
    public string message;
}