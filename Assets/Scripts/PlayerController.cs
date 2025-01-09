using DG.Tweening;
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
    [SerializeField] private float _jumpForce = 50f;
    [Header("StairCheck")]
    [SerializeField] private Transform _stairCheckTransform;
    [SerializeField] float stepHeight = 0.2f; // Maximum step height



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
        if(Input.GetKeyDown(KeyCode.Space))
        { 
            _isJump = true;
            _playerAnimationManager?.UpdateJumpParams(_isJump, false);

        }
    }

    private void FixedUpdate()
    {
        CheckGround();
        if (_isInAir) {
            HandleInAir();
        }
        HandleRotate();
        _playerAnimationManager?.UpdateMovementParams(_horizontalMove, _verticalMove);
        //_playerAnimationManager?.UpdateJumpParams(_isJump, _isGrounded);
        _rigidbody.velocity = new Vector3(_horizontalMove, 0, _verticalMove);
        if (_isJump)
        {
            DoJump();
            _isJump = false;
            _isGrounded = false;
        }

        StepClimb();
    }

    private void StepClimb()
    {
        // Ensure there's movement input
        if (Mathf.Abs(_horizontalMove) <= 0.1f && Mathf.Abs(_verticalMove) <= 0.1f)
            return;

        // Raycast parameters
        float stepDepth = 0.2f;  // Depth of the step in front of the player
        Vector3 rayOrigin = _stairCheckTransform.position;
        Vector3 upperRayOrigin = rayOrigin + Vector3.up * stepHeight;

        // Raycast to detect a step
        if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit lowerHit, 0.3f))
        {
            // Perform a secondary raycast above the step to ensure it's climbable
            if (lowerHit.collider.tag != "Walkable") return;
            if (!Physics.Raycast(upperRayOrigin, transform.forward, 0.3f))
            {
                _rigidbody.position += new Vector3(0, stepHeight, 0) + transform.forward*0.1f;
            }
        }
    }

    private void HandleInAir()
    {
        var originVec = _rigidbody.velocity;
        _rigidbody.AddForce(0, -9.8f*2*9.8f, 0, ForceMode.Acceleration);
    }

    private void CheckGround()
    {
        Vector3 boxSize = new Vector3(0.2f, 0.1f, 0.2f);
        Vector3 boxCenter = transform.position + Vector3.down * (1.55f / 2);

        Collider[] overlaps = Physics.OverlapBox(boxCenter, boxSize, Quaternion.identity, LayerMask.GetMask("Ground"));
        _isGrounded = overlaps.Length > 0;
        _isInAir = true;
        if (_isGrounded) 
        {
            _isInAir = false;
            _rigidbody.velocity.Set(_rigidbody.velocity.x, 0, _rigidbody.velocity.y);
            _playerAnimationManager?.UpdateJumpParams(false, true);
        }
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

        if (direction.sqrMagnitude > 0.05f) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            //_rigidbody.rotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.deltaTime * 20f);
            _rigidbody.transform.DORotate(targetRotation.eulerAngles, 0.2f, RotateMode.Fast);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (_rigidbody == null) return;

        // Define the box size and position
        Vector3 boxSize = new Vector3(0.2f, 0.1f, 0.2f); // Same as used in BoxCast
        Vector3 boxCenter = transform.position + Vector3.down * (1.55f / 2); // Same center as BoxCast

        // Set Gizmos color based on whether the object is grounded
        Gizmos.color = _isGrounded ? Color.green : Color.red;

        // Draw the box for debugging
        Gizmos.DrawWireCube(boxCenter, boxSize * 2); // Multiply size by 2 since it's the full size, not half-size
    }
}
