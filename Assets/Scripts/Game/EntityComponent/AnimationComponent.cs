using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationComponent : MonoBehaviour
{
    private Animator _animator;

    IAttackNotifier  _attackSrc;
    IMoveNotifier    _moveSrc;
    IDeathNotifier   _deathSrc;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _attackSrc = GetComponent<IAttackNotifier>();
        _moveSrc = GetComponent<IMoveNotifier>();
        _deathSrc = GetComponent<IDeathNotifier>();
    }

    public void Initialize(EntityData data)
    {
        _animator.runtimeAnimatorController = data.animatorController;
    }

    public void Bind()
    {
        //_animator.SetTrigger("Spawn");
        _attackSrc.OnAttackPerformed += HandleAttack;
        _moveSrc.OnMove += HandleMove;
        _deathSrc.OnDeath += HandleDeath;
    }

    public void Unbind()
    {
        _attackSrc.OnAttackPerformed -= HandleAttack;
        _moveSrc.OnMove -= HandleMove;
        _deathSrc.OnDeath -= HandleDeath;
    }

    void HandleAttack(IDamageable _)
    {
        _animator.SetTrigger("Attack");
    }

    void HandleMove()
    {
        _animator.SetTrigger("Move");
    }

    void HandleDeath()
    {
        _animator.SetTrigger("Die");
    }

    // 이 함수는 Die 애니메이션이 끝날 때 실행되어야 함.
    public void OnDeathAnimationComplete()
    {
        Unbind();
        Destroy(gameObject);
    }

}
