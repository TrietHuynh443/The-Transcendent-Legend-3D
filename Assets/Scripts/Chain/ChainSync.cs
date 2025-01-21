using System.Collections;
using System.Collections.Generic;
using Obi;
using Photon.Pun;
using UnityEngine;

public class ChainSync : MonoBehaviourPunCallbacks, IPunObservable
{
    private GameObject _solverObj;
    
    private ObiSolver _solver;
    private ChainManager _chainManager;
    void Start()
    {
        _chainManager = FindObjectOfType<ChainManager>();
    }

    public void SetSolverObj(GameObject solverObj)
    {
        _solverObj = solverObj;
        _solver = solverObj.GetComponent<ObiSolver>();
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // if (!_solver)
        //     return;
        //
        // if (stream.IsWriting)
        // {
        //     Debug.Log("Writing");
        //     stream.SendNext(_solver.renderablePositions.count);
        //     foreach (var pos in _solver.renderablePositions)
        //     {
        //         stream.SendNext(pos.x);
        //         stream.SendNext(pos.y);
        //         stream.SendNext(pos.z);
        //         stream.SendNext(pos.w);
        //
        //     }
        // }
        // else
        // {
        //     Debug.Log("Receiving");
        //     int count = (int)stream.ReceiveNext();
        //     Vector4[] positions = new Vector4[count];
        //     for (int i = 0; i < count; i++)
        //     {
        //         positions[i].x = (float)stream.ReceiveNext();
        //         positions[i].y = (float)stream.ReceiveNext();
        //         positions[i].z = (float)stream.ReceiveNext();
        //         positions[i].w = (float)stream.ReceiveNext();
        //     }
        //     
        //     Debug.Log("maxStepsPerFrame " + _solver.maxStepsPerFrame);
        //
        //     if (true)
        //     {
        //         Debug.Log("Positioning");
        //         _solver.renderablePositions.CopyFrom(positions, 0, 0, count);
        //     }
        //     
        // }
    }
}
