using UnityEngine;
using NativeWebSocket;
using System.Text;
using System.Collections.Generic;
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
public class UserNetwork : MonoBehaviour
{
    public static UserNetwork Instance { get; private set; }
    public string MyId { get; private set; } = System.Guid.NewGuid().ToString();
    public Team MyTeam { get; private set; }

    private WebSocket socket;

    public static WebSocket GetSocket() => Instance?.socket;

    private static List<string> connectedIds = new();

    public static IReadOnlyList<string> GetAllConnectedIds() => connectedIds;
    public void SetTeam(Team team)
    {
        MyTeam = team;
        Debug.Log($"✅ 내 팀 설정됨: {team}");
    }

    void Awake()
    {
        Debug.Log("🧪 UserNetwork.Awake() 호출됨");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("⚠️ 중복된 UserNetwork 감지 → 파괴");
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
            Debug.Log($"📩 테스트 메시지 수신: {json}");
            HandleMessage(bytes); // ✅ 꼭 호출!
        };

        socket.OnClose += (_) => Debug.LogWarning("🔌 연결 종료");
        socket.OnError += (e) => Debug.LogError("❌ 오류 발생: " + e);

        await socket.Connect();
    }
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        socket?.DispatchMessageQueue(); // 💡 이걸 반드시 호출해야 메시지 수신됨
#endif
    }


    void HandleMessage(byte[] bytes)
    {
        string json = Encoding.UTF8.GetString(bytes).Trim();
        var header = JsonUtility.FromJson<MessageTypeHeader>(json);

        Debug.Log($"📩 수신된 메시지: {json}");



        switch (header.type)
        {
            case "startCountdown":
                StartUIController.Instance?.BeginCountdown();
                break;
            case "gameStart":
                StartUIController.Instance?.OnAllPlayersReady();
                break;
            case "init":
                UnitManager.Instance?.OnReceiveInitMessage(json);
                break;
            case "stateUpdate":
                UnitManager.Instance?.OnReceiveStateUpdate(json);
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
                ownerId = System.Guid.NewGuid().ToString() // or use a playerId if stored
            };

            string json = JsonUtility.ToJson(readyMsg);
            socket.SendText(json);

            Debug.Log("📤 [UserNetwork] ready 전송 완료");
        }
    }


}
