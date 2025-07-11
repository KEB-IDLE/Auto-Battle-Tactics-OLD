public static class APIEndpoints
{
    public const string AddExp = "/add-exp";
    public const string UpdateGold = "/update-gold";
    public static string Profile(int userId) => $"/profile/{userId}";
    public static string RankRecord(int userId) => $"/rank-record/{userId}";
}
