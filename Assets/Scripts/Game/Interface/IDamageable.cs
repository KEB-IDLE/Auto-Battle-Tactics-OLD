using UnityEngine;

public interface IDamageable
{
    bool IsAlive();
    bool IsTargetable();
    void RequestDamage(float damage);
}
