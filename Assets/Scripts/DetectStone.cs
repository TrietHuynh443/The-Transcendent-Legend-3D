using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DetectStone : MonoBehaviour
{
    public GameObject stones;
    private bool isTriggered = false;
    private List<int> triggerList = new List<int>();

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        int trigger = other.gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
        if (!triggerList.Contains(trigger))
            triggerList.Add(trigger);
        
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int player = PhotonNetwork.PlayerList[i].ActorNumber;
            if (!triggerList.Contains(player))
                return;
        }
        
        stones.SetActive(true);
        AudioManager.Instance.PlaySFX("Stone");
        isTriggered = true;
    }
}
