using UnityEngine;
using NativeWebSocket;
using System.Text;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
[System.Serializable]

public class UserNetwork : MonoBehaviour
{
    public static UserNetwork Instance { get; private set; }
    private Queue<string> pendingInitMessages = new();
    public string MyId { get; private set; } = System.Guid.NewGuid().ToString();
    public Team MyTeam { get; private set; }
    public bool IsTeamReady { get; private set; } = false;
    public bool IsSocketReady => socket != null && socket.State == WebSocketState.Open;
    private Dictionary<Team, float> savedCoreHp = new();


    private WebSocket socket;
    private static List<string> connectedIds = new();
    public static WebSocket GetSocket() => Instance?.socket;
    public static IReadOnlyList<string> GetAllConnectedIds() => connectedIds;

    public void SetTeam(Team team)
    {
        MyTeam = team;
        IsTeamReady = true;
        Debug.Log($"✅ 내 팀 설정됨: {team}");

        var controller = Object.FindFirstObjectByType<TeamUIController>();
        if (controller == null)
        {
            Debug.LogError("❌ TeamUIController 못 찾음");
        }
        else
        {
            controller.SetTeam(team);
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        socket = new WebSocket("ws://localhost:3000");

        socket.OnOpen += () => Debug.Log("🟢 [UserNetwork] 서버에 연결됨");

        socket.OnMessage += (bytes) =>
        {
            string json = Encoding.UTF8.GetString(bytes);
            HandleMessage(bytes);
        };

        socket.OnClose += (_) => Debug.LogWarning("🔌 연결 종료");
        socket.OnError += (e) => Debug.LogError("❌ 오류 발생: " + e);

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
                {
                    string scene = SceneManager.GetActiveScene().name;

                    if (scene == "3-GameScene2")
                    {
                        GameManager2.Instance?.OnAllPlayersReadyFromServer();
                    }
                    else if (scene == "4-BattleScene")
                    {

                        GameManager2.Instance?.ReturnToPlacementScene();
                    }
                    break;
                }

            case "init":
                if (UnitManager.Instance != null)
                    UnitManager.Instance.OnReceiveInitMessage(json);
                else
                    pendingInitMessages.Enqueue(json); // ✅ 저장해놨다가 나중에 처리
                break;
            case "teamAssign":
                var teamMsg = JsonUtility.FromJson<TeamAssignMessage>(json);
                Team parsedTeam = (Team)System.Enum.Parse(typeof(Team), teamMsg.team);
                SetTeam(parsedTeam);
                break;
            case "assignId":
                var assign = JsonUtility.FromJson<AssignIdMessage>(json);
                MyId = assign.clientId;
                Debug.Log($"🆔 [UserNetwork] 내 클라이언트 ID 설정됨: {MyId}");
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
                ownerId = MyId
            };

            string json = JsonUtility.ToJson(readyMsg);
            socket.SendText(json);

            Debug.Log("📤 [UserNetwork] ready 전송 완료");
        }
    }
    public void ResetReadyState()
    {
        alreadyReadySent = false;
    }
    public void ProcessPendingMessages()
    {
        if (UnitManager.Instance == null) return;

        while (pendingInitMessages.Count > 0)
        {
            string msg = pendingInitMessages.Dequeue();
            UnitManager.Instance.OnReceiveInitMessage(msg);
        }

        Debug.Log("🧹 [UserNetwork] 대기 중인 init 메시지 처리 완료");
    }

    public void SaveCoreHp(Team team, float hp)
    {
        savedCoreHp[team] = hp;
        Debug.Log($"💾 [UserNetwork] 코어 체력 로컬 저장됨: {team} → {hp}");
    }

    public float GetSavedCoreHp(Team team)
    {
        if (savedCoreHp.TryGetValue(team, out float hp))
            return hp;

        return 100f; // 기본값 (원하는 기본 체력으로 조정 가능)
    }
}
