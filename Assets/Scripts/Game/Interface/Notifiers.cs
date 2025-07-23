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

public interface IEffectNotifier
{
    event Action<Transform> OnAttackEffect;
    event Action<Transform> OnTakeDamageEffect;
    event Action<Transform> OnDeathEffect;
    event Action<Transform> FlightEffect;
}