using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationManager : MonoBehaviour
{
    private Animator _animator;
    private int _horizontalParam;
    private int _verticalParam;
    private int _isJumpParam;
    private int _isGroundedParam;

    // Start is called before the first frame update

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _horizontalParam = Animator.StringToHash("Horizontal");
        _verticalParam = Animator.StringToHash("Vertical");
        _isGroundedParam = Animator.StringToHash("IsGrounded");
        _isJumpParam = Animator.StringToHash("IsJump");
    }

    public void UpdateMovementParams(float horizontal, float vertical)
    {
        var (snappedHorizontal, snappedVertical) = SnappingMoveParams(horizontal, vertical);
        _animator.SetFloat(_verticalParam, snappedVertical);
        _animator.SetFloat(_horizontalParam, snappedHorizontal);
    }

    private (float snapepdHorizontal, float snappedVertical) SnappingMoveParams(float horizontal, float vertical)
    {
        #region SnappingHorizontal
        horizontal = Mathf.Abs(horizontal);
        if (horizontal > 0.1f && horizontal <= 0.55f)
        {
            horizontal = 0.55f;
        }
        else if(horizontal > 0.55)
        {
            horizontal = 1f;
        }
        #endregion


        #region SnappingVertical
        vertical = Mathf.Abs(vertical);
        if (vertical > 0.1f && vertical <= 0.55f)
        {
            vertical = 0.55f;
        }
        else if(vertical > 0.55f)
        {
            vertical = 1f;
        }
        #endregion
        return (horizontal, vertical); 
    }

    public void UpdateJumpParams(bool isJump, bool isGrounded)
    {
        _animator.SetBool(_isJumpParam, isJump);
        _animator.SetBool(_isGroundedParam, isGrounded);
    }
}
