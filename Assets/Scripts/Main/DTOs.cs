[System.Serializable]
public class AddExpRequest { public int userId; public int amount; }
[System.Serializable]
public class UpdateGoldRequest { public int userId; public int amount; }

[System.Serializable]
public class GenericResponse
{
    public bool success;
    public int exp;
    public int gold;
}

[System.Serializable]
public class UpdateGoldResponse
{
    public bool success;
    public int newGold;
}

[System.Serializable]
public class ProfileResponse
{
    public bool success;
    public UserData user;
}

[System.Serializable]
public class RankRecordResponse
{
    public bool success;
    //public RankRecord record;
}
