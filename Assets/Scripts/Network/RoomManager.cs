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
    
    [SerializeField]  private List<GameObject> _players = new List<GameObject>();
    
    public int CurrentPlayersCount => _players.Count;
    
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
        
        GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPoint.position + Vector3.forward * (_players.Count * 5f), Quaternion.identity);
        player.GetComponent<PlayerSetup>().IsLocalPlayer();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("1");
        
    }

    public void RegisterPlayer(GameObject player)
    {
        _players.Add(player);
        
        if (_players.Count >= 2)
        {
            Debug.Log("3");
            // Get the last two players in the list
            GameObject player1 = _players[^2];
            GameObject player2 = _players[^1];

            // Call the CreateChain method with the last two players
            _chainManager.CreateChain(player1, player2);
        }
    }
}
