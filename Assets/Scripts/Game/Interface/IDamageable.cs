using UnityEngine;

public interface IDamageable
{
    bool IsAlive(); // ���� ���� Ȯ��
    void TakeDamage(float damage); // ����� ����
}
