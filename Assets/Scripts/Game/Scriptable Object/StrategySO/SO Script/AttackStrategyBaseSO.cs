using UnityEngine;

public abstract class AttackStrategyBaseSO : ScriptableObject
{
    public virtual void Attack(AttackComponent context, IDamageable target) { }
    public virtual void DrawGizmos(AttackComponent context, EntityData data) { }
    public virtual void ExecuteDelayedAttack(AttackComponent context, IDamageable target) { }
    public virtual float DelayTime => 0f;
}
