using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    private PlayerMovementController _controller;
    [SerializeField] private GameObject _camera;
    private RoomManager _roomManager;

    void Awake()
    {
        _controller = GetComponent<PlayerMovementController>();
        _controller.enabled = false;
        
        _camera.SetActive(false);
        _roomManager = FindObjectOfType<RoomManager>();
        _roomManager.RegisterPlayer(transform.gameObject);
    }

    public void IsLocalPlayer()
    {
        _controller.enabled = true;
        _camera.SetActive(true);
    }
}
