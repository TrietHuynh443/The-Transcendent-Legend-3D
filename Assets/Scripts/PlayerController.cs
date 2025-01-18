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
    [SerializeField] private float _jumpForce = 20f;
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

    [SerializeField] private float maxJumpTime = 0.2f;  // Time player can hold jump for max height
    [SerializeField] private float jumpHoldForce = 20f; // Force applied when holding the jump button
    [SerializeField] private float coyoteTime = 0.2f;   // Allow jump shortly after leaving the ground
    private float _jumpTimer;
    private bool _isHoldingJump;
    private float _coyoteTimer;
    
    private void DoJump()
    {
        // Check if within coyote time or grounded
        if (!_isGrounded && _coyoteTimer <= 0f) return;
    
        // Initial jump force
        _rigidbody.AddForce(new Vector3(0, _jumpForce, 0), ForceMode.Impulse);
    
        // Set state variables
        _isInAir = true;
        _coyoteTimer = 0f; // Reset coyote time
        _jumpTimer = 0f;   // Reset jump hold timer
        _isHoldingJump = true;
    
        // Update animation
        _playerAnimationManager?.UpdateJumpParams(true, false);
    }

    private void Update()
    {
        // Capture jump input
        _horizontalMove = Input.GetAxis("Horizontal");
        _verticalMove = Input.GetAxis("Vertical");
    
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _isJump = true;
        }
    
        if (Input.GetKey(KeyCode.Space))
        {
            _isHoldingJump = true;
        }
        else
        {
            _isHoldingJump = false;
        }
    
        // Increment coyote timer if not grounded
        if (!_isGrounded)
        {
            _coyoteTimer += Time.deltaTime;
        }
        else
        {
            _coyoteTimer = 0f;
        }
    }
    
    private void FixedUpdate()
    {
        CheckGround();
        if (_isInAir) HandleInAir();
        HandleRotate();
        _playerAnimationManager?.UpdateMovementParams(_horizontalMove, _verticalMove);
        _rigidbody.velocity = new Vector3(_horizontalMove, _rigidbody.velocity.y, _verticalMove);
    
        if (_isJump)
        {
            DoJump();
            _isJump = false;
        }
    
        if (_isHoldingJump && _jumpTimer < maxJumpTime)
        {
            // Add extra force while jump button is held, within the allowed time
            _rigidbody.AddForce(new Vector3(0, jumpHoldForce * Time.fixedDeltaTime, 0), ForceMode.Force);
            _jumpTimer += Time.fixedDeltaTime;
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
        _rigidbody.AddForce(0, -9.8f*12f, 0, ForceMode.Force);
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

    private void HandleRotate()
    {
        Vector3 direction = new Vector3(_horizontalMove, 0, _verticalMove);

        if (direction.sqrMagnitude > 0.15f) 
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
