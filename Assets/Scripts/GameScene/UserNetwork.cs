using UnityEngine;
using NativeWebSocket;
using System.Text;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UserNetwork : MonoBehaviour
{
    public string MyId => userId;      // 예전 코드 호환
    public string ClientId => userId;  // 혹시 다른 곳에서 ClientId를 참조할 수도 있어 대비

    public static UserNetwork Instance { get; private set; }
    private Queue<string> pendingInitMessages = new();

    public string userId;

    public Team MyTeam { get; private set; }
    public bool IsTeamReady { get; private set; } = false;
    private WebSocket socket;
    public static WebSocket GetSocket() => Instance?.socket;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        userId = SessionManager.Instance.profile.user_id.ToString();
    }

    private string websocketUrl = "ws://localhost:3000";    // Local development
    //private string websocketUrl = "wss://jamsik.p-e.kr";  // Production (HTTPS)

    async void Start()
    {
        socket = new WebSocket(websocketUrl);

        socket.OnOpen += () => Debug.Log("[UserNetwork] Connected to server");

        socket.OnMessage += (bytes) =>
        {
            HandleMessage(bytes);
        };

        socket.OnClose += (_) => Debug.LogWarning("Connection closed");
        socket.OnError += (e) => Debug.LogError("WebSocket error: " + e);

        await socket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        socket?.DispatchMessageQueue();
#endif
    }

    void LateUpdate()
    {
        if (pendingInitMessages.Count > 0 && UnitManager.Instance != null)
        {
            while (pendingInitMessages.Count > 0)
            {
                string msg = pendingInitMessages.Dequeue();
                UnitManager.Instance.OnReceiveInitMessage(msg);
            }
        }
    }

    public void SetTeam(Team team)
    {
        MyTeam = team;
        IsTeamReady = true;
        Debug.Log($"Team set: {team}");

        var controller = Object.FindFirstObjectByType<TeamUIController>();
        if (controller == null)
        {
            Debug.LogError("TeamUIController not found");
        }
        else
        {
            controller.SetTeam(team);
        }
    }

    void HandleMessage(byte[] bytes)
    {
        string json = Encoding.UTF8.GetString(bytes).Trim();
        var header = JsonUtility.FromJson<MessageTypeHeader>(json);

        switch (header.type)
        {
            case "startCountdown":
                TimerManager.Instance?.ResetUI();
                TimerManager.Instance?.BeginCountdown();
                break;

            case "gameStart":
                string scene = SceneManager.GetActiveScene().name;

                if (scene == "2-GameScene")
                {
                    GameManager2.Instance?.OnAllPlayersReadyFromServer();
                }
                else if (scene == "3-BattleScene")
                {
                    GameManager2.Instance?.ReturnToPlacementScene();
                }
                break;


            case "init":
                if (UnitManager.Instance != null)
                    UnitManager.Instance.OnReceiveInitMessage(json);
                else
                    pendingInitMessages.Enqueue(json);
                break;

            case "teamAssign":
                var teamMsg = JsonUtility.FromJson<TeamAssignMessage>(json);
                Team parsedTeam = (Team)System.Enum.Parse(typeof(Team), teamMsg.team);
                SetTeam(parsedTeam);
                break;

            case "assignId":
                var assign = JsonUtility.FromJson<AssignIdMessage>(json);
                userId = assign.clientId;
                Debug.Log($"Client ID set: {userId}");
                break;
        }
    }

    private bool alreadyReadySent = false;

    public void SendReady()
    {
        if (socket != null && socket.State == WebSocketState.Open && !alreadyReadySent)
        {
            alreadyReadySent = true;

            var readyMsg = new ReadyMessage
            {
                type = "ready",
                ownerId = userId
            };

            string json = JsonUtility.ToJson(readyMsg);
            socket.SendText(json);

            Debug.Log("Ready message sent");
        }
    }

    public void ResetReadyState()
    {
        alreadyReadySent = false;
    }
}

[System.Serializable]
public class TeamAssignMessage
{
    public string type;
    public string ownerId;
    public string team;
}

[System.Serializable]
public class MessageTypeHeader
{
    public string type;
}

[System.Serializable]
public class ReadyMessage
{
    public string type;
    public string ownerId;
}

[System.Serializable]
public class AssignIdMessage
{
    public string type;
    public string clientId;
}
