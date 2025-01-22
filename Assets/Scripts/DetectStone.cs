using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectStone : MonoBehaviour
{
    public GameObject stones;
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;
        stones.SetActive(true);
        AudioManager.Instance.PlaySFX("Stone");
        isTriggered = true;
    }
}
