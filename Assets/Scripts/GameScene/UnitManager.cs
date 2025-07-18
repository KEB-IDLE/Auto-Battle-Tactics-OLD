// ✅ UnitManager.cs
/*
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    [SerializeField] private List<EntityData> allEntityData;
    private Dictionary<int, EntityData> entityDataByLayer = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var data in allEntityData)
        {
            if (data != null && !entityDataByLayer.ContainsKey(data.Layer))
                entityDataByLayer[data.Layer] = data;
        }
    }

    public EntityData GetEntityData(int layer)
    {
        entityDataByLayer.TryGetValue(layer, out var data);
        return data;
    }

    public void SpawnUnits(int layer, Vector3 position)
    {
        var data = GetEntityData(layer);
        if (data == null)
        {
            Debug.LogWarning($"❌ 유닛 레이어 {layer}에 해당하는 프리팹이 없습니다.");
            return;
        }

        GameObject go = GameObject.Instantiate(data.entityPrefab, position, Quaternion.identity);
        go.layer = layer;

        var teamComponent = go.GetComponent<TeamComponent>();
        if (data.Layer == 7) // Human
            teamComponent.SetTeam(Team.Blue);
        else if (data.Layer == 8) // Mutant
            teamComponent.SetTeam(Team.Red);
        else
        {
            Debug.LogWarning($"❗ 예상치 못한 레이어: {go.layer}, 기본값 Red로 설정함");
            teamComponent.SetTeam(Team.Red);
        }

        Debug.Log($"[Spawned] layer={data.Layer} 팀 = {teamComponent.Team}");


        var entity = go.GetComponent<Entity>();
        entity.SetUnitId(System.Guid.NewGuid().ToString());

        var health = go.GetComponent<HealthComponent>();
        health?.Initialize(entity.Data);

        GameManager2.Instance.Register(entity);

        // ✅ 유닛 스폰 후 상태를 서버에 전송하도록 직접 요청
        go.GetComponent<UnitNetwork>()?.SendInit();
    }

    public void OnReceiveInitMessage(string json)
    {

        var msg = JsonUtility.FromJson<InitMessage>(json);
        if (GameManager2.Instance.FindById(msg.unitId) != null) return;
        bool isMyUnit = msg.ownerId == UserNetwork.Instance.MyId;
        var team = isMyUnit ? Team.Blue : Team.Red;


        var pos = new Vector3(msg.position[0], msg.position[1], msg.position[2]);
        int layer;

        switch (msg.unitType)
        {
            case "Human":
                layer = 7;
                break;
            case "Mutant":
                layer = 8;
                break;
            default:
                Debug.LogWarning($"❌ 알 수 없는 unitType 수신: {msg.unitType}");
                return;
        }

        var data = GetEntityData(layer);
        if (data == null) return;

        var obj = Instantiate(data.entityPrefab, pos, Quaternion.identity);
        obj.layer = layer;

        var entity = obj.GetComponent<Entity>();
        entity.SetUnitId(msg.unitId);

        var teamComponent = obj.GetComponent<TeamComponent>();
        if (teamComponent != null)
        {
            teamComponent.Team = team;
        }

        var health = obj.GetComponent<HealthComponent>();
        health?.Initialize(entity.Data);
        health?.Initialize(msg.hp);

        GameManager2.Instance.Register(entity);
    }

    public void OnReceiveStateUpdate(string json)
    {
        var update = JsonUtility.FromJson<StateUpdateMessage>(json);
        foreach (var state in update.units)
        {
            var entity = GameManager2.Instance.FindById(state.unitId);
            if (entity != null)
                entity.GetComponent<UnitNetwork>()?.ApplyRemoteState(state);
        }
    }
} // 메시지 클래스들은 UnitNetwork.cs에만 존재하도록 유지

*/