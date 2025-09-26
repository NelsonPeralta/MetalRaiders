using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class RagdollSpawn : MonoBehaviourPunCallbacks
{
    public GameObject ragdollPrefab;
    public GameObject ragdollSpawnPoint;
    public PhotonView PV;
    [Space(20)]
    public Transform Head;
    public Transform Chest;
    public Transform Hips;
    [Space(10)]
    public Transform UpperArmLeft;
    public Transform UpperArmRight;
    [Space(10)]
    public Transform LowerArmLeft;
    public Transform LowerArmRight;
    [Space(10)]
    public Transform UpperLegLeft;
    public Transform UpperLegRight;
    [Space(10)]
    public Transform LowerLegLeft;
    public Transform LowerLegRight;

    //public void StartRagdollSpawn()
    //{
    //    PV.RPC("SpawnRagdoll", RpcTarget.All);
    //}

    //[PunRPC]
    public void SpawnRagdoll()
    {
        //var ragdoll = Instantiate(ragdollPrefab, ragdollSpawnPoint.transform.position, ragdollSpawnPoint.transform.rotation);
        var ragdoll = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlinePlayerRagdoll"), Vector3.zero, Quaternion.identity);

        ragdoll.GetComponent<RagdollPrefab>().ragdollHead.position = Head.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollChest.position = Chest.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollHips.position = Hips.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollHead.rotation = Head.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollChest.rotation = Chest.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollHips.rotation = Hips.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmLeft.position = UpperArmLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmRight.position = UpperArmRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmLeft.rotation = UpperArmLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmRight.rotation = UpperArmRight.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmLeft.position = LowerArmLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmRight.position = LowerArmRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmLeft.rotation = LowerArmLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmRight.rotation = LowerArmRight.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegLeft.position = UpperLegLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegRight.position = UpperLegRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegLeft.rotation = UpperLegLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegRight.rotation = UpperLegRight.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegLeft.position = LowerLegLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegRight.position = LowerLegRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegLeft.rotation = LowerLegLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegRight.rotation = LowerLegRight.rotation;

        Object.Destroy(ragdoll, 15);

        Log.Print(() =>"Spawned Ragdoll");
    }
}
