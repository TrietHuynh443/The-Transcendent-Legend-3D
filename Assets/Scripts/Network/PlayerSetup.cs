using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    private PlayerController _controller;
    [SerializeField] private GameObject _camera;

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _controller.enabled = false;
        
        _camera.SetActive(false);
    }
    public void IsLocalPlayer()
    {
        _controller.enabled = true;
        _camera.SetActive(true);
    }
}
