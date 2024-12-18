using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    private Animator _animator;
    private float _horizontalMove;
    private float _verticalMove;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _horizontalMove = Input.GetAxis("Horizontal");
        _verticalMove = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        _rb.velocity = new Vector3(_horizontalMove, 0, _verticalMove);
        if(_rb.velocity.magnitude > 0.1f)
        {
            if (!_animator.GetBool("Move"))
            {
                _animator.SetBool("Move", true);
            }
        }
        else
        {
            _animator.SetBool("Move", false);
        }
    }
}
