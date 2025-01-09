using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [Space]
    [SerializeField] private Transform _spawnPoint;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Network: Connecting");
        
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        
        Debug.Log("Network: Connected to Server");
        
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        
        Debug.Log("Network: Joined Lobby");

        PhotonNetwork.JoinOrCreateRoom("Room", null, null);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        
        Debug.Log("Network: Joined Room");
        
        GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPoint.position, Quaternion.identity);
        player.GetComponent<PlayerSetup>().IsLocalPlayer();
    }
}
