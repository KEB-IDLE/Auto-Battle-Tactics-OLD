using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class AnimationComponent : MonoBehaviour
{
    private Animator _animator;
    private AnimatorOverrideController _controller;
    private AudioSource _audioSource;
    private AudioClip summonSound;
    private AudioClip attackSound;
    private AudioClip deathSound;


    void Awake()
    {
        _animator = GetComponent<Animator>();
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

    public void HandleAttack(bool isAttacking)
    {
        if (isAttacking)
            _animator.SetBool("Attack", true);
        else 
            _animator.SetBool("Attack", false);
    }

    public void HandleDeath()
    {
        _animator.SetTrigger("Death");
    }
    public void HandleMove() => _animator.SetTrigger("Move");
    public void StopAllAction()
    {
        _animator.SetBool("Attack", false);
        _animator.SetTrigger("StopAllAction");
    }
}

