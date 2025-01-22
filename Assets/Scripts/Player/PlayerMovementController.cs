using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMovementController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private Transform _orientation;

    [Header("Movement")] [SerializeField] private float _moveSpeed;

    [SerializeField] private float _groundDrag;

    [SerializeField] private float _jumpForce;
    [SerializeField] private float _airMultiplier;
    private bool _readyToJump = true;
    private bool _isInAir = false;
    private bool _isMoving = false;
    private bool _isGrounded = false;

    public bool IsMoving => _isMoving;
    public bool IsGrounded => _isGrounded;

    public Vector2 Velocity => _rb.velocity;

    [Header("Keybinds")] [SerializeField] private KeyCode _jumpKey = KeyCode.Space;

    [Header("Ground Check")] [SerializeField]
    private LayerMask _groundLayer;

    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpInput;

    private Vector3 _moveDirection;

    private Rigidbody _rb;
    private Collider _collider;
    private PlayerAnimationController _playerAnim;

    [Header("Photon Sync")]
    [SerializeField] private bool _synchronizePosition = true;
    [SerializeField] private bool _synchronizeRotation = true;
    private Vector3 _storedPosition;
    private Vector3 _direction;
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;
    private PhotonView _chainManagerPhotonView;
    private bool _firstTake = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _collider = GetComponent<Collider>();
        _playerAnim = GetComponent<PlayerAnimationController>();
        _chainManagerPhotonView = FindObjectOfType<ChainManager>().GetComponent<PhotonView>();
    }

    public bool CanProcessPhysics()
    {
        return _chainManagerPhotonView.IsMine;
    }


    public void SetChainPosition(Vector3 position, Quaternion rotation, float lag)
    {
        if (!CanProcessPhysics())
        {
            _direction = position - _storedPosition;
            _storedPosition = transform.position;
            _networkPosition = position;
            _networkRotation = rotation;
            _networkPosition += _direction * lag;
        }
    }

    public void SetJumpForce(float force)
    {
        _jumpForce = force;
    }

    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
    }

    void Update()
    {
        //Ground check
        if (CanProcessPhysics())
        {
            GroundCheck();
        }

        if (photonView.IsMine)
        {
            MyInput();
        }

        if (CanProcessPhysics())
        {
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
        else
        {
            if (_firstTake)
            {
                transform.localPosition = _networkPosition;
                _firstTake = false;
            }
            else
            {
                float distance = Vector3.Distance(transform.localPosition, _networkPosition);
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, _networkPosition, distance  * Time.deltaTime * PhotonNetwork.SerializationRate);
                // transform.localRotation = Quaternion.RotateTowards(transform.localRotation, _networkRotation, this.m_Angle * Time.deltaTime * PhotonNetwork.SerializationRate);
            }
            
        }
    }

    public void SetReadyToJump(bool ready)
    {
        _readyToJump = ready;
    }

    void FixedUpdate()
    {
        if (CanProcessPhysics())
            MovePlayer();
    }

    private void GroundCheck()
    {
        _isGrounded = Physics.BoxCast(_collider.bounds.center, _collider.bounds.extents / 1.5f, Vector3.down,
            Quaternion.identity, _collider.bounds.extents.y / 2 + 0.5f, _groundLayer);
    }

    private void MyInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        _jumpInput = Input.GetKey(_jumpKey);
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

        if (!_isGrounded && Mathf.Abs(_rb.velocity.y) < 0.01)
        {
            // _readyToJump = true;
            // if (_isInAir)
            // {
            //     SetAnimation(PlayerAnimationState.JumpLand);
            // }
        }

        if (_jumpInput && _readyToJump && _isGrounded)
        {
            _readyToJump = false;
            Jump();
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_horizontalInput);
            stream.SendNext(_verticalInput);
            stream.SendNext(_jumpInput);
            stream.SendNext(_isMoving);
            stream.SendNext(_isGrounded);
        }
        else
        {
            _horizontalInput = (float)stream.ReceiveNext();
            _verticalInput = (float)stream.ReceiveNext();
            _jumpInput = (bool)stream.ReceiveNext();
            _isMoving = (bool)stream.ReceiveNext();
            _isGrounded = (bool)stream.ReceiveNext();
        }
    }
}