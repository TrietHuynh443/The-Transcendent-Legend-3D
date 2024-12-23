using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private float _horizontalMove;
    private float _verticalMove;
    private bool _isJump;
    private PlayerAnimationManager _playerAnimationManager;
    private bool _isInAir;
    private bool _isGrounded;
    [SerializeField] private float _jumpForce;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerAnimationManager = GetComponent<PlayerAnimationManager>();
        _isGrounded = true;

    }

    // Update is called once per frame
    void Update()
    {
        _horizontalMove = Input.GetAxis("Horizontal");
        _verticalMove = Input.GetAxis("Vertical");
        if(Input.GetKeyDown(KeyCode.Space)) { _isJump = true; }
    }

    private void FixedUpdate()
    {
        if (_isInAir) {
            HandleInAir();
            CheckGround();
            _playerAnimationManager.UpdateJumpParams(_isJump, _isGrounded);
        }
        HandleRotate();
        _playerAnimationManager?.UpdateMovementParams(_horizontalMove, _verticalMove);
        _playerAnimationManager?.UpdateJumpParams(_isJump, _isGrounded);
        _rigidbody.velocity = new Vector3(_horizontalMove, 0, _verticalMove);
        if (_isGrounded && _isJump)
        {
            DoJump();
            _isJump = false;
            _isGrounded = false;
        }

    }

    private void HandleInAir()
    {
        _rigidbody.AddForce(new Vector3(0, -9.8f * 8, 0), ForceMode.Acceleration);
    }

    private void CheckGround()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.55f/2 + 0.1f, LayerMask.GetMask("Ground"));
        if (_isGrounded) _isInAir = false;
    }

    private void DoJump()
    {
        if (!_isGrounded) { return; }
        _rigidbody.AddForce(new Vector3(0, _jumpForce, 0), ForceMode.Impulse);
        _isInAir = true;
    }

    private void HandleRotate()
    {
        Vector3 direction = new Vector3(_horizontalMove, 0, _verticalMove);

        if (direction.sqrMagnitude > 0.01f) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            _rigidbody.rotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.deltaTime * 20f);
        }
    }
}
