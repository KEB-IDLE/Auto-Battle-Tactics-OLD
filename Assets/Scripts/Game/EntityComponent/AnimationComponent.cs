using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationComponent : MonoBehaviour
{
    private Animator _animator;
    private AnimatorOverrideController _controller;

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
        if (_controller == null)
            _controller = new AnimatorOverrideController(data.animatorController);

        _animator.runtimeAnimatorController = _controller;

        if (data.attackClip != null)
            _controller["Attack"] = data.attackClip;
        if (data.runClip != null)
            _controller["Run"] = data.runClip;
        if (data.deathClip != null)
            _controller["Die"] = data.deathClip;
        if (data.idleClip != null)
            _controller["Idle"] = data.idleClip;
    }

    public void Bind()
    {
        _attackSrc.OnAttackStateChanged += HandleAttack;
        _moveSrc.OnMove += HandleMove;
        _deathSrc.OnDeath += HandleDeath;
    }

    public void Unbind()
    {
        _attackSrc.OnAttackStateChanged -= HandleAttack;
        _moveSrc.OnMove -= HandleMove;
        _deathSrc.OnDeath -= HandleDeath;
    }

    void HandleAttack(bool isAttacking)
    {
        if (isAttacking)
        {
            Debug.Log("attack Start!");
            _animator.SetBool("Attack", true);
        }
        else
        {
            Debug.Log("attack End!");
            _animator.SetBool("Attack", false);
        }
    }
            

    void HandleMove()
    {
        _animator.SetTrigger("Move");
    }

    void HandleDeath()
    {
        _animator.SetTrigger("Die");
        Unbind();
    }
}
