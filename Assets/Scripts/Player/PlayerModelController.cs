using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerModelController : MonoBehaviour
{
    [SerializeField] private List<GameObject> _models;
    [SerializeField] private GameObject _playerObj;
    
    private Animator _animator;
    [SerializeField] private float _playerHeight;
    public Animator Animator => _animator;
    public float PlayerHeight => _playerHeight;

    private int _currentModel = 0;

    public int CurrentModel => _currentModel;

    // Start is called before the first frame update
    void Start()
    {
        RoomManager _roomManager = FindObjectOfType<RoomManager>();
        int currentPlayerCount = _roomManager.CurrentPlayersCount;
        SetModel(currentPlayerCount % _models.Count);
    }

    public void SetModel(int modelID)
    {
        if (modelID < 0 || modelID >= _models.Count)
            modelID = 0;
        
        foreach (Transform child in _playerObj.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject model = Instantiate(_models[modelID], _playerObj.transform);
        // GameObject model = PhotonNetwork.Instantiate(_models[modelID].name, _playerObj.transform.position, Quaternion.identity);
        _animator = model.GetComponent<Animator>();
        _animator.Play("Idle");
        _currentModel = modelID;
    }
}