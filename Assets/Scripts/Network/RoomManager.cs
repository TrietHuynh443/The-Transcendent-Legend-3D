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

    [SerializeField] private List<GameObject> _players = new List<GameObject>();
    [SerializeField] private Dictionary<(Player, Player), GameObject> _chains = new Dictionary<(Player, Player), GameObject>();
    
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
        
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        
        GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPoint.position + Vector3.forward * (playerCount * 5f), Quaternion.identity);
        player.GetComponent<PlayerSetup>().IsLocalPlayer();
        photonView.RPC("RegisterPlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        Debug.Log("Network: Left Room");
        
        photonView.RPC("UnregisterPlayer", RpcTarget.AllBuffered, player);
    }

    private GameObject FindPlayerById(int actorNumber)
    {
        foreach (var obj in FindObjectsOfType<PlayerSetup>())
        {
            if (obj.GetComponent<PhotonView>().Owner.ActorNumber == actorNumber)
                return obj.gameObject;
        }
        return null;
    }

    [PunRPC]
    public void RegisterPlayer(Player player)
    {
        Debug.Log("Network: Registering Player " + player.ActorNumber);
        
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length - 1; i++)
            {
                Player player1 = PhotonNetwork.PlayerList[i];
                Player player2 = PhotonNetwork.PlayerList[i + 1];

                if (!_chains.ContainsKey((player1, player2)))
                {
                    GameObject player1Obj = FindPlayerById(player1.ActorNumber);
                    GameObject player2Obj = FindPlayerById(player2.ActorNumber);

                    if (player1Obj != null && player2Obj != null)
                    {
                        Debug.Log("Creating chain between player " + player1.ActorNumber + " and " + player2.ActorNumber);
                        GameObject chain = _chainManager.CreateChain(player1Obj, player2Obj);
                        _chains.Add((player1, player2), chain);
                    }
                }
            }
        }
    }

    [PunRPC]
    public void UnregisterPlayer(Player player)
    {
        Debug.Log("Network: Unregister Player " + player.ActorNumber);
        
        Player previousPlayer = null;
        Player nextPlayer = null;

        foreach (var playerPair in _chains.Keys)
        {
            if (Equals(playerPair.Item1, player))
            {
                nextPlayer = playerPair.Item2;
            }
            if (Equals(playerPair.Item2, player))
            {
                previousPlayer = playerPair.Item1;
            }
        }

        if (_chains.ContainsKey((player, nextPlayer)))
        {
            Debug.Log("Destroying chain between player " + player.ActorNumber + " and " + nextPlayer.ActorNumber);
            Destroy(_chains[(player, nextPlayer)]);
            _chains.Remove((player, nextPlayer));
        }
        
        if (_chains.ContainsKey((previousPlayer, player)))
        {
            Debug.Log("Destroying chain between player " + previousPlayer.ActorNumber + " and " + player.ActorNumber);
            Destroy(_chains[(previousPlayer, player)]);
            _chains.Remove((previousPlayer, player));
        }

        if (previousPlayer != null && nextPlayer != null)
        {
            GameObject player1Obj = FindPlayerById(previousPlayer.ActorNumber);
            GameObject player2Obj = FindPlayerById(nextPlayer.ActorNumber);

            if (player1Obj != null && player2Obj != null)
            {
                Debug.Log("Creating chain between player " + previousPlayer.ActorNumber + " and " + nextPlayer.ActorNumber);
                GameObject chain = _chainManager.CreateChain(player1Obj, player2Obj);
                _chains.Add((previousPlayer, nextPlayer), chain);
            }
        }
        
       
    }
}
