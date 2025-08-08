/// <summary>
/// 클라이언트에서 사용하는 서버 API 엔드포인트들을 상수로 정의한 클래스입니다.
/// 유지보수와 오타 방지를 위해 사용합니다.
/// </summary>
public static class APIEndpoints
{
    public const string Register = "/auth/register";
    public const string Login = "/auth/login";

    public const string Profile = "/user";

    public const string ProfileIcon = "/user/icon";
    public const string ProfileCharacter = "/user/character";
    public const string ProfileLevel = "/user/level";
    public const string ProfileExp = "/user/exp";
    public const string ProfileGold = "/user/gold";

    public const string ProfileIcons = "/user/icons";

    public const string Record = "/user/record";

    public const string GlobalRanking = "/ranking";

    //public const string MatchHistory = "/user/match/history";
}