using UnityEngine;
using NativeWebSocket;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System;

public class WebSocketClient : MonoBehaviour
{
    WebSocket websocket;
    public string playerId;
    public GameObject playerPrefab;
    public float moveSpeed = 5f;

    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    private Dictionary<string, Rigidbody> rigidbodies = new Dictionary<string, Rigidbody>();

    private Vector3 predictedPosition = Vector3.zero;
    private Coroutine sendPositionCoroutine;

    void Start()
    {
        playerId = Guid.NewGuid().ToString();
        Debug.Log("[Start] 생성된 playerId: " + playerId);

        predictedPosition = Vector3.zero;
        GameObject me = Instantiate(playerPrefab, predictedPosition, Quaternion.identity);
        me.name = playerId;
        players[playerId] = me;

        Rigidbody rb = me.GetComponent<Rigidbody>();
        if (rb == null) rb = me.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rigidbodies[playerId] = rb;

        Debug.Log("[Start] 내 플레이어 오브젝트 생성 완료");

        StartCoroutine(ConnectCoroutine());
    }

    IEnumerator ConnectCoroutine()
    {
        websocket = new WebSocket("ws://localhost:8080");

        websocket.OnOpen += () => Debug.Log("✅ WebSocket connected");
        websocket.OnError += (e) => Debug.LogError("❌ WebSocket error: " + e);
        websocket.OnClose += (e) => Debug.LogWarning("⚠️ WebSocket closed");

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            PositionsMessage data = JsonUtility.FromJson<PositionsMessage>(message);

            if (data.type != "positions" || data.players == null) return;

            foreach (var p in data.players)
            {
                if (!players.ContainsKey(p.id))
                {
                    GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                    go.name = p.id;
                    players[p.id] = go;

                    Rigidbody rb = go.GetComponent<Rigidbody>();
                    if (rb == null) rb = go.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rigidbodies[p.id] = rb;

                    Debug.Log($"[WebSocket] 새 플레이어 생성: {p.id}");
                }

                Vector3 newPos = new Vector3(p.x, p.y, p.z);
                rigidbodies[p.id].MovePosition(newPos);

                if (p.id == playerId)
                {
                    predictedPosition = newPos;
                }
            }
        };

        var connectTask = websocket.Connect();
        while (!connectTask.IsCompleted)
            yield return null;

        if (connectTask.IsFaulted)
        {
            Debug.LogError("WebSocket 연결 실패: " + connectTask.Exception);
            yield break;
        }

        sendPositionCoroutine = StartCoroutine(SendPositionLoop());
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif

        if (!players.ContainsKey(playerId))
        {
            Debug.LogWarning("[Update] 내 플레이어가 아직 생성되지 않았습니다.");
            return;
        }

        HandleInputPrediction();
    }

    void HandleInputPrediction()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(h, 0, v);
        if (direction != Vector3.zero)
        {
            predictedPosition += direction * moveSpeed * Time.deltaTime;
        }
    }

    IEnumerator SendPositionLoop()
    {
        while (true)
        {
            SendPredictedPosition();
            yield return new WaitForSeconds(0.1f);
        }
    }

    async void SendPredictedPosition()
    {
        if (websocket.State != WebSocketState.Open)
            return;

        var data = new PositionMessage
        {
            type = "position",
            id = playerId,
            x = predictedPosition.x,
            y = predictedPosition.y,
            z = predictedPosition.z
        };

        string json = JsonUtility.ToJson(data);
        await websocket.SendText(json);
    }

    async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }

    void OnDestroy()
    {
        if (sendPositionCoroutine != null)
            StopCoroutine(sendPositionCoroutine);

        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.Close();
        }

        foreach (var kvp in players)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value);
        }
        players.Clear();
    }

    [Serializable]
    public class PositionMessage
    {
        public string type;
        public string id;
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class PositionsMessage
    {
        public string type;
        public List<PositionMessage> players;
    }
}
