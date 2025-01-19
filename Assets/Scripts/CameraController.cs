using System;
using UnityEngine;

namespace CameraController
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] [Range(0,1f)] private float _sensitivity;
        private float pitch; // vertical look
        private float yaw; // horizontal look
        private void Update()
        {
            pitch -= Input.GetAxis("Mouse Y") * _sensitivity;
            yaw = Input.GetAxis("Mouse X") * _sensitivity;
            pitch = Mathf.Clamp(pitch, -30, 60);
        }

        private void LateUpdate()
        {
            _target.transform.rotation *= Quaternion.AngleAxis(yaw, Vector3.up);
            var currentEuler = _target.eulerAngles;
            _target.transform.eulerAngles = new Vector3(pitch, currentEuler.y, currentEuler.z);
        }
    }
}