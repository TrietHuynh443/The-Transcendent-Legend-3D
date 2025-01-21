using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMovementController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform _orientation;
    
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    
    [SerializeField] private float _groundDrag;
    
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _airMultiplier;
    private bool _readyToJump = true;
    private bool _isInAir = false;
    private bool _isMoving = false;
    public bool IsMoving => _isMoving;
    public bool IsGrounded => _isGrounded;
    
    public Vector2 Velocity => _rb.velocity;
    
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
    private PlayerAnimationController _playerAnim;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _collider = GetComponent<Collider>();
        _playerModel = GetComponent<PlayerModelController>();
        _playerAnim = GetComponent<PlayerAnimationController>();
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
    }

    public void SetReadyToJump(bool ready)
    {
        _readyToJump = ready;
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void GroundCheck()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.05f, _groundLayer);
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
        
        _playerAnim.SetAnimation(PlayerAnimationController.PlayerAnimationState.JumpStart);
    }
}
