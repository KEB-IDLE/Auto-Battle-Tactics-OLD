using System;
using UnityEngine;

public interface IAttackNotifier
{
    event Action<bool> OnAttackStateChanged;
}

public interface IMoveNotifier
{
    event Action OnMove;
}

public interface IDeathNotifier
{
    event Action OnDeath;
}