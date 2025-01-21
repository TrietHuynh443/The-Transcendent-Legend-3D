using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [Space]
    [SerializeField] private Transform _spawnPoint;
    
    private ChainManager _chainManager;
    // Start is called before the first frame update
    void Start()
    {
        _chainManager = GetComponent<ChainManager>();
        Debug.Log("Network: Connecting");
        
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Network: Connected to Server");
        
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Network: Joined Lobby");

        PhotonNetwork.JoinOrCreateRoom("Room", null, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Network: Joined Room");
        
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        
        GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPoint.position + Vector3.forward * (playerCount * 5f), Quaternion.identity);
        player.GetComponent<PlayerSetup>().IsLocalPlayer();
        photonView.RPC("RegisterPlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        Debug.Log("Network: Left Room " + player.ActorNumber);
        
        photonView.RPC("UnregisterPlayer", RpcTarget.AllBuffered, player);
    }


    [PunRPC]
    public void RegisterPlayer(Player player)
    {
        Debug.Log("Network: Registering Player " + player.ActorNumber);
        
        _chainManager.UpdateChainPlayerJoin(player);
    }

    [PunRPC]
    public void UnregisterPlayer(Player player)
    {
        Debug.Log("Network: Unregister Player " + player.ActorNumber);
        
        _chainManager.UpdateChainForPlayerLeave(player);
    }
}
