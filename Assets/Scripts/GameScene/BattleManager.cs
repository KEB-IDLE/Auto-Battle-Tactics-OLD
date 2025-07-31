using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private List<Entity> allBattleUnits = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 유닛 등록 (중복 방지)
    /// </summary>
    public void RegisterUnit(Entity entity)
    {
        if (!allBattleUnits.Contains(entity))
            allBattleUnits.Add(entity);
    }

    /// <summary>
    /// 특정 유닛 ID가 등록되어 있는지 확인
    /// </summary>
    public bool IsUnitRegistered(string unitId)
    {
        return allBattleUnits.Any(e => e.UnitId == unitId);
    }

    /// <summary>
    /// 특정 팀의 유닛 목록 반환
    /// </summary>
    public List<Entity> GetUnitsByTeam(Team team)
    {
        return allBattleUnits.Where(u => u.Team == team).ToList();
    }

    /// <summary>
    /// 유닛 전체 초기화 (전투 종료 후 등)
    /// </summary>
    public void ClearUnits()
    {
        allBattleUnits.Clear();
    }
}
