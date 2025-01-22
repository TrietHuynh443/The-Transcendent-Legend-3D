using System;
using ExitGames.Client.Photon;
using Manager;
using Photon.Pun;
using UnityEngine;

namespace Network
{
    public class PhotonEventConsumer : UnitySingleton<PhotonEventConsumer>
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
                case GameEvent.StartGame:
                    Debug.Log("Consumed Play Event");
                    GameManager.Instance.Play();
                    break;
                case GameEvent.JoinRoom:
                    GameManager.Instance.IncreasePlayerNumber((int)payload.CustomData);
                    break;
            }
        }

        private void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= ConsumeEvent;
        }
        
    }
}