using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{

    private float currentHP; // 현재 체력
    private bool isBurning; // 불타고 있는 상태
    private bool isFrozen;  // 얼어있는 상태
    private bool isStunned; // 기절 상태
    private bool isPoisoned; // 중독 상태

    public void Initialize(EntityData data)
    {
        //entityData = data; // EntityData 스크립트 오브젝트 할당
        currentHP = data.maxHP;

        // 상태 변수 초기화
        //isBurning = false;                          // 불타고 있는 상태 초기화
        //isFrozen = false;                           // 얼어있는 상태 초기화
        //isStunned = false;                          // 기절 상태 초기화
        //isPoisoned = false;                         // 중독 상태 초기화
    }

    public bool IsAlive()
    {
        return currentHP > 0; // 현재 체력이 0보다 크면 살아있음
    }

    public void TakeDamage(float damage)
    {
        if (IsAlive())
        {
            currentHP -= damage; // 데미지를 받아 현재 체력 감소
            if (currentHP <= 0)
            {
                Die(); // 체력이 0 이하가 되면 죽음 처리
            }
        }
    }

    public void Die() // 죽었을 때 필요한 공통 로직 작성
    {
        Debug.Log("testagent is dead");
        Destroy(gameObject); // 오브젝트 파괴
        return;
    }

}
