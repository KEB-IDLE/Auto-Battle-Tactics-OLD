using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance { get; private set; }

    public bool BattleStarted { get; private set; } = false;
    public bool IsPlacementPhase { get; private set; } = true;
    private List<InitMessage> myInitMessages = new();
    private List<Entity> registeredEntities = new();
    private List<Entity> myUnits = new();
    private List<Entity> battleEntities = new List<Entity>();
    private bool isSceneReady = false;
    private List<InitMessage> allInitMessages = new();
    private List<InitMessage> latestPlacementInitMessages = new(); // 전투 전 저장된 배치 유닛 상태

    public int CurrentRound { get; private set; } = 0;
    public int currentGold = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Register(Entity entity)
    {
        if (entity == null || entity.gameObject == null) return;

        if (registeredEntities.Any(e => e.UnitId == entity.UnitId))
        {
            Debug.LogWarning($"🚫 중복 유닛 ID: {entity.UnitId}");
            return;
        }

        registeredEntities.Add(entity);

        if (IsMyUnit(entity))
        {
            myUnits.Add(entity);
        }

        entity.GetComponent<HealthComponent>().OnDeath += () => Unregister(entity);
    }
    public void RegisterBattleEntity(Entity entity)
    {
        if (entity == null || entity.gameObject == null) return;

        if (battleEntities.Any(e => e.UnitId == entity.UnitId))
        {
            Debug.LogWarning($"⚠️ 유닛({entity.UnitId})은 이미 전투 유닛으로 등록됨 → 생략");
            return;
        }

        battleEntities.Add(entity);
        Debug.Log($"✅ 전투 유닛 등록 완료: {entity.UnitId}");
    }


    public void Unregister(Entity entity)
    {
        registeredEntities.Remove(entity);
        myUnits.Remove(entity);
    }

    public void StartBattle()
    {
        if (BattleStarted)
        {
            Debug.LogWarning("이미 전투가 시작되었습니다.");
            return;
        }

        BattleStarted = true;
        IsPlacementPhase = false;

        Debug.Log("✅ 전투 시작!");
    }

    public void SaveInitMessage(InitMessage msg)
    {
        myInitMessages.Add(msg);
        Debug.Log($"🧾 [GameManager2] InitMessage 저장됨: {msg.unitType} at ({msg.position[0]}, {msg.position[1]}, {msg.position[2]})");
    }
    public void LockAllUnits()
    {
        foreach (var entity in registeredEntities.ToList())
        {
            if (entity == null || entity.gameObject == null) continue;

            // 이동 정지
            var move = entity.GetComponent<MoveComponent>();
            if (move != null) move.enabled = false;

            // 공격 정지
            var atk = entity.GetComponent<AttackComponent>();
            if (atk != null) atk.StopAllAction();  // isGameEnded 등 내부도 정리

            // 애니메이션 정지 (즉시 멈춤)
            var animator = entity.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }

        Debug.Log("🛑 모든 유닛 행동 정지 완료");
    }



    public void SendInitMessages()
    {
        foreach (var unit in myUnits.ToList())
        {
            if (unit == null || unit.gameObject == null)
            {
                Debug.LogWarning("❗ Destroy된 유닛을 건너뜀");
                continue;
            }

            unit.GetComponent<UnitNetwork>()?.SendInit();
        }
    }

    public bool IsMyUnit(Entity entity)
    {
        return entity.OwnerId == UserNetwork.Instance.MyId;
    }
    public void AddInitMessage(InitMessage msg)
    {
        if (allInitMessages.Any(m => m.unitId == msg.unitId))
        {
            Debug.LogWarning($"⚠️ [AddInitMessage] 중복 무시: {msg.unitId}");
            return;
        }

        allInitMessages.Add(msg);
    }

    public List<InitMessage> GetInitMessages()
    {
        return new List<InitMessage>(allInitMessages); // ✅ 전체 유닛 반환
    }
    public void ClearInitMessages()
    {
        myInitMessages.Clear();
    }
    public bool IsUnitRegistered(string unitId)
    {
        return registeredEntities.Any(e => e.UnitId == unitId);
    }

    public void DeactivateAllMyUnits()
    {
        foreach (var unit in myUnits.ToList())
        {
            if (unit != null)
            {
                unit.gameObject.SetActive(false);
                DontDestroyOnLoad(unit.gameObject);
            }
        }

        Debug.Log("🧹 DeactivateAllMyUnits: 유닛 비활성화 완료");
    }
    public void OnAllPlayersReadyFromServer()
    {
        Debug.Log("💥 [GameManager2] 모든 플레이어 준비됨 → 전투 씬으로 전환");
        StartCoroutine(GoToBattleScene());
    }

    public IEnumerator GoToBattleScene()
    {
        currentGold = GoldManager.Instance?.GetCurrentGold() ?? 0;

        SendInitMessages();

        yield return new WaitForSeconds(1f); // 최소 1초 이상 기다리기

        DeactivateAllMyUnits();

        SceneManager.LoadScene("4-BattleScene");

        yield return null;

        StartBattle();
    }

    public void NotifyBattleSceneReady()
    {
        isSceneReady = true;
    }
    public void ReturnToPlacementScene()
    {
        Debug.Log("🏁 전투 종료 → 배치 씬으로 돌아감");

        IsPlacementPhase = true;
        BattleStarted = false;
        ClearInitMessages();
        CurrentRound++;

        SceneManager.LoadScene("3-GameScene2");

        // ✅ 씬 로드 후 복원하는 코루틴 시작
        StartCoroutine(RestoreScene());
    }
    private IEnumerator RestoreScene()
    {
        yield return null; // 씬 로딩 대기

        while (UnitManager.Instance == null)
            yield return null;

        foreach (var unit in myUnits)
        {
            if (unit != null)
            {
                unit.gameObject.SetActive(true);
                Register(unit);
                Debug.Log($"♻️ 유닛 복원됨: {unit.UnitId}");
            }
        }

        // 🔁 코어 체력 복원
        // 🔁 코어 체력 복원 및 체력바 업데이트
        var cores = Object.FindObjectsByType<Core>(FindObjectsSortMode.None);
        foreach (var core in cores)
        {
            var team = core.GetComponent<TeamComponent>().Team;
            var hpComponent = core.GetComponent<HealthComponent>();

            // ✅ objectData에서 maxHP 가져오기
            var coreData = core.GetComponent<Core>()?.GetObjectData();
            if (coreData == null)
            {
                Debug.LogError($"❌ Core의 ObjectData가 비어 있음: {team}");
                continue;
            }

            float maxHP = coreData.maxHP;
            float restoredHp = UserNetwork.Instance.GetSavedCoreHp(team);

            // ✅ 체력 컴포넌트 초기화 + 복원
            hpComponent.Initialize(maxHP);     // maxHP 설정
            hpComponent.RestoreHP(restoredHp); // currentHP 복원

            // ✅ 체력바 연결
            var healthBar = core.GetComponentInChildren<HealthBar>();
            if (healthBar != null)
            {
                healthBar.Initialize(hpComponent);
                healthBar.UpdateBar(hpComponent.CurrentHp, hpComponent.MaxHp);
                Debug.Log($"🖼️ [UI] {team} 코어 체력바 갱신 완료 (배치 씬)");
            }

            Debug.Log($"🩺 {team} 코어 체력 복원됨: {restoredHp}/{maxHP}");
        }


        int updatedGold = currentGold + 50;
        GoldManager.Instance?.SetGold(updatedGold);
        Debug.Log($"💰 배치 골드 복원: {currentGold} + 50 → {updatedGold}");

        TimerManager.Instance?.ResetUI(); // UI 초기화
        TimerManager.Instance?.BeginCountdown(); // 🔥 수동 시작

        var teamController = Object.FindFirstObjectByType<TeamUIController>();
        if (teamController != null)
        {
            teamController.SetTeam(UserNetwork.Instance.MyTeam);
            Debug.Log($"🎯 팀 UI 재설정 완료: {UserNetwork.Instance.MyTeam}");
        }
        else
        {
            Debug.LogWarning("⚠️ TeamUIController 찾지 못함");
        }
    }

    public List<Entity> GetBattleEntities()
    {
        return new List<Entity>(battleEntities);
    }

    public bool IsSceneReady()
    {
        return isSceneReady;
    }
}


