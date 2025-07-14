using System;
using UnityEngine;

public interface IAttackable
{
    bool IsAttacking();
    IDamageable DetectTarget();

    event Action<bool> OnAttackStateChanged;
    Transform LockedTargetTransform { get; }
}
