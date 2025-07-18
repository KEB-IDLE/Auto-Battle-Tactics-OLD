using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance { get; private set; }

    //public UserProfile profile;
    //public UserRecord record;
    //public List<UserChampion> champions = new();
    //public List<UserDeck> decks = new();
    //public List<MatchHistory> matchHistory = new();
    public string accessToken;

    private List<Entity> registeredEntities = new List<Entity>();

    public bool BattleStarted { get; private set; } = false;
    

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

    /// <summary>
    /// 유닛 등록
    /// </summary>
    public void Register(Entity entity)
    {
        if (entity == null || entity.gameObject == null) return;

        if (registeredEntities.Any(e => e.UnitId == entity.UnitId)) return;

        registeredEntities.Add(entity);
        Debug.Log($"🧩 [GameManager] 유닛 등록됨: {entity.name}");

        // ✅ 죽을 때 자동 제거
        entity.GetComponent<HealthComponent>().OnDeath += () => Unregister(entity);
    }


    /// <summary>
    /// 유닛 제거
    /// </summary>
    public void Unregister(Entity entity)
    {
        if (registeredEntities.Contains(entity))
            registeredEntities.Remove(entity);
    }

    /// <summary>
    /// UnitId로 유닛 검색
    /// </summary>
    public Entity FindById(string unitId)
    {
        return registeredEntities.Find(e => e.UnitId == unitId);
    }

    /// <summary>
    /// 전투 시작
    /// </summary>
    public void StartBattle()
    {
        if (BattleStarted)
        {
            Debug.LogWarning("이미 전투가 시작되었습니다.");
            return;
        }

        BattleStarted = true;

        foreach (var entity in registeredEntities)
        {
            var anim = entity.GetComponent<AnimationComponent>();
            if (anim != null)
            {
                Debug.Log($"[GameManager] 유닛 {entity.name} → StartBattle 호출");
                anim.PlayAnimation("BattleStart"); // Animator에 존재해야 함
            }
        }

        LockAllUnitsMovement();
        Debug.Log("✅ 전투 시작!");
    }

    /// <summary>
    /// 유닛 모두 제거
    /// </summary>
    public void UnregisterAll()
    {
        foreach (var entity in registeredEntities.ToList())
            Unregister(entity);

        registeredEntities.Clear();
        BattleStarted = false;
    }

    /// <summary>
    /// 모든 유닛 이동 잠금
    /// </summary>
    public void LockAllUnitsMovement()
    {
        foreach (var entity in registeredEntities)
        {
            var mover = entity.GetComponent<UnitManager>();
            if (mover != null)
                mover.enabled = false; // 또는 mover.LockMovement();
        }
    }
}
