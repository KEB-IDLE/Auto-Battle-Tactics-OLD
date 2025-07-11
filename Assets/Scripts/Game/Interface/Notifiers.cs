using System;
using UnityEngine;

public interface IAttackNotifier
{
    event Action<IDamageable> OnAttackPerformed;
}

public interface IMovementNotifier
{
    event Action OnMoveStart;
    event Action OnMoveEnd;
}

public interface IDeathNotifier
{
    event Action OnDeath;
}