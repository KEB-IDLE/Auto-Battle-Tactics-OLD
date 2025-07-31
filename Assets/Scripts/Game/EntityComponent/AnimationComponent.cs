using System.Collections;
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

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
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
        if (data.summonSound != null)
            this.summonSound = data.summonSound;
        if (data.attackSound != null)
            this.attackSound = data.attackSound;
        if (data.deathSound != null)
            this.deathSound = data.deathSound;
    }

    public void HandleAttack(bool isAttacking)
    {
        if (isAttacking)
        {
            _animator.SetBool("Attack", true);
            if(this.attackSound != null)
                _audioSource.PlayOneShot(attackSound);
        }
        else
            _animator.SetBool("Attack", false);
    }
            

    public void HandleMove()
    {
        _animator.SetTrigger("Move");
    }

    public void HandleDeath()
    {
        Debug.Log("DeathAnimation is Called");
        _animator.SetTrigger("Death");
        if(this.deathSound != null)
            _audioSource.PlayOneShot(deathSound);
    }
}

