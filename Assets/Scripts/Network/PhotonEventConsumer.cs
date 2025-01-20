using System;
using ExitGames.Client.Photon;
using Manager;
using Photon.Pun;
using UnityEngine;

namespace Network
{
    public class PhotonEventConsumer : MonoBehaviour
    {
        private void OnEnable()
        {
            PhotonNetwork.NetworkingClient.EventReceived += ConsumeEvent;
        }

        private void ConsumeEvent(EventData payload)
        {
            switch (payload.Code)
            {
                case GameEvent.WinningGame:
                    GameManager.Instance.LoadWinningScene();
                    break;
            }
        }

        private void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= ConsumeEvent;
        }
        
    }
}