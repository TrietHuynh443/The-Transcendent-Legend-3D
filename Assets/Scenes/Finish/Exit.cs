using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Exit : MonoBehaviour
{
    public Canvas canvas;
    private List<int> triggerList = new List<int>();

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        int trigger = other.gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
        if (!triggerList.Contains(trigger))
            triggerList.Add(trigger);
        
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int player = PhotonNetwork.PlayerList[i].ActorNumber;
            if (!triggerList.Contains(player))
                return;
        }
        
        AudioManager.Instance.musicSource.Stop();
        AudioManager.Instance.PlayMusic("Win");
        canvas.gameObject.SetActive(true);
    }
}
