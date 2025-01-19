using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerModelController : MonoBehaviour
{
    [SerializeField] private List<GameObject> _models;
    [SerializeField] private GameObject _playerObj;
    
    private Animator _animator;
    public Animator Animator => _animator;

    private int _currentModel = 0;

    public int CurrentModel => _currentModel;

    // Start is called before the first frame update
    void Start()
    {
        SetModel(0);
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