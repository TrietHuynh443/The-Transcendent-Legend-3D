using System.Collections;
using System.Collections.Generic;
using Obi;
using Photon.Pun;
using UnityEngine;

public class ChainManager : MonoBehaviourPunCallbacks
{
    private Dictionary<GameObject, GameObject> _chains = new Dictionary<GameObject, GameObject>();
    private GameObject _solverObject;
    [SerializeField] private GameObject _chainPrefab;

    void Awake()
    {
        CreateSolver();
    }

    void CreateSolver()
    {
        _solverObject = new GameObject("Obi Solver", typeof(ObiSolver));
        // ObiSolver solver = solverObject.GetComponent<ObiSolver>();
    }

    public void CreateChain(GameObject player1, GameObject player2)
    {
        GameObject chain = Instantiate(_chainPrefab, _solverObject.transform);
        _chains.Add(chain, player1);
        ObiRope rope = chain.GetComponent<ObiRope>();

        // Tweak rope parameters:
        rope.maxBending = 0.02f;

        StartCoroutine(CreateChainBluePrint(rope, player1, player2));
    }

    private IEnumerator CreateChainBluePrint(ObiRope rope, GameObject player1, GameObject player2)
    {
        yield return 0;

        // Create a new blueprint for the rope
        ObiRopeBlueprint blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.5f;

        // Calculate the desired attachment points (midpoints of players)
        Vector3 attachmentPoint1 = player1.transform.position;
        attachmentPoint1.y += player1.GetComponent<PlayerModelController>().PlayerHeight / 2;

        Vector3 attachmentPoint2 = player2.transform.position;
        attachmentPoint2.y += player2.GetComponent<PlayerModelController>().PlayerHeight / 2;

        // Convert the second attachment point to the local space of player1
        Vector3 localHit = player1.transform.InverseTransformPoint(attachmentPoint2);

        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything & ~(1 << 5), 10);

        // Configure the blueprint path with two control points
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(Vector3.zero, -localHit.normalized, localHit.normalized, Vector3.up, 0.1f, 0.1f,
            1, filter, Color.white, "Player1 Attachment");
        blueprint.path.AddControlPoint(localHit, -localHit.normalized, localHit.normalized, Vector3.up, 0.1f, 0.1f, 1,
            filter, Color.white, "Player2 Attachment");
        blueprint.path.FlushEvents();

        // Generate the rope's particle representation
        yield return blueprint.Generate();

        // Assign the blueprint to the rope
        rope.ropeBlueprint = blueprint;

        yield return new WaitForFixedUpdate();
        yield return null;

        // Pin both ends of the rope to the players
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
    }
}