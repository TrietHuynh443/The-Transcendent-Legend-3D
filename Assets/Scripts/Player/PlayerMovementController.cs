using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    enum PlayerAnimationState
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

    [SerializeField] private Transform _orientation;
    
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    
    [SerializeField] private float _groundDrag;
    
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _jumpCooldown;
    [SerializeField] private float _airMultiplier;
    private bool _readyToJump = true;
    private bool _isInAir = false;
    private bool _isMoving = false;
    
    [Header("Keybinds")]
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;
    
    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayer;
    private bool _isGrounded;
    
    private float _horizontalInput;
    private float _verticalInput;
    
    private Vector3 _moveDirection;
    
    private Rigidbody _rb;
    private Collider _collider;
    private PlayerModelController _playerModel;
    private PlayerAnimationState _prevAnimState = PlayerAnimationState.Idle;
    private PlayerAnimationState _animState = PlayerAnimationState.Idle;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _collider = GetComponent<Collider>();
        _playerModel = GetComponent<PlayerModelController>();
        
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
        //Ground check
        GroundCheck();
        
        MyInput();
        SpeedControl();
        
        //Handle drag
        if (_isGrounded)
        {
            _rb.drag = _groundDrag;
            _isInAir = false;
        }
        else
        {
            _rb.drag = 0;
            _isInAir = true;
        }

        if (_horizontalInput != 0 || _verticalInput != 0)
        {
            _isMoving = true;
        }
        else
        {
            _isMoving = false;
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void GroundCheck()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.05f, _groundLayer);
    }

    private bool IsAnimationPlaying(PlayerAnimationState state)
    {
        return _playerModel.Animator.GetCurrentAnimatorStateInfo(0).IsName(_animationStateNames[state]);
    }

    private float AnimationTime()
    {
        return _playerModel.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    private void SetAnimation(PlayerAnimationState state)
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
                _animState = PlayerAnimationState.JumpIdle;
            }
        }
        else if (IsAnimationPlaying(PlayerAnimationState.JumpIdle))
        {
            if (Mathf.Abs(_rb.velocity.y) < 0.01)
            {
                SetAnimation(PlayerAnimationState.JumpLand);
                Debug.Log("Land");
                AudioManager.Instance.PlaySFX("Land");
            }
        }
        else if (IsAnimationPlaying(PlayerAnimationState.JumpLand))
        {
            if (AnimationTime() >= 1 || (_isGrounded && _isMoving && AnimationTime() >= 0.5))
            {
                SetAnimation(_isMoving ? PlayerAnimationState.Run : PlayerAnimationState.Idle);
                _readyToJump = true;
            }
        }
        else if (IsAnimationPlaying(PlayerAnimationState.Run))
        {
            if (!_isMoving)
            {
                SetAnimation(PlayerAnimationState.Idle);

            }
        }
        else if (IsAnimationPlaying(PlayerAnimationState.Idle))
        {
            if (_isMoving)
            {
                SetAnimation(PlayerAnimationState.Run);
            }
        }

        if (_prevAnimState != _animState)
        {
            _playerModel.Animator.Play(_animationStateNames[_animState]);
        }
    }

    private void MyInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (!_isGrounded && Mathf.Abs(_rb.velocity.y) < 0.01)
        {
            // _readyToJump = true;
            // if (_isInAir)
            // {
            //     SetAnimation(PlayerAnimationState.JumpLand);
            // }
        }

        if (Input.GetKey(_jumpKey) && _readyToJump && _isGrounded)
        {
            _readyToJump = false;
            Jump();
        }
    }

    private void MovePlayer()
    {
        _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;

        if (_isGrounded)
        {
            _rb.AddForce(_moveDirection.normalized * (_moveSpeed * 10f), ForceMode.Force);
        }
        else if (!_isGrounded)
        {
            _rb.AddForce(_moveDirection.normalized * (_moveSpeed * 10f * _airMultiplier), ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

        if (flatVel.magnitude > _moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * _moveSpeed;
            _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        
        _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
        
        SetAnimation(PlayerAnimationState.JumpStart);
    }
}
