using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UI.Event;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [Space]
    [SerializeField] private Transform _spawnPoint;
    
    [SerializeField]  private List<GameObject> _players = new List<GameObject>();
    [SerializeField] private UIDataSO _uiDataSO;
    public int CurrentPlayersCount => _players.Count;
    
    private ChainManager _chainManager;

    private PhotonView _photonView;

    // Start is called before the first frame update
    void Start()
    {
        _chainManager = GetComponent<ChainManager>();
        Debug.Log("Network: Connecting");
        _photonView = GetComponent<PhotonView>();
        // PhotonNetwork.ConnectUsingSettings();
        DontDestroyOnLoad(gameObject);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        EventAggregator.Instance?.AddEventListener<SubmitCharacterEvent>(ConnectRoomUI);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        EventAggregator.Instance?.RemoveEventListener<SubmitCharacterEvent>(ConnectRoomUI);
    }

    public void ConnectRoomUI(SubmitCharacterEvent evt)
    {
        Debug.Log($"Network: Connecting {evt.CharacterId} {evt.RoomName}");
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public void ConnectRoom()
    {
        Vector3 pos = Vector3.forward * (_players.Count * 5f);
        pos = _spawnPoint == null ? pos  : pos + _spawnPoint.position;
        GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, pos, Quaternion.identity);
        InitAvatar(player);
        player.GetComponent<PlayerSetup>().IsLocalPlayer();
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

        PhotonNetwork.JoinOrCreateRoom(_uiDataSO.RoomName, null, null);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Network: Joined Room");
        // if(_photonView.IsMine)
        PhotonRaiseEventHandler.Instance.RaiseJoinRoomEvent();
        // EventAggregator.Instance.RaiseEvent(new PlayerJoinRoomEvent());
    }

    private void InitAvatar(GameObject player)
    {
        var playerModelController = player.GetComponent<PlayerModelController>();
        if (!playerModelController)
        {
            return;
        }
        photonView.RPC("SetModel", RpcTarget.AllBuffered, _uiDataSO.CharacterId);
    }

    private GameObject FindPlayerById(string playerId)
    {
        foreach (var obj in FindObjectsOfType<PlayerSetup>())
        {
            if (obj.GetComponent<PhotonView>().Owner.UserId == playerId)
                return obj.gameObject;
        }
        return null;
    }

    [PunRPC]
    public void RegisterPlayer(string playerID)
    {
        GameObject player = FindPlayerById(playerID);
        _players.Add(player);
        
        if (_players.Count >= 2)
        {
            // Get the last two players in the list
            GameObject player1 = _players[^2];
            GameObject player2 = _players[^1];

            // Call the CreateChain method with the last two players
            _chainManager.CreateChain(player1, player2);
        }
    }

    [PunRPC]
    public void DeregisterPlayer(string playerID)
    {
        GameObject player = FindPlayerById(playerID);
        if (!_players.Contains(player))
        {
            Debug.LogWarning("Player not found in the list.");
            return;
        }

        int index = _players.IndexOf(player);

        // Remove the player from the list
        _players.RemoveAt(index);

        // Handle chain removal and re-creation
        GameObject previousPlayer = index > 0 ? _players[index - 1] : null;
        GameObject nextPlayer = index < _players.Count ? _players[index] : null;

        // Remove the chain involving the current player
        if (nextPlayer != null)
        {
            _chainManager.DeleteChain(player);
        }

        // Remove the chain between previous and next (if exists)
        if (previousPlayer != null)
        {
            _chainManager.DeleteChain(previousPlayer);
        }

        // Create a new chain between previous and next
        if (previousPlayer != null && nextPlayer != null)
        {
            _chainManager.CreateChain(previousPlayer, nextPlayer);
        }
    }
    
}
