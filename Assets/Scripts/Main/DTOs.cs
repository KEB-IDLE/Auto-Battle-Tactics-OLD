


[System.Serializable]
public class UserRecord
{
    public int user_id;
    public int win;
    public int lose;
}

[System.Serializable]
public class Champion
{
    public int id;
    public string name;
    public string description;
}

[System.Serializable]
public class UserChampion
{
    public int user_id;
    public int champion_id;
    public Champion Champion;
}

[System.Serializable]
public class UserDeck
{
    public int id;
    public int user_id;
    public string deck_name;
    public int[] champion_ids;
}

[System.Serializable]
public class MatchHistory
{
    public int id;
    public int user_id;
    public int match_id;
    public string result;
    public string created_at;
}
