/*
using System;
using UnityEngine;

public class TeamComponent : MonoBehaviour, ITeamProvider
{
    [SerializeField]private Team _team;
    public Team Team => _team;

    public void SetTeam(Team team)
    {
        if (_team == team) return;
        _team = team;
    }
}
public enum Team
{
    Red,
    Blue,
    Object
}
*/


using System;
using UnityEngine;

public class TeamComponent : MonoBehaviour, ITeamProvider
{
    [SerializeField] private Team _team;
    public Team Team { get; set; }

    public void SetTeam(Team team)
    {
        if (_team == team) return;
        _team = team;
    }
}
public enum Team
{
    Red,
    Blue,
    Object
}