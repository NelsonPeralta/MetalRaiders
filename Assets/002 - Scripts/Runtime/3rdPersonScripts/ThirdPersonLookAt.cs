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
    //    //Log.Print(() =>"In OPSV");
    //    anim = GetComponent<Animator>();
    //    chest = anim.GetBoneTransform(HumanBodyBones.Chest);
    //    if (stream.IsWriting) // If I am the owner
    //    {
    //        //Log.Print(() =>"Writing: " + lookAtGO.position);
    //        stream.SendNext(lookAtGO.position);
    //    }
    //    else if (stream.IsReading) // If I am a client
    //    {
    //        //Log.Print(() =>"In Reading: " + stream.ReceiveNext());
    //        //if (chest)
    //        //{
    //        //    Log.Print(() =>"There is a chest");
    //        //    chest.LookAt((Vector3)stream.ReceiveNext());
    //        //}
    //        //else
    //        //{
    //        //    Log.Print(() =>"There is no chest");
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

        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On)
        {
            MoveLookAtTargetToThirdPersonPosition();
        }
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
        //if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.Off)
        {
            if (_playerController.player.isAlive && !_playerController.cameraIsFloating)
            {
                chest.LookAt(lookAtGO.position);
                chest.rotation = chest.rotation * Quaternion.Euler(currentOffset);
            }
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


    public void MoveLookAtTargetToThirdPersonPosition()
    {
        Log.Print(() => "MoveLookAtTargetToThirdPersonPosition");
        lookAtGO.localPosition = new Vector3(lookAtGO.localPosition.x,
                lookAtGO.localPosition.y, lookAtGO.localPosition.z - (PlayerCamera.THIRD_PERSON_LOCAL_OFFSET.z * 2));
    }

    public void ReturnLookAtTargetToOriginalPosition()
    {
        lookAtGO.localPosition = new Vector3(lookAtGO.localPosition.x,
                lookAtGO.localPosition.y, lookAtGO.localPosition.z + (PlayerCamera.THIRD_PERSON_LOCAL_OFFSET.z * 2));
    }
}
