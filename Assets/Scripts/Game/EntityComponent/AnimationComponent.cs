using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent (typeof(AudioSource))]
public class AnimationComponent : MonoBehaviour
{
    private Animator _animator;
    private AnimatorOverrideController _controller;
    private AudioSource _audioSource;
    private AudioClip summonSound;
    private AudioClip attackSound;
    private AudioClip deathSound;

    IAttackNotifier  _attackSrc;
    IMoveNotifier    _moveSrc;
    IDeathNotifier   _deathSrc;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _attackSrc = GetComponent<IAttackNotifier>();
        _moveSrc = GetComponent<IMoveNotifier>();
        _deathSrc = GetComponent<IDeathNotifier>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(EntityData data)
    {
        if (_controller == null)
            _controller = new AnimatorOverrideController(data.animatorController);
        _animator.runtimeAnimatorController = _controller;

        try
        {
            if (data.attackClip != null)
                _controller["Attack"] = data.attackClip;
            if (data.runClip != null)
                _controller["Run"] = data.runClip;
            if (data.deathClip != null)
                _controller["Die"] = data.deathClip;
            if (data.idleClip != null)
                _controller["Idle"] = data.idleClip;

            if (data.summonSound != null)
                this.summonSound = data.summonSound;
            if (data.attackSound != null)
                this.attackSound = data.attackSound;
            if (data.deathSound != null)
                this.deathSound = data.deathSound;
        }
        catch
        {
            Debug.LogError("Clip / Sound 설정이 되어있지 않음");
        }
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
            _audioSource.PlayOneShot(attackSound);
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
        _audioSource.PlayOneShot(deathSound);
        Unbind();
    }
}
