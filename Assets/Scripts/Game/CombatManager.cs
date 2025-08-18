using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public class CombatManager : MonoBehaviour
{
    public static event Action OnGameEnd;
    public static void EndGame()
    {
        MusicManager.Instance.StopMusic();
        OnGameEnd?.Invoke();
    }


    public void Start()
    {
        MusicManager.Instance.PlayBattle();
    }

}
