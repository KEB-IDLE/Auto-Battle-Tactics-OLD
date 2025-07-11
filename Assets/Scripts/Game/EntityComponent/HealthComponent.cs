using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{

    private float currentHP; // ���� ü��
    private bool isBurning; // ��Ÿ�� �ִ� ����
    private bool isFrozen;  // ����ִ� ����
    private bool isStunned; // ���� ����
    private bool isPoisoned; // �ߵ� ����

    public void Initialize(EntityData data)
    {
        //entityData = data; // EntityData ��ũ��Ʈ ������Ʈ �Ҵ�
        currentHP = data.maxHP;

        // ���� ���� �ʱ�ȭ
        //isBurning = false;                          // ��Ÿ�� �ִ� ���� �ʱ�ȭ
        //isFrozen = false;                           // ����ִ� ���� �ʱ�ȭ
        //isStunned = false;                          // ���� ���� �ʱ�ȭ
        //isPoisoned = false;                         // �ߵ� ���� �ʱ�ȭ
    }

    public bool IsAlive()
    {
        return currentHP > 0; // ���� ü���� 0���� ũ�� �������
    }

    public void TakeDamage(float damage)
    {
        if (IsAlive())
        {
            currentHP -= damage; // �������� �޾� ���� ü�� ����
            if (currentHP <= 0)
            {
                Die(); // ü���� 0 ���ϰ� �Ǹ� ���� ó��
            }
        }
    }

    public void Die() // �׾��� �� �ʿ��� ���� ���� �ۼ�
    {
        Debug.Log("testagent is dead");
        Destroy(gameObject); // ������Ʈ �ı�
        return;
    }

}
