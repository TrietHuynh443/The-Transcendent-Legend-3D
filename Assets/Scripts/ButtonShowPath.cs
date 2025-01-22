using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

public class ButtonShowPath : MonoBehaviour
{
    public GameObject path;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter()
    {
        AudioManager.Instance.PlaySFX("ButtonPress");
        path.SetActive(true);
        
        // Set transform of position y lower than 0.5
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z);
    }

    void OnTriggerExit()
    {
        path.SetActive(false);
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
    }
}
