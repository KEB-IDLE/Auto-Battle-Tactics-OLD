using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    [SerializeField] private List<EntityData> allEntityData;
    private Dictionary<string, EntityData> entityDataMap = new();

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
            if (!entityDataMap.ContainsKey(data.unitType))
            {
                entityDataMap[data.unitType] = data;
            }
        }
    }

    public EntityData GetEntityData(string unitType)
    {
        if (!entityDataMap.TryGetValue(unitType, out var data))
        {
            Debug.LogError($"❌ 유닛 타입 [{unitType}] 에 대한 EntityData가 없습니다.");
        }
        return data;
    }
    public GameObject GetTeamPrefab(string unitType, Team team)
    {
        var data = GetEntityData(unitType);
        if (data == null)
        {
            Debug.LogError($"❌ [GetTeamPrefab] EntityData 없음: {unitType}");
            return null;
        }

        return team == Team.Red ? data.redPrefab : data.bluePrefab;
    }


    public void SpawnUnits(string unitType, Vector3 position, string ownerId)
    {
        Debug.Log($"🟡 [SpawnUnits] 호출됨: {unitType}");
        var data = GetEntityData(unitType);
        if (data == null)
        {
            Debug.LogError($"❌ [SpawnUnits] EntityData 없음. unitType: {unitType}");
            return;
        }
        // 팀 결정
        Team team = (ownerId == UserNetwork.Instance.MyId)
            ? UserNetwork.Instance.MyTeam
            : (UserNetwork.Instance.MyTeam == Team.Red ? Team.Blue : Team.Red);

        // 팀에 따른 프리팹 선택
        GameObject prefab = GetTeamPrefab(unitType, team);
        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        var entity = go.GetComponent<Entity>();

        entity.SetData(data);

        string generatedId = System.Guid.NewGuid().ToString();
        entity.SetUnitId(generatedId);
        entity.SetOwnerId(ownerId);
        GameManager2.Instance.Register(entity);

        // 네트워크 초기화
        bool isMine = (ownerId == UserNetwork.Instance.MyId);
        bool isPlacement = GameManager2.Instance.IsPlacementPhase;
        go.GetComponent<UnitNetwork>()?.InitializeNetwork(isMine);

        // 팀 설정 (내 유닛만 직접 설정)
        if (isMine)
        {
            var teamComponent = go.GetComponent<TeamComponent>();
            if (teamComponent != null)
            {
                teamComponent.SetTeam(UserNetwork.Instance.MyTeam);
                Debug.Log($"✅ 내 유닛 팀 설정됨: {UserNetwork.Instance.MyTeam}");
            }
        }

        // 배치/전투에 따라 컴포넌트 활성화 분기
        var move = go.GetComponent<MoveComponent>();
        if (move != null) move.enabled = !isPlacement;

        var atk = go.GetComponent<AttackComponent>();
        if (atk != null) atk.enabled = !isPlacement;

        var core = go.GetComponent<CoreComponent>();
        if (core != null) core.enabled = !isPlacement;

        // 배치 중에는 내 유닛만 드래그 가능
        if (isMine && isPlacement)
        {
            if (!go.TryGetComponent<DraggableUnit>(out _))
            {
                go.AddComponent<DraggableUnit>();
            }
        }
    }


    public void OnReceiveInitMessage(string message)
    {
        var initData = JsonUtility.FromJson<InitMessage>(message);

        Debug.Log($"📨 [Init 수신] 유닛ID: {initData.unitId} | 타입: {initData.unitType} | 팀: {initData.team} | 소유자: {initData.ownerId}");

        // ✅ 전투 씬 진입 후 일괄 복원을 위해 무조건 메시지 저장
        GameManager2.Instance.AddInitMessage(initData);

        // ✅ BattleScene 씬일 때만 즉시 복원
        if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Battle"))
            return;

        // 내 유닛은 복원하지 않음 (씬에서 이미 복원됨)
        if (initData.ownerId == UserNetwork.Instance.MyId)
        {
            Debug.Log($"⚠️ [무시됨] 내 유닛 메시지임 → {initData.unitId}");
            return;
        }

        Debug.Log($"🟥 [적 유닛 복원 시작] 유닛ID: {initData.unitId}");

        string unitType = initData.unitType;
        Vector3 position = new Vector3(initData.position[0], initData.position[1], initData.position[2]);

        if (!Enum.TryParse(initData.team, out Team parsedTeam))
        {
            Debug.LogError($"❌ [Init] 팀 파싱 실패: {initData.team}");
            return;
        }
        GameObject prefab = GetTeamPrefab(unitType, parsedTeam);
        if (prefab == null)
        {
            Debug.LogError($"❌ [Init] {parsedTeam} 팀 프리팹 없음: {unitType}");
            return;
        }

        var data = GetEntityData(unitType);
        GameObject go = Instantiate(prefab, position, Quaternion.identity);

        int parsedLayer = LayerMask.NameToLayer(initData.layer);
        if (parsedLayer != -1)
            go.layer = parsedLayer;
        else
            Debug.LogError($"❗ 존재하지 않는 레이어 이름: {initData.layer}");

        var entity = go.GetComponent<Entity>();
        entity.SetUnitId(initData.unitId);
        entity.SetOwnerId(initData.ownerId);
        GameManager2.Instance.Register(entity);

        go.GetComponent<TeamComponent>()?.SetTeam(parsedTeam);

        var health = go.GetComponent<HealthComponent>();
        if (health != null)
        {
            if (initData.hp > 0)
                health.Initialize(initData.hp);
            else
                health.Initialize(entity.Data);
        }

        go.GetComponent<AnimationComponent>()?.Initialize(data);
        go.GetComponent<AttackComponent>()?.Initialize(entity.Data);
        go.GetComponent<UnitNetwork>()?.InitializeNetwork(false);

        Debug.Log($"✅ [적 유닛 복원 완료] {unitType} ({initData.unitId}) 위치: {position}");
    }
    public int GetAgentTypeId(string unitType)
    {
        var d = GetEntityData(unitType);
        if (d == null) return -1;

        // 1) 프리팹에 있는 NavMeshAgent에서 직접 가져오기(가장 정확)
        GameObject prefab = d.bluePrefab != null ? d.bluePrefab : d.redPrefab;
        if (prefab != null)
        {
            var agent = prefab.GetComponent<NavMeshAgent>();
            if (agent != null) return agent.agentTypeID;
        }

        // 2) (프로젝트에 있을 경우) scale 이름으로 AgentType 검색
        //    EntityData에 entityScale( Small / Medium / Large ) 같은 필드가 있다면 사용
        try
        {
            string wanted = null;
            // 예: EntityData에 entityScale 이 enum으로 있다면 이렇게 매칭
            // wanted = d.entityScale.ToString(); // "Small"/"Medium"/"Large"

            if (!string.IsNullOrEmpty(wanted))
            {
                int count = NavMesh.GetSettingsCount();
                for (int i = 0; i < count; i++)
                {
                    var s = NavMesh.GetSettingsByIndex(i);
                    string name = NavMesh.GetSettingsNameFromID(s.agentTypeID);
                    if (string.Equals(name, wanted, StringComparison.OrdinalIgnoreCase))
                        return s.agentTypeID;
                }
            }
        }
        catch { /* entityScale 없으면 그냥 넘어감 */ }

        // 3) 마지막 보루: 첫 설정이나 -1 반환
        if (NavMesh.GetSettingsCount() > 0)
            return NavMesh.GetSettingsByIndex(0).agentTypeID;

        Debug.LogWarning($"[UnitManager] GetAgentTypeId 실패: {unitType} → -1 반환");
        return -1;
    }
}
