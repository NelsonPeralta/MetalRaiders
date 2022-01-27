using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotionTracker : MonoBehaviour
{
    public Player playerProperties;
    public Camera minimapCamera;
    public GameObject friendlyDot;
    public GameObject ennemyDot;
    private void Awake()
    {
        if (playerProperties.PV.IsMine)
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

        playerProperties.GetComponent<Movement>().OnPlayerStartedMoving += OnPlayerStartedMoving_Delegate;
        playerProperties.GetComponent<Movement>().OnPlayerStoppedMoving += OnPlayerStoppedMoving_Delegate;
    }

    void OnPlayerStartedMoving_Delegate(Movement movement)
    {
        friendlyDot.SetActive(true);
    }

    void OnPlayerStoppedMoving_Delegate(Movement movement)
    {
        friendlyDot.SetActive(false);
    }
}
