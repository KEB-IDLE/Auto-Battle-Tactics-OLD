using UnityEngine;

public abstract class AttackStrategyBaseSO : ScriptableObject
{
    public abstract void Attack(AttackComponent context, IDamageable target);
}
