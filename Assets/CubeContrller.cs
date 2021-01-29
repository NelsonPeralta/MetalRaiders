using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Rewired;

public class CubeContrller : MonoBehaviour
{
    public int playerID = 0;
    Player player;

    private void Start()
    {
        player = ReInput.players.GetPlayer(playerID);
    }

    private void FixedUpdate()
    {
        if(player.GetButtonDown("Jump"))
        {
            Debug.Log("Jumped" + playerID);
            transform.Translate(transform.up);
        }
    }

}
