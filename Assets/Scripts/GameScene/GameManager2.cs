using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public void LockAllUnitsMovement()
    {
        foreach (var entity in registeredEntities)
        {
            var mover = entity.GetComponent<MoveComponent>();
            if (mover != null)
                mover.enabled = false;
        }
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
                unit.gameObject.SetActive(false);     // ✅ 유닛은 숨기고
                registeredEntities.Remove(unit);      // ✅ 등록 리스트에서 제거
            }
        }

        myUnits.Clear(); // ✅ 필요 시 복귀 후 다시 채움
        Debug.Log("🧹 DeactivateAllMyUnits: 유닛 비활성화 + 등록 해제 완료");
    }
    public void NotifyBattleSceneReady()
    {
        isSceneReady = true;
    }

    public bool IsSceneReady()
    {
        return isSceneReady;
    }
}


