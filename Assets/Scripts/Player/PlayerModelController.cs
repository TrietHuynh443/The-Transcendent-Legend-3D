using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerModelController : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<GameObject> _models;
    [SerializeField] private GameObject _playerObj;
    
    private Animator _animator;
    [SerializeField] private float _playerHeight;
    [SerializeField] private UIDataSO _uiDataSO;
    public Animator Animator => _animator;
    public float PlayerHeight => _playerHeight;

    private int _currentModel = 0;
    public int CurrentModel => _currentModel;

    // Start is called before the first frame update
    void Start()
    {
        int currentPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        if (photonView.IsMine)
        {
            photonView.RPC("SetModel", RpcTarget.AllBuffered, _uiDataSO.CharacterId % _models.Count);
        }
    }

    [PunRPC]
    public void SetModel(int modelID)
    {
        if (modelID < 0 || modelID >= _models.Count)
            modelID = 0;
        
        foreach (Transform child in _playerObj.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject model = Instantiate(_models[modelID], _playerObj.transform);
        _animator = model.GetComponent<Animator>();
        _currentModel = modelID;
    }
}