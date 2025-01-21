using System.Collections;
using System.Collections.Generic;
using Obi;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class ChainManager : MonoBehaviourPunCallbacks
{
    private Dictionary<GameObject, GameObject> _solvers = new Dictionary<GameObject, GameObject>();
    [SerializeField] private GameObject _solverPrefab;
    [SerializeField] private GameObject _chainPrefab;

    [Header("Chain Config")] [SerializeField]
    private float _playerMidpointRate = 0.35f;

    [Range(0, 15)] [SerializeField] private int _playerCategory = 5;
    [Range(0, 15)] [SerializeField] private int _chainCategory = 10;
    [SerializeField] private float _controlPointMass = 0.01f;

    public void CreateChain(GameObject player1, GameObject player2)
    {
        GameObject solver = Instantiate(_solverPrefab);
        _solvers.Add(player1, solver);

        StartCoroutine(CreateChainBluePrint(solver, player1, player2));
    }

    public void DeleteChain(GameObject player1)
    {
        Destroy(_solvers[player1]);
        _solvers.Remove(player1);
    }

    private IEnumerator CreateChainBluePrint(GameObject solver, GameObject player1, GameObject player2)
    {
        GameObject chain = Instantiate(_chainPrefab, solver.transform);
        ObiRope rope = chain.GetComponent<ObiRope>();
        rope.maxBending = 0.02f;

        yield return 0;

        // Create a new blueprint for the rope
        ObiRopeBlueprint blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.5f;

        // Calculate the desired attachment points (midpoints of players)
        Vector3 attachmentPoint1 = player1.transform.position;
        attachmentPoint1.y += player1.GetComponent<PlayerModelController>().PlayerHeight * _playerMidpointRate;

        Vector3 attachmentPoint2 = player2.transform.position;
        attachmentPoint2.y += player2.GetComponent<PlayerModelController>().PlayerHeight * _playerMidpointRate;

        // Convert the second attachment point to the local space of player1
        Vector3 localHit = player1.transform.InverseTransformPoint(attachmentPoint2);

        int filter =
            ObiUtils.MakeFilter(ObiUtils.CollideWithEverything & ~(1 << _playerCategory) & ~(1 << _chainCategory),
                _chainCategory);

        // Configure the blueprint path with two control points
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(Vector3.zero, -localHit.normalized, localHit.normalized, Vector3.up,
            _controlPointMass, 0.1f,
            1, filter, Color.white, "Player1 Attachment");
        blueprint.path.AddControlPoint(localHit, -localHit.normalized, localHit.normalized, Vector3.up,
            _controlPointMass, 0.1f, 1,
            filter, Color.white, "Player2 Attachment");
        blueprint.path.FlushEvents();

        // Generate the rope's particle representation
        yield return blueprint.Generate();

        // Assign the blueprint to the rope
        rope.ropeBlueprint = blueprint;

        yield return new WaitForFixedUpdate();
        yield return null;

        var pinConstraints =
            rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();

        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(rope.solverIndices[0],
            player1.GetComponent<ObiColliderBase>(),
            player1.transform.InverseTransformPoint(attachmentPoint1), Quaternion.identity, 0, 0,
            float.PositiveInfinity);

        batch.AddConstraint(rope.solverIndices[blueprint.activeParticleCount - 1],
            player2.GetComponent<ObiColliderBase>(),
            player2.transform.InverseTransformPoint(attachmentPoint2), Quaternion.identity, 0, 0,
            float.PositiveInfinity);

        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);

        rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
        
        if (!photonView.IsMine)
        {
            ObiSolver obiSolver = solver.GetComponent<ObiSolver>();
            
            obiSolver.distanceConstraintParameters.enabled = false;
            obiSolver.bendingConstraintParameters.enabled = false;
            obiSolver.particleCollisionConstraintParameters.enabled = false;
            obiSolver.collisionConstraintParameters.enabled = false;
            obiSolver.pinConstraintParameters.enabled = false;
        }
        else
        {
            Debug.Log("Help2");
        }
    }
}