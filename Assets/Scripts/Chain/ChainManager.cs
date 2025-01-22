using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Obi;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ChainManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private GameObject _solverPrefab;
    [SerializeField] private GameObject _chainPrefab;

    [Header("Chain Config")] [SerializeField]
    private float _playerMidpointRate = 0.35f;

    [SerializeField] private float _maxBending = 0.02f;
    [SerializeField] private float _chainLength = 5f;


    [Range(0, 15)] [SerializeField] private int _playerCategory = 5;
    [Range(0, 15)] [SerializeField] private int _chainCategory = 10;
    [SerializeField] private float _controlPointMass = 0.01f;


    [SerializeField]
    private Dictionary<(Player, Player), GameObject> _chains = new Dictionary<(Player, Player), GameObject>();

    [SerializeField]
    private Dictionary<GameObject, (Player, Player)> _chainsInv = new Dictionary<GameObject, (Player, Player)>();


    public GameObject CreateSolver(GameObject player1, GameObject player2)
    {
        GameObject solverObj = Instantiate(_solverPrefab);

        StartCoroutine(CreateChainBluePrint(solverObj, player1, player2));

        return solverObj;
    }

    private IEnumerator CreateChainBluePrint(GameObject solverObj, GameObject player1, GameObject player2)
    {
        GameObject chain = Instantiate(_chainPrefab, solverObj.transform);
        ObiRope rope = chain.GetComponent<ObiRope>();
        rope.maxBending = _maxBending;

        yield return 0;

        // Create a new blueprint for the rope
        ObiRopeBlueprint blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 1.0f;

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
        blueprint.path.AddControlPoint(attachmentPoint1, -localHit.normalized, localHit.normalized, Vector3.up,
            _controlPointMass, 0.1f, 1, filter, Color.white, "Player1 Attachment");
        blueprint.path.AddControlPoint(attachmentPoint2, -localHit.normalized, localHit.normalized, Vector3.up,
            _controlPointMass, 0.1f, 1, filter, Color.white, "Player2 Attachment");
        blueprint.path.FlushEvents();

        var attachment1 = rope.AddComponent<ObiParticleAttachment>();
        attachment1.target = player1.transform;
        attachment1.particleGroup = blueprint.groups[0];

        var attachment2 = rope.AddComponent<ObiParticleAttachment>();
        attachment2.target = player2.transform;
        attachment2.particleGroup = blueprint.groups[1];

        ObiRopeCursor cursor = rope.GetComponent<ObiRopeCursor>();
        cursor.ChangeLength(-rope.restLength);
        cursor.ChangeLength(_chainLength);

        if (photonView.IsMine)
        {
            attachment1.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
            attachment2.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
        }
        else
        {
            attachment1.attachmentType = ObiParticleAttachment.AttachmentType.Static;
            attachment2.attachmentType = ObiParticleAttachment.AttachmentType.Static;
        }

        // Generate the rope's particle representation
        yield return blueprint.Generate();

        // Assign the blueprint to the rope
        rope.ropeBlueprint = blueprint;

        // yield return new WaitForFixedUpdate();
        // yield return null;
    }

    private GameObject FindPlayerById(int actorNumber)
    {
        foreach (var obj in FindObjectsOfType<PlayerSetup>())
        {
            if (obj.GetComponent<PhotonView>().Owner.ActorNumber == actorNumber)
                return obj.gameObject;
        }

        return null;
    }

    private void CreateChain(Player player1, Player player2)
    {
        GameObject player1Obj = FindPlayerById(player1.ActorNumber);
        GameObject player2Obj = FindPlayerById(player2.ActorNumber);
        if (player1Obj != null && player2Obj != null)
        {
            Debug.Log("Creating chain between player " + player1.ActorNumber + " and " + player2.ActorNumber);
            GameObject chain = CreateSolver(player1Obj, player2Obj);

            _chains.Add((player1, player2), chain);
            _chainsInv.Add(chain, (player1, player2));
        }
    }

    private void DestroyChain(Player player1, Player player2)
    {
        Debug.Log("Destroying chain between player " + player1.ActorNumber + " and " + player2.ActorNumber);
        Destroy(_chains[(player1, player2)]);
        _chainsInv.Remove(_chains[(player1, player2)]);
        _chains.Remove((player1, player2));
    }

    public void UpdateChainPlayerJoin(Player player)
    {
        Debug.Log("Network: UpdateChainPlayerJoined " + player.ActorNumber);

        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length - 1; i++)
            {
                Player player1 = PhotonNetwork.PlayerList[i];
                Player player2 = PhotonNetwork.PlayerList[i + 1];

                if (!_chains.ContainsKey((player1, player2)))
                {
                    CreateChain(player1, player2);
                }
            }
        }
    }

    public void UpdateChainForPlayerLeave(Player player)
    {
        Debug.Log("Network: UpdateChainForPlayerLeave " + player.ActorNumber);

        Player previousPlayer = null;
        Player nextPlayer = null;

        foreach (var playerPair in _chains.Keys)
        {
            if (Equals(playerPair.Item1, player))
            {
                nextPlayer = playerPair.Item2;
            }

            if (Equals(playerPair.Item2, player))
            {
                previousPlayer = playerPair.Item1;
            }
        }

        if (_chains.ContainsKey((player, nextPlayer)))
        {
            DestroyChain(player, nextPlayer);
        }

        if (_chains.ContainsKey((previousPlayer, player)))
        {
            DestroyChain(previousPlayer, player);
        }

        if (previousPlayer != null && nextPlayer != null)
        {
            CreateChain(previousPlayer, nextPlayer);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_chainsInv.Count);
            foreach (var item in _chainsInv)
            {
                var (player1, player2) = item.Value;
                GameObject player1Obj = FindPlayerById(player1.ActorNumber);
                GameObject player2Obj = FindPlayerById(player2.ActorNumber);
                stream.SendNext(player1.ActorNumber);
                stream.SendNext(player2.ActorNumber);
                stream.SendNext(player1Obj.transform.position);
                stream.SendNext(player2Obj.transform.position);
                stream.SendNext(player1Obj.transform.rotation);
                stream.SendNext(player2Obj.transform.rotation);
            }
        }
        else
        {
            int count = (int)stream.ReceiveNext();
            for (int i = 0; i < count; i++)
            {
                int actorNumber1 = (int)stream.ReceiveNext();
                int actorNumber2 = (int)stream.ReceiveNext();
                Vector3 pos1 = (Vector3)stream.ReceiveNext();
                Vector3 pos2 = (Vector3)stream.ReceiveNext();
                Quaternion rot1 = (Quaternion)stream.ReceiveNext();
                Quaternion rot2 = (Quaternion)stream.ReceiveNext();

                GameObject player1Obj = FindPlayerById(actorNumber1);
                GameObject player2Obj = FindPlayerById(actorNumber2);

                var player1Movement = player1Obj.GetComponent<PlayerMovementController>();
                var player2Movement = player2Obj.GetComponent<PlayerMovementController>();
                
                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                player1Movement.SetChainPosition(pos1, rot1, lag);
                player2Movement.SetChainPosition(pos2, rot2, lag);
            }
        }
    }
}