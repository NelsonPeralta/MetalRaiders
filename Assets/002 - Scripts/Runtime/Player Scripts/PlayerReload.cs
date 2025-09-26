using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.EventSystems;
using ExitGames.Client.Photon.StructWrapping;

public class PlayerReload : MonoBehaviour
{
    ControllerType controllerType { get { return playerController.activeControllerType; } }
    PhotonView PV { get { return player.PV; } }
    Rewired.Player rewiredPlayer { get { return playerController.rewiredPlayer; } }

    [SerializeField] Player player;
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerInventory playerInventory;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!player.isDead && !player.isRespawning && !playerController.isSprinting && !playerController.pauseMenuOpen)
            if (GameManager.instance.gameStarted)
                ManualReload();

        if (player.PV.IsMine)
        {

            if (!GameManager.instance.gameStarted)
                return;

            if (!GetComponent<Player>().isDead && !GetComponent<Player>().isRespawning)
            {

            }
        }
    }

    void ManualReload()
    {
        if (!GetComponent<Player>().isDead)
        {
            if (PV.IsMine && rewiredPlayer.GetButtonDown("Reload") && !playerController.isReloading && !playerController.isDualWielding)
            {
                Log.Print(() =>"Maual Reload");
                //PV.RPC("CheckRealodButton_RPC", RpcTarget.All);
            }
        }
    }
}
