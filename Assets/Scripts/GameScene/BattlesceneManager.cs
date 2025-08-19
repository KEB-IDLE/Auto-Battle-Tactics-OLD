using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System;

public class BattleSceneManager : MonoBehaviour
{
    private bool battleEnded = false;

    private void Awake()
    {
        CoreComponent.OnAnyCoreDestroyed += EndBattleByCoreDeath;
    }

    private IEnumerator Start()
    {
        battleEnded = false;

        // 배틀 시작 신호 대기
        while (!GameManager2.Instance || !GameManager2.Instance.BattleStarted)
            yield return null;

        GameManager2.Instance.ApplySavedCoreHpToCurrentSceneCores();

        var initMessages = GameManager2.Instance.GetInitMessages();
        Debug.Log($"📦 [BattleScene] 복원할 InitMessage 개수: {initMessages.Count}");

        foreach (var msg in initMessages)
        {
            // 1) 데이터/팀/프리팹
            var data = UnitManager.Instance.GetEntityData(msg.unitType);
            if (data == null) { Debug.LogError($"❌ EntityData 없음: {msg.unitType}"); continue; }

            if (!Enum.TryParse(msg.team, out Team teamParsed)) teamParsed = Team.Blue;

            var prefab = UnitManager.Instance.GetTeamPrefab(msg.unitType, teamParsed);
            if (prefab == null) { Debug.LogError($"❌ [복원 실패] 프리팹 없음: {msg.unitType} / {teamParsed}"); continue; }

            // 2) 요청 좌표 -> Vector3
            Vector3 req = (msg.position != null && msg.position.Length >= 3)
                            ? new Vector3(msg.position[0], msg.position[1], msg.position[2])
                            : Vector3.zero;

            // 3) 인스턴스 생성 + NavMesh 보정 후 배치
            var go = Instantiate(prefab);
            var agent = go.GetComponent<NavMeshAgent>();

            Vector3 target = req;
            if (agent)
            {
                var filter = new NavMeshQueryFilter
                {
                    agentTypeID = agent.agentTypeID,
                    areaMask = NavMesh.AllAreas
                };
                if (NavMesh.SamplePosition(req, out var nav, 2.0f, filter))
                    target = nav.position;

                // Warp 성공 시에만 ResetPath 호출
                bool warped = agent.Warp(target);
                if (!warped)
                {
                    Debug.LogWarning("[BattleScene] Warp 실패 → transform 배치로 대체");
                    go.transform.SetPositionAndRotation(target, Quaternion.identity);
                }
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

            // 4) 활성화 후 컴포넌트 초기화/등록
            if (!go.activeSelf) go.SetActive(true);

            var entity = go.GetComponent<Entity>();
            if (entity)
            {
                entity.SetData(data);
                entity.SetUnitId(msg.unitId);
                entity.SetOwnerId(msg.ownerId);
            }
            GameManager2.Instance.RegisterBattleEntity(entity);
            go.GetComponent<TeamComponent>()?.SetTeam(teamParsed);

            if (!string.IsNullOrEmpty(msg.layer))
            {
                int li = LayerMask.NameToLayer(msg.layer);
                if (li >= 0) go.layer = li;
            }

            go.GetComponent<HealthComponent>()?.Initialize(data);
            go.GetComponent<AnimationComponent>()?.Initialize(data);
            go.GetComponent<AttackComponent>()?.Initialize(data);
            go.GetComponent<EffectComponent>()?.Initialize(data);
        }


        yield return new WaitUntil(() => TimerManager.Instance != null && TimerManager.Instance.countdownText != null);
        Debug.Log("⏲ 전투씬에서 타이머 직접 시작함");
        TimerManager.Instance?.ResetUI();
        TimerManager.Instance?.BeginCountdown();
    }


    private void EndBattleByCoreDeath(Team loser)
    {
        if (battleEnded) return;
        battleEnded = true;
        EndBattleAndReturn();
    }

    public void EndBattleByTimeout()
    {
        if (battleEnded) return;
        battleEnded = true;
        EndBattleAndReturn();
    }

    private void EndBattleAndReturn()
    {
        foreach (var e in GameManager2.Instance.GetBattleEntities())
        {
            if (!e) continue;
            if (e.GetComponent<Core>() != null) { e.gameObject.SetActive(false); continue; }
            Destroy(e.gameObject);
        }

        GameManager2.Instance?.SaveAllCoreHp();
        GameManager2.Instance?.ClearBattleEntities();
        UserNetwork.Instance?.ResetReadyState();
        UserNetwork.Instance?.SendReady();
    }

    private void OnDestroy()
    {
        CoreComponent.OnAnyCoreDestroyed -= EndBattleByCoreDeath;
    }
}
