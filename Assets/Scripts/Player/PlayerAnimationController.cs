using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum PlayerAnimationState
    {
        None,
        Idle,
        Walk,
        Run,
        JumpStart,
        JumpIdle,
        JumpLand,
    }
    
    private Dictionary<PlayerAnimationState, string> _animationStateNames;

    
    private PlayerModelController _playerModel;
    private PlayerMovementController _playerMovement;
    private PlayerAnimationState _prevAnimState = PlayerAnimationState.Idle;
    private PlayerAnimationState _animState = PlayerAnimationState.Idle;
    void Start()
    {
        _playerModel = GetComponent<PlayerModelController>();
        _playerMovement = GetComponent<PlayerMovementController>();
        
        _animationStateNames = new Dictionary<PlayerAnimationState, string>
        {
            { PlayerAnimationState.Idle, "Idle" },
            { PlayerAnimationState.Walk, "Walk_A" },
            { PlayerAnimationState.Run, "Running_A" },
            { PlayerAnimationState.JumpStart, "Jump_Start" },
            { PlayerAnimationState.JumpIdle, "Jump_Idle" },
            { PlayerAnimationState.JumpLand, "Jump_Land" }
        };
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            UpdateAnimation();
        }
    }

    private bool IsAnimationPlaying(PlayerAnimationState state)
    {
        return _animState == state;
        // return _playerModel.Animator.GetCurrentAnimatorStateInfo(0).IsName(_animationStateNames[state]);
    }

    private float AnimationTime()
    {
        return _playerModel.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public void SetAnimation(PlayerAnimationState state)
    {
        _prevAnimState = _animState;
        _animState = state;
    }

    private void UpdateAnimation()
    {
        if (IsAnimationPlaying(PlayerAnimationState.JumpStart))
        {
            if (AnimationTime() >= 1)
            {
                SetAnimation(PlayerAnimationState.JumpIdle);
            }
        }
        else if (IsAnimationPlaying(PlayerAnimationState.JumpIdle))
        {
            if (_playerMovement.IsGrounded || Mathf.Abs(_playerMovement.Velocity.y) < 0.01)
            {
                SetAnimation(PlayerAnimationState.JumpLand);
            }
        }
        else if (IsAnimationPlaying(PlayerAnimationState.JumpLand))
        {
            if (AnimationTime() >= 1 || (_playerMovement.IsGrounded && _playerMovement.IsMoving && AnimationTime() >= 0.5))
            {
                _playerMovement.SetReadyToJump(true);
                SetAnimation(_playerMovement.IsMoving ? PlayerAnimationState.Run : PlayerAnimationState.Idle);
            }
        }
        else if (IsAnimationPlaying(PlayerAnimationState.Run))
        {
            if (!_playerMovement.IsMoving)
            {
                SetAnimation(PlayerAnimationState.Idle);
            }
        }
        else if (IsAnimationPlaying(PlayerAnimationState.Idle))
        {
            if (_playerMovement.IsMoving)
            {
                SetAnimation(PlayerAnimationState.Run);
            }
        }

        if (_prevAnimState != _animState)
        {
            _playerModel.Animator.Play(_animationStateNames[_animState]);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_prevAnimState);
            stream.SendNext(_animState);
        }
        else
        {
            _prevAnimState = (PlayerAnimationState)stream.ReceiveNext();
            _animState = (PlayerAnimationState)stream.ReceiveNext();
            if (_prevAnimState != _animState)
            {
                _playerModel.Animator.Play(_animationStateNames[_animState]);
            }
        }
    }
}
