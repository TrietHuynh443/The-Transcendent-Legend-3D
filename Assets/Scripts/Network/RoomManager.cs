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
        Debug.Log("Network: Joined Room");
        // if(_photonView.IsMine)
        PhotonRaiseEventHandler.Instance.RaiseJoinRoomEvent();
        // EventAggregator.Instance.RaiseEvent(new PlayerJoinRoomEvent());
        photonView.RPC("RegisterPlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
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
