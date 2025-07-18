using UnityEngine;
using NativeWebSocket;
using System.Collections;
[System.Serializable]
public class UnitState
{
    public string unitId;
    public string unitType;
    public float[] position;
    public string animation;
    public int hp;
}
[System.Serializable]
public class InitMessage
{
    public string type = "init";
    public string unitId;
    public string unitType;
    public float[] position;
    public int hp;
    public int atk;
    public string ownerId;
}


[System.Serializable]
public class StateUpdateMessage
{
    public string type = "stateUpdate";
    public UnitState[] units;
}

public class UnitNetwork : MonoBehaviour
{
    private WebSocket socket;
    private Entity entity;

    void Start()
    {
        entity = GetComponent<Entity>();
        socket = UserNetwork.GetSocket();

        if (socket != null && socket.State == WebSocketState.Open)
        {
            SendInit();
            StartCoroutine(SendStateLoop());
        }
    }

    public void SendInit()
    {

        Vector3 pos = transform.position;
        var msg = new InitMessage
        {
            unitId = entity.UnitId,
            unitType = entity.UnitType,
            position = new float[] { pos.x, pos.y, pos.z },
            hp = Mathf.FloorToInt(GetComponent<HealthComponent>()?.CurrentHp ?? entity.Data.maxHP),
            atk = Mathf.FloorToInt(entity.Data.attackDamage),
            ownerId = UserNetwork.Instance.MyId
        };

        string json = JsonUtility.ToJson(msg);
        Debug.Log($"[SendInit] ▶ unitId={entity.UnitId}, layer={gameObject.layer}, unitType={entity.UnitType}");
        socket.SendText(json);
    }

    IEnumerator SendStateLoop()
    {
        while (true)
        {
            if (socket != null && socket.State == WebSocketState.Open)
            {
                Vector3 pos = transform.position;
                string anim = GetComponent<AnimationComponent>()?.GetCurrentAnimation();
                float hp = GetComponent<HealthComponent>()?.CurrentHp ?? entity.Data.maxHP;

                var msg = new StateUpdateMessage
                {
                    type = "stateUpdate",
                    units = new UnitState[]
                    {
                        new UnitState
                        {
                            unitId = entity.UnitId,
                            unitType = entity.UnitType,
                            position = new float[] { pos.x, pos.y, pos.z },
                            animation = anim,
                            hp = Mathf.FloorToInt(hp)
                        }
                    }
                };

                string json = JsonUtility.ToJson(msg);
                socket.SendText(json);
            }
            yield return new WaitForSeconds(2f);
        }
    }

    public void ApplyRemoteState(UnitState state)
    {
        if (state.position != null && state.position.Length == 3)
        {
            Vector3 pos = new Vector3(state.position[0], state.position[1], state.position[2]);
            GetComponent<PositionComponent>()?.SyncPosition(pos);
        }

        if (!string.IsNullOrEmpty(state.animation))
        {
            GetComponent<AnimationComponent>()?.PlayAnimation(state.animation);
        }
    }
}