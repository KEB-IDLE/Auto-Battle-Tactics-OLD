using UnityEngine;
using System;
using System.Collections;

public class BattleSceneManager : MonoBehaviour
{
    private bool battleEnded = false;

    private void Awake()
    {
        CoreComponent.OnAnyCoreDestroyed += EndBattleByCoreDeath;
    }

    IEnumerator Start()
    {
        battleEnded = false;
        Debug.Log("🟢 [BattleSceneManager] 전투 씬 시작됨 → 유닛 복원 시도");

        while (GameManager2.Instance == null)
            yield return null;

        float timeout = 5f;
        while (timeout > 0f)
        {
            var msgs = GameManager2.Instance.GetInitMessages();
            if (msgs.Count > 0)
                break;

            timeout -= Time.deltaTime;
            yield return null;
        }

        var initMessages = GameManager2.Instance.GetInitMessages();

        Debug.Log($"📦 [BattleScene] 복원할 InitMessage 개수: {initMessages.Count}");

        foreach (var msg in initMessages)
        {
            Vector3 position = new Vector3(msg.position[0], msg.position[1], msg.position[2]);
            var data = UnitManager.Instance.GetEntityData(msg.unitType);

            if (!Enum.TryParse(msg.team, out Team parsedTeam))
            {
                Debug.LogError($"❌ [복원 실패] 팀 파싱 오류: {msg.team}");
                continue;
            }

            var prefab = UnitManager.Instance.GetTeamPrefab(msg.unitType, parsedTeam);
            if (data == null || prefab == null)
            {
                Debug.LogError($"❌ [복원 실패] 프리팹 없음: {msg.unitType}");
                continue;
            }

            var go = Instantiate(prefab);
            go.SetActive(false);

            // NavMesh 스냅
            Vector3 spawnPos = position;
            if (UnityEngine.AI.NavMesh.SamplePosition(position, out var navHit, 2.5f, UnityEngine.AI.NavMesh.AllAreas))
                spawnPos = navHit.position;
            go.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

            // ⚠️ 데이터 먼저 주입
            var entity = go.GetComponent<Entity>();
            entity.SetData(data);

            entity.SetUnitId(msg.unitId);
            entity.SetOwnerId(msg.ownerId);
            GameManager2.Instance.RegisterBattleEntity(entity);
            go.GetComponent<TeamComponent>()?.SetTeam(parsedTeam);
            int parsedLayer = LayerMask.NameToLayer(msg.layer);
            if (parsedLayer != -1) go.layer = parsedLayer;

            go.SetActive(true);

            go.GetComponent<HealthComponent>()?.Initialize(data);
            go.GetComponent<AnimationComponent>()?.Initialize(data);
            go.GetComponent<AttackComponent>()?.Initialize(data);
            go.GetComponent<EffectComponent>()?.Initialize(data);
            go.GetComponent<UnitNetwork>()?.InitializeNetwork(msg.ownerId == UserNetwork.Instance.MyId);

            Debug.Log($"✅ 복원 완료: {msg.unitType} ({msg.unitId})");
        }

        Debug.Log("🚩 [BattleSceneManager] 복원 완료 신호 전송됨");
        GameManager2.Instance?.NotifyBattleSceneReady();

        GameManager2.Instance?.RestoreAllCoreHp();

        yield return new WaitUntil(() => TimerManager.Instance != null && TimerManager.Instance.countdownText != null);
        Debug.Log("⏲ 전투씬에서 타이머 직접 시작함");
        TimerManager.Instance?.ResetUI();
        TimerManager.Instance?.BeginCountdown();
    }

    public void EndBattleByTimeout()
    {
        if (battleEnded) return;
        battleEnded = true;

        Debug.Log("⏰ 전투 시간 종료 → EndBattleAndReturn()");
        EndBattleAndReturn();
    }

    private void EndBattleByCoreDeath(Team loser)
    {
        if (battleEnded) return;
        battleEnded = true;

        Debug.Log($"💀 {loser} 코어 파괴됨 → EndBattleAndReturn()");
        EndBattleAndReturn();
    }

    private void EndBattleAndReturn()
    {
        foreach (var entity in GameManager2.Instance.GetBattleEntities())
        {
            if (entity == null) continue;

            // 코어는 파괴하지 않고 비활성화만
            if (entity.GetComponent<Core>() != null)
            {
                entity.gameObject.SetActive(false); // ✅ 코어 숨기기만 함
                continue;
            }

            // 유닛은 제거
            Destroy(entity.gameObject);
        }

        GameManager2.Instance?.SaveAllCoreHp();

        // ✅ 다음 라운드를 위해 전투 유닛 리스트 비우기
        GameManager2.Instance?.ClearBattleEntities();
        // 준비 완료 전송
        UserNetwork.Instance?.ResetReadyState();
        UserNetwork.Instance?.SendReady();
    }

    private void OnDestroy()
    {
        CoreComponent.OnAnyCoreDestroyed -= EndBattleByCoreDeath;
    }
}
