using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UI.Event;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [Space]
    
    [SerializeField]  private List<GameObject> _players = new List<GameObject>();
    [SerializeField] private UIDataSO _uiDataSO;
    public int CurrentPlayersCount => _players.Count;
    
    private RoomChainedManager _roomChainedManager;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Network: Connecting");
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
        Debug.Log("RoomManager/ConnectRoom");
        // StartCoroutine(Wait)
        _roomChainedManager = FindObjectOfType<RoomChainedManager>();
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        
        Transform spawnPoint = spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber % 4].transform;
        Vector3 pos = Vector3.forward * (_players.Count * 5f);
        pos = spawnPoint == null ? pos  : pos + spawnPoint.position;
        GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, pos, Quaternion.identity);
        InitAvatar(player);
        player.GetComponent<PlayerSetup>().IsLocalPlayer();;
        _roomChainedManager.PlayerJoinRoom();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Network: Connected to Server");
        
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Network: Joined Lobby");

        PhotonNetwork.JoinOrCreateRoom(_uiDataSO.RoomName, null, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("RoomManager/OnJoinedRoom");
        // if(_photonView.IsMine)
        PhotonRaiseEventHandler.Instance.RaiseJoinRoomEvent();
        // EventAggregator.Instance.RaiseEvent(new PlayerJoinRoomEvent()
        // photonView.RPC("RegisterPlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
    }

    public override void OnCreatedRoom()
    {
        GameManager.Instance.IsMaster = true;
    }

    private void InitAvatar(GameObject player)
    {
        var playerModelController = player.GetComponent<PlayerModelController>();
        if (!playerModelController)
        {
            return;
        }
        // photonView.RPC("SetModel", RpcTarget.AllBuffered, _uiDataSO.CharacterId);
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        Debug.Log("RoomManager/OnPlayerLeftRoom " + player.ActorNumber);
        _roomChainedManager.PlayerLeftRoom(player);
        
        // photonView.RPC("UnregisterPlayer", RpcTarget.AllBuffered, player);
    }
}
