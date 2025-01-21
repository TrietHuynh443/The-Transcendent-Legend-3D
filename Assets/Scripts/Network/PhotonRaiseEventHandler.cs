using ExitGames.Client.Photon;
using Manager;
using Network.SO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameEvent
{
    public const byte JoinRoom = 1;
    public const byte StartGame = 2;
    public const byte WinningGame = 3;
    public const byte LosingGame = 4;
}


public class PhotonRaiseEventHandler : UnitySingleton<PhotonRaiseEventHandler>
{
    [SerializeField] private CheckPointSO _checkPointSO;

    public void RaisePlayEvent()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(GameEvent.StartGame, null, raiseEventOptions, SendOptions.SendReliable);
    }
    
    public void RaiseWinningEvent()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(GameEvent.WinningGame, null, raiseEventOptions, SendOptions.SendReliable);
    }

    public void RaiseLosingEvent()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(GameEvent.LosingGame, null, raiseEventOptions, SendOptions.SendReliable);
    }

    public void RaiseJoinRoomEvent()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(GameEvent.JoinRoom, PhotonNetwork.CurrentRoom.PlayerCount, raiseEventOptions, SendOptions.SendReliable);
    }
}
