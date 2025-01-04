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
    [SerializeField] private float _jumpForce;
    [SerializeField] private GameObject _stepRayUpper;
    [SerializeField] private GameObject _stepRayLower;
    [SerializeField] private float _smoothClimbDistance = 0.2f;
    [SerializeField] private float _stepSmooth = 0.2f;


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
        Debug.DrawRay(_stepRayLower.transform.position, transform.TransformDirection(Vector3.forward) * _smoothClimbDistance, Color.red);
        Debug.DrawRay(_stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward) * _smoothClimbDistance, Color.green);
        //if (Mathf.Abs(_horizontalMove) <= 0.1f) return;
        RaycastHit lowerHit;
        RaycastHit upperHit;
        if (Physics.Raycast(_stepRayLower.transform.position, transform.TransformDirection(Vector3.forward), out lowerHit, _smoothClimbDistance))
        {
            Debug.Log("hit1");
            if (!Physics.Raycast(_stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward), out upperHit, _smoothClimbDistance)) 
            {
                _rigidbody.position += new Vector3(0, _stepSmooth, 0);
                Debug.Log("unhit 2 --> up");

            }
        }
        if (Physics.Raycast(_stepRayLower.transform.position, transform.TransformDirection(new Vector3(1.5f, 0, 1)), out lowerHit, _smoothClimbDistance))
        {
            if (!Physics.Raycast(_stepRayUpper.transform.position, transform.TransformDirection(new Vector3(1.5f, 0, 1)), out upperHit, _smoothClimbDistance))
            {
                _rigidbody.position += new Vector3(0, _stepSmooth, 0);
            }
        }

        if (Physics.Raycast(_stepRayLower.transform.position, transform.TransformDirection(new Vector3(-1.5f, 0, 1)), out lowerHit, _smoothClimbDistance))
        {
            if (!Physics.Raycast(_stepRayUpper.transform.position, transform.TransformDirection(new Vector3(-1.5f, 0, 1)), out upperHit, _smoothClimbDistance))
            {
                _rigidbody.position += new Vector3(0, _stepSmooth, 0);
            }
        }


    }

    private void HandleInAir()
    {
        _rigidbody.AddForce(new Vector3(0, -9.8f * 8, 0), ForceMode.Acceleration);
    }

    private void CheckGround()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.55f/2 + 0.2f, LayerMask.GetMask("Ground"));
        _isInAir = true;
        if (_isGrounded) 
        {
            _isInAir = false;
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

        if (direction.sqrMagnitude > 0.01f) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            //_rigidbody.rotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.deltaTime * 20f);
            _rigidbody.transform.DORotate(targetRotation.eulerAngles, 0.2f, RotateMode.Fast);
        }
    }
}
