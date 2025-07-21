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
        Debug.Log("[Start] playerId: " + playerId);

        predictedPosition = Vector3.zero;
        GameObject me = Instantiate(playerPrefab, predictedPosition, Quaternion.identity);
        me.name = playerId;
        players[playerId] = me;

        Rigidbody rb = me.GetComponent<Rigidbody>();
        if (rb == null) rb = me.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rigidbodies[playerId] = rb;

        StartCoroutine(ConnectCoroutine());
    }

    IEnumerator ConnectCoroutine()
    {
        Debug.Log("[ConnectCoroutine] 시작");

        websocket = new WebSocket("ws://localhost:8080");

        websocket.OnOpen += () => {
            Debug.Log("✅ WebSocket connected (이벤트)");

            sendPositionCoroutine = StartCoroutine(SendPositionLoop());
            Debug.Log("[OnOpen] sendPositionCoroutine 시작");
        };

        websocket.OnError += (e) => Debug.LogError("❌ WebSocket error: " + e);
        websocket.OnClose += (e) => Debug.LogWarning("⚠️ WebSocket closed");

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("[OnMessage] 수신된 메시지: " + message);

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

                    Debug.Log("[OnMessage] 새 플레이어 생성: " + p.id);
                }

                Vector3 newPos = new Vector3(p.x, p.y, p.z);

                if (rigidbodies.TryGetValue(p.id, out Rigidbody rbToMove))
                {
                    rbToMove.MovePosition(newPos);
                }
                else
                {
                    players[p.id].transform.position = newPos;
                }

                if (p.id == playerId)
                {
                    predictedPosition = newPos;
                }
            }
        };

        var connectTask = websocket.Connect();

        // 연결 완료까지 기다리기
        yield return new WaitUntil(() => connectTask.IsCompleted);

        if (connectTask.IsFaulted)
        {
            Debug.LogError("[ConnectCoroutine] WebSocket 연결 실패: " + connectTask.Exception);
            yield break;
        }

        Debug.Log("[ConnectCoroutine] WebSocket 연결 성공");
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

        Vector3 dir = new Vector3(h, 0, v);

        if (dir != Vector3.zero)
        {
            predictedPosition += dir * moveSpeed * Time.deltaTime;
        }
    }

    IEnumerator SendPositionLoop()
    {
        Debug.Log("[SendPositionLoop] 시작됨");
        
        while (true)
        {
            SendPredictedPosition();
            yield return new WaitForSeconds(0.01f);
        }
    }

    async void SendPredictedPosition()
    {
        if (websocket.State != WebSocketState.Open)
        {
            Debug.LogWarning("[Send] WebSocket이 열려있지 않음");
            return;
        }

        var data = new PositionMessage
        {
            type = "position",
            id = playerId,
            x = predictedPosition.x,
            y = predictedPosition.y,
            z = predictedPosition.z
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("[Send] 위치 보냄: " + json);

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
