using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMinimap : MonoBehaviour
{
    public PlayerProperties playerProperties;
    public GameObject friendlyDot;
    public GameObject ennemyDot;
    private void Awake()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        {
            if (playerProperties.PV.IsMine)
            {
                friendlyDot.SetActive(true);
                ennemyDot.SetActive(false);
            }
            else
            {
                friendlyDot.SetActive(false);
                ennemyDot.SetActive(true);
            }
        }
    }
}
