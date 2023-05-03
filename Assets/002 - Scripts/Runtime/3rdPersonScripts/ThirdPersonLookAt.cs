using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ThirdPersonLookAt : MonoBehaviour
{
    public Animator anim;
    public Transform lookAtGO;
    public Transform chest;
    public Movement movement;
    //public PhotonView photonView;

    [Header("Offsets")]
    public Vector3 currentOffset;
    public Vector3 iddleOffset;
    [Space(15)]
    public Vector3 LeftOffset;
    public Vector3 LeftForwardOffset;
    public Vector3 ForwardOffset;
    public Vector3 RightForwardOffest;
    public Vector3 RightOffset;
    public Vector3 RightBackwardsOffset;
    public Vector3 BackwardsOffset;
    public Vector3 LeftBackwardsOffset;

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    //Debug.Log("In OPSV");
    //    anim = GetComponent<Animator>();
    //    chest = anim.GetBoneTransform(HumanBodyBones.Chest);
    //    if (stream.IsWriting) // If I am the owner
    //    {
    //        //Debug.Log("Writing: " + lookAtGO.position);
    //        stream.SendNext(lookAtGO.position);
    //    }
    //    else if (stream.IsReading) // If I am a client
    //    {
    //        //Debug.Log("In Reading: " + stream.ReceiveNext());
    //        //if (chest)
    //        //{
    //        //    Debug.Log("There is a chest");
    //        //    chest.LookAt((Vector3)stream.ReceiveNext());
    //        //}
    //        //else
    //        //{
    //        //    Debug.Log("There is no chest");
    //        //}
    //    }
    //}

    private void Awake()
    {
        anim = GetComponent<Animator>();
        //photonView = GetComponent<PhotonView>();
        if (!chest)
            chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        currentOffset = iddleOffset;
    }

    private void LateUpdate()
    {
        if (movement.GetComponent<PlayerController>().isSprinting)
            return;
        UpdateOffset(movement.movementDirection);
        PlayerChestRotation();
        //photonView.RPC("UpdateOffset", RpcTarget.All, movement.directionIndicator);
        //photonView.RPC("PlayerChestRotation", RpcTarget.All);
    }

    void PlayerChestRotation()
    {
        //Debug.Log(lookAtGO.position);
        chest.LookAt(lookAtGO.position);
        chest.rotation = chest.rotation * Quaternion.Euler(currentOffset);
    }

    void UpdateOffset(Movement.PlayerMovementDirection pmd)
    {
        if (pmd == Movement.PlayerMovementDirection.Idle) // Idle
            currentOffset = iddleOffset;
        else if (pmd == Movement.PlayerMovementDirection.Left) // Left
            currentOffset = LeftOffset;
        else if (pmd == Movement.PlayerMovementDirection.ForwardLeft) // Left Forward
            currentOffset = LeftForwardOffset;
        else if (pmd == Movement.PlayerMovementDirection.Forward) // Forward
            currentOffset = ForwardOffset;
        else if (pmd == Movement.PlayerMovementDirection.ForwardRight)// Right Forward
            currentOffset = RightForwardOffest;
        else if (pmd == Movement.PlayerMovementDirection.Right)// Right
            currentOffset = RightOffset;
        else if (pmd == Movement.PlayerMovementDirection.BackwardsRight)// Right Backwards
            currentOffset = RightBackwardsOffset;
        else if (pmd == Movement.PlayerMovementDirection.Backwards)// Backwards
            currentOffset = BackwardsOffset;
        else if (pmd == Movement.PlayerMovementDirection.BackwardsLeft)// Left Backwards
            currentOffset = LeftBackwardsOffset;
    }
}
