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

[System.Serializable]
public class InitProfileRequest
{
    public string nickname;
    public int profile_icon_id;
    public int main_champion_id;
}

[System.Serializable]
public class GenericResponse
{
    public bool success;
    public string message;
}
