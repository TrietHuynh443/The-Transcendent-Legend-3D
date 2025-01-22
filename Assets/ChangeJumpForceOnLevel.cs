using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class ChangeJumpForceOnLevel : MonoBehaviour
{
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _moveSpeed;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerMovementController>().SetJumpForce(_jumpForce);
            other.gameObject.GetComponent<PlayerMovementController>().SetMoveSpeed(_moveSpeed);
        }
    }
}
