using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UI.Event;

public class RoomChainedManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private ChainManager _chainManager;

    public void PlayerJoinRoom()
    {
        Debug.Log("RoomChainManager/PlayerJoinRoom");
        photonView.RPC("RegisterPlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
    }

    public void PlayerLeftRoom(Player player)
    {
        Debug.Log("RoomChainManager/PlayerLeftRoom " + player.ActorNumber);
        
        photonView.RPC("UnregisterPlayer", RpcTarget.AllBuffered, player);
    }


    [PunRPC]
    public void RegisterPlayer(Player player)
    {
        Debug.Log("RoomChainManager/RegisterPlayer " + player.ActorNumber);
        
        _chainManager.
            UpdateChainPlayerJoin(
                player);
    }

    [PunRPC]
    public void UnregisterPlayer(Player player)
    {
        Debug.Log("RoomChainManager/UnregisterPlayer " + player.ActorNumber);
        
        _chainManager.UpdateChainForPlayerLeave(player);
    }
}
