using UnityEngine;

public interface IDamageable
{
    bool IsAlive(); // ���� ���� Ȯ��
    void TakeDamage(float damage); // ����� ����
    //Transform Transform { get; } // Transform �Ӽ� �߰�
}
