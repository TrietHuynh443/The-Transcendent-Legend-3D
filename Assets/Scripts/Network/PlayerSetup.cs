using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPunCallbacks
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
        if (photonView.IsMine)
        {
            _roomManager.GetComponent<PhotonView>().RPC("RegisterPlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.UserId);
        }
    }

    public void IsLocalPlayer()
    {
        _controller.enabled = true;
        _camera.SetActive(true);
    }

    void OnDestroy()
    {
        if (photonView.IsMine)
        {
            _roomManager?.GetComponent<PhotonView>().RPC("DeregisterPlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.UserId);
        }
    }
}
