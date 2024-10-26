using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ThirdPersonLookAt : MonoBehaviour
{
    public Animator anim;
    public Transform lookAtGO;
    public Transform chest;
    public PlayerMovement movement;
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




    [SerializeField] PlayerController _playerController;
    [SerializeField] PlayerMovement _movement;

    Vector3 _crouchRotationFix = new Vector3(0, 30, 0);




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

    private void Update()
    {
        try
        {
            //if (_playerController.isCrouching)
            //    transform.localRotation = Quaternion.Euler(_crouchRotationFix);
            //else
            //    transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        catch { }
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
        if (_playerController.player.isAlive && !_playerController.cameraIsFloating)
        {
            chest.LookAt(lookAtGO.position);
            chest.rotation = chest.rotation * Quaternion.Euler(currentOffset);
        }
    }

    void UpdateOffset(PlayerMovement.PlayerMovementDirection pmd)
    {
        if (pmd == PlayerMovement.PlayerMovementDirection.Idle) // Idle
            currentOffset = iddleOffset;
        else if (pmd == PlayerMovement.PlayerMovementDirection.Left) // Left
            currentOffset = LeftOffset;
        else if (pmd == PlayerMovement.PlayerMovementDirection.ForwardLeft) // Left Forward
            currentOffset = LeftForwardOffset;
        else if (pmd == PlayerMovement.PlayerMovementDirection.Forward) // Forward
            currentOffset = ForwardOffset;
        else if (pmd == PlayerMovement.PlayerMovementDirection.ForwardRight)// Right Forward
            currentOffset = RightForwardOffest;
        else if (pmd == PlayerMovement.PlayerMovementDirection.Right)// Right
            currentOffset = RightOffset;
        else if (pmd == PlayerMovement.PlayerMovementDirection.BackwardsRight)// Right Backwards
            currentOffset = RightBackwardsOffset;
        else if (pmd == PlayerMovement.PlayerMovementDirection.Backwards)// Backwards
            currentOffset = BackwardsOffset;
        else if (pmd == PlayerMovement.PlayerMovementDirection.BackwardsLeft)// Left Backwards
            currentOffset = LeftBackwardsOffset;
    }
}
