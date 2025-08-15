// ✅ StartBattleScene.cs
using UnityEngine;
using System;
using System.Collections;

public class StartBattleScene : MonoBehaviour
{
    IEnumerator Start()
    {
        Debug.Log("🟢 [BattleSceneManager] 전투 씬 시작됨 → 유닛 복원 시도");

        // GameManager2 대기
        while (GameManager2.Instance == null)
            yield return null;

        // InitMessage 수신 대기 (최대 3초)
        float timeout = 3f;
        while (GameManager2.Instance.GetInitMessages().Count == 0 && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        var initMessages = GameManager2.Instance.GetInitMessages();
        Debug.Log($"📦 [BattleScene] 복원할 InitMessage 개수: {initMessages.Count}");

        foreach (var msg in initMessages)
        {
            // 요청 좌표
            Vector3 position = new Vector3(msg.position[0], msg.position[1], msg.position[2]);

            // 데이터/팀/프리팹
            var data = UnitManager.Instance.GetEntityData(msg.unitType);
            if (data == null) { Debug.LogError($"❌ EntityData 없음: {msg.unitType}"); continue; }

            if (!System.Enum.TryParse(msg.team, out Team parsedTeam)) parsedTeam = Team.Blue;
            var prefab = UnitManager.Instance.GetTeamPrefab(msg.unitType, parsedTeam);
            if (prefab == null) { Debug.LogError($"❌ 프리팹 없음: {msg.unitType} / {parsedTeam}"); continue; }

            // 인스턴스 생성 + NavMesh 보정 후 배치
            GameObject go = Instantiate(prefab);
            var agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();

            Vector3 target = position;
            if (agent)
            {
                var filter = new UnityEngine.AI.NavMeshQueryFilter
                {
                    agentTypeID = agent.agentTypeID,
                    areaMask = UnityEngine.AI.NavMesh.AllAreas
                };
                if (UnityEngine.AI.NavMesh.SamplePosition(position, out var nav, 2.0f, filter))
                    target = nav.position;

                bool warped = agent.Warp(target);
                if (!warped) go.transform.SetPositionAndRotation(target, Quaternion.identity);
                else if (agent.isOnNavMesh)
                {
                    agent.ResetPath();
                    agent.isStopped = false;
                }
            }
            else
            {
                go.transform.SetPositionAndRotation(target, Quaternion.identity);
            }

            // 활성화 후 세팅/등록
            if (!go.activeSelf) go.SetActive(true);

            var entity = go.GetComponent<Entity>();
            if (entity)
            {
                entity.SetData(data);
                entity.SetUnitId(msg.unitId);
                entity.SetOwnerId(msg.ownerId);
            }
            GameManager2.Instance.RegisterBattleEntity(entity);
            go.GetComponent<TeamComponent>()?.SetTeam(parsedTeam);

            int parsedLayer = LayerMask.NameToLayer(msg.layer);
            if (parsedLayer != -1) go.layer = parsedLayer;

            // 필요하면 초기화 호출
            go.GetComponent<HealthComponent>()?.Initialize(data);
            go.GetComponent<AnimationComponent>()?.Initialize(data);
            go.GetComponent<AttackComponent>()?.Initialize(data);
            go.GetComponent<EffectComponent>()?.Initialize(data);
        }
        Debug.Log("🚩 [BattleSceneManager] 복원 완료 신호 전송됨");
    }
}
