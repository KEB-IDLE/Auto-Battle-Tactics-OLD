using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public class CombatManager : MonoBehaviour
{
    public static event Action OnGameEnd;
    public static event Action OnRoundStart;
    public static event Action OnRoundEnd;
    public static void EndGame()
    {
        MusicManager.Instance.StopMusic();
        OnGameEnd?.Invoke();
        //UIManager.Instance.ShowGameEndPanel();
    }

    public static void StartRound()
    {
        OnRoundStart?.Invoke();
    }

    public static void EndRound()
    {
        OnRoundEnd?.Invoke();
    }

    public void Start()
    {
        MusicManager.Instance.PlayBattle();
    }

}
