using UnityEngine;
using NativeWebSocket;
using UnityEngine.AI;

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
    public string team;
    public string layer;
}

public class UnitNetwork : MonoBehaviour
{
    private WebSocket socket;
    private Entity _entity;
    private bool _isMyUnit = false;
    private bool hasInitSent = false;

    public void InitializeNetwork(bool isMine)
    {
        _isMyUnit = isMine;
        _entity = GetComponent<Entity>();
        socket = UserNetwork.GetSocket();

        if (!_isMyUnit) return;

        // 내 유닛: 애니메이션 + AI 설정
        var anim = GetComponent<AnimationComponent>();
        var move = GetComponent<MoveComponent>();
        var atk = GetComponent<AttackComponent>();

        if (anim != null && move != null)
            move.OnMove += anim.HandleMove;
        if (anim != null && atk != null)
            atk.OnAttackStateChanged += anim.HandleAttack;

        move?.SetIsMine(true);

        Debug.Log($"[UnitNetwork] ✅ 내 유닛 소유권 설정 완료: {_entity.UnitId}");
    }

    public void SendInit()
    {
        if (hasInitSent || !_isMyUnit || socket == null || socket.State != WebSocketState.Open) return;
        hasInitSent = true;

        Vector3 pos = transform.position;
        var msg = new InitMessage
        {
            unitId = _entity.UnitId,
            unitType = _entity.UnitType,
            position = new float[] { pos.x, pos.y, pos.z },
            hp = Mathf.FloorToInt(GetComponent<HealthComponent>()?.CurrentHp ?? _entity.Data.maxHP),
            atk = Mathf.FloorToInt(_entity.Data.attackDamage),
            ownerId = UserNetwork.Instance.MyId,
            team = UserNetwork.Instance.MyTeam.ToString(),
            layer = LayerMask.LayerToName(gameObject.layer)
        };

        string json = JsonUtility.ToJson(msg);
        socket.SendText(json);

        GameManager2.Instance?.SaveInitMessage(msg);
        hasInitSent = true;
        Debug.Log($"\uD83D\uDCE4 [SendInit] {_entity.UnitId} 정보 서버 전송 완료");
    }
}
