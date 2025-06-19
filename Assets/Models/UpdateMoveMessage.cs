[System.Serializable]
public class BaseSocketMessage
{
    public string type;
}

[System.Serializable]
public class UpdateMoveMessage : BaseSocketMessage
{
    public int playerId;
    public float x;
    public float y;
    public string direction;
}

[System.Serializable]
public class PlayerOnlineMessage : BaseSocketMessage
{
    public int playerId;
    public string playerName;
    public float x;
    public float y;
    public string direction;
}
