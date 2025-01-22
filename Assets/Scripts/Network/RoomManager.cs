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
    
    [SerializeField] private UIDataSO _uiDataSO;
    
    private RoomChainedManager _roomChainedManager;
    private bool _testRoom = false; 

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Network: Connecting");
        
        DontDestroyOnLoad(gameObject);
    }

    public void SetTestRoom(bool enable)
    {
        _testRoom = enable;

        if (_testRoom)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
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
        
        Transform spawnPoint = spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber % spawnPoints.Length].transform;
        GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPoint.position, Quaternion.identity);
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

        if (_testRoom)
        {
            PhotonNetwork.JoinOrCreateRoom("Test Room", null, null);
        }
        else
        {
            PhotonNetwork.JoinOrCreateRoom(_uiDataSO.RoomName, null, null);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("RoomManager/OnJoinedRoom");
        // if(_photonView.IsMine)
        PhotonRaiseEventHandler.Instance.RaiseJoinRoomEvent();

        if (_testRoom)
        {
            ConnectRoom();
        }
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
