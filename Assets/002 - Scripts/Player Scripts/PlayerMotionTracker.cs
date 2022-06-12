using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMotionTracker : MonoBehaviourPun
{
    public Player player;
    public Camera minimapCamera;
    public GameObject friendlyDot;
    public GameObject ennemyDot;
    private void Awake()
    {
        if (player.PV.IsMine)
        {
            friendlyDot.SetActive(true);
            ennemyDot.SetActive(false);
        }
        else
        {
            minimapCamera.enabled = false;
            friendlyDot.SetActive(false);
            ennemyDot.SetActive(true);
        }
    }

    private void Start()
    {
        friendlyDot.SetActive(false);

        player.GetComponent<Movement>().OnPlayerStartedMoving -= OnPlayerStartedMoving_Delegate;
        player.GetComponent<Movement>().OnPlayerStartedMoving += OnPlayerStartedMoving_Delegate;
        player.GetComponent<Movement>().OnPlayerStoppedMoving -= OnPlayerStoppedMoving_Delegate;
        player.GetComponent<Movement>().OnPlayerStoppedMoving += OnPlayerStoppedMoving_Delegate;

        player.GetComponent<PlayerController>().OnCrouchDown -= OnCrouchDown_Delegate;
        player.GetComponent<PlayerController>().OnCrouchDown += OnCrouchDown_Delegate;

        player.GetComponent<PlayerController>().OnCrouchUp -= OnCrouchUp_Delegate;
        player.GetComponent<PlayerController>().OnCrouchUp += OnCrouchUp_Delegate;
    }

    void OnPlayerStartedMoving_Delegate(Movement movement)
    {
        if (!movement.GetComponent<PlayerController>().isCrouching)
            if (GetComponent<PhotonView>().IsMine)
                GetComponent<PhotonView>().RPC("ShowDot_RPC", RpcTarget.All);
        //friendlyDot.SetActive(true);
    }

    void OnPlayerStoppedMoving_Delegate(Movement movement)
    {
        if (GetComponent<PhotonView>().IsMine)
            GetComponent<PhotonView>().RPC("HideDot_RPC", RpcTarget.All);
    }

    void OnCrouchUp_Delegate(PlayerController playerController)
    {
        if (player.GetComponent<Movement>().isMoving)
            if (GetComponent<PhotonView>().IsMine)
                GetComponent<PhotonView>().RPC("ShowDot_RPC", RpcTarget.All);
    }

    void OnCrouchDown_Delegate(PlayerController playerController)
    {
        if (GetComponent<PhotonView>().IsMine)
            GetComponent<PhotonView>().RPC("HideDot_RPC", RpcTarget.All);
    }

    [PunRPC]
    void ShowDot_RPC()
    {
        if (player.GetComponent<PhotonView>().IsMine)
            friendlyDot.SetActive(true);
        else
            ennemyDot.SetActive(true);
    }

    [PunRPC]
    void HideDot_RPC()
    {
        friendlyDot.SetActive(false);
        ennemyDot.SetActive(false);
    }
}
