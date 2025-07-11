using UnityEngine;

public interface IDamageable
{
    bool IsAlive(); // 생존 여부 확인
    void TakeDamage(float damage); // 대미지 적용
    //Transform Transform { get; } // Transform 속성 추가
}
