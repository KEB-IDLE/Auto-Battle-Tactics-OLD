using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    public List<HealthComponent> allUnits = new();

    void Awake()
    {
        Instance = this;
    }
    public void Register(HealthComponent hc)
    {
        if (!allUnits.Contains(hc))
            allUnits.Add(hc);
    }
    public void Unregister(HealthComponent hc)
    {
        allUnits.Remove(hc);
    }

    void LateUpdate()
    { 
        foreach (var health in allUnits)
        {
            health.ApplyPendingDamage();
        }
    }
}
