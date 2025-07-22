using UnityEngine;

public interface IOrientable
{
    //void LookAtTarget(Vector3 position);
    void LookAtTarget(IDamageable target);
}
