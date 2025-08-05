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
public class CoreHpMessage
{
    public string type = "coreHp";
    public string team;
    public float hp;
}
[System.Serializable]
public class CoreHpUpdateMessage
{
    public string type;
    public string team;
    public float hp;
}


public class UserNetwork : MonoBehaviour
{
    public static UserNetwork Instance { get; private set; }
    private Queue<string> pendingInitMessages = new();
    public string MyId { get; private set; } = System.Guid.NewGuid().ToString();
    public Team MyTeam { get; private set; }
    public bool IsTeamReady { get; private set; } = false;
    public bool IsSocketReady => socket != null && socket.State == WebSocketState.Open;


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
                        // 안전 장치: 씬 로딩이 너무 빨라졌을 때 대비
                        TimerManager.Instance?.ResetUI();
                        TimerManager.Instance?.BeginCountdown();
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
            case "coreHpUpdate":
                var hpUpdate = JsonUtility.FromJson<CoreHpUpdateMessage>(json);
                ApplyCoreHp(hpUpdate.team, hpUpdate.hp);
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
    public void SendCoreHp(Team team, float hp)
    {
        if (socket != null && socket.State == WebSocketState.Open)
        {
            var msg = new CoreHpMessage
            {
                team = team.ToString(),
                hp = hp
            };

            string json = JsonUtility.ToJson(msg);
            socket.SendText(json);

            Debug.Log($"📤 [UserNetwork] 코어 체력 전송됨: 팀={team}, 체력={hp}");
        }
        else
        {
            Debug.LogWarning("❗ [UserNetwork] 서버에 연결되지 않아 코어 체력 전송 실패");
        }
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
    private void ApplyCoreHp(string teamName, float hp)
    {
        if (!System.Enum.TryParse(teamName, out Team team))
        {
            Debug.LogError($"❌ 팀 이름 파싱 실패: {teamName}");
            return;
        }

        var cores = Object.FindObjectsByType<Core>(FindObjectsSortMode.None);
        foreach (var core in cores)
        {
            var coreTeam = core.GetComponent<TeamComponent>().Team;
            if (coreTeam == team)
            {
                var health = core.GetComponent<HealthComponent>();
                health.Initialize(hp);
                Debug.Log($"🩺 {team} 코어 체력 적용됨: {hp}");
            }
        }
    }


}
