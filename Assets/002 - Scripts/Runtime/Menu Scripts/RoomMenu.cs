using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;
using static SwarmManager;

public class RoomMenu : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("ONENABLE RoomMenu SCRIPT");
        if (GameManager.instance.gameMode == GameMode.Swarm)
        {
            FindObjectOfType<Launcher>().multiplayerMcComponentsHolder.SetActive(false);
            FindObjectOfType<Launcher>().swarmMcComponentsHolder.SetActive(PhotonNetwork.IsMasterClient);
            FindObjectOfType<Launcher>().levelToLoadIndex = 11;
        }
        else if (GameManager.instance.gameMode == GameMode.Multiplayer)
        {
            if (CurrentRoomManager.instance.roomType != CurrentRoomManager.RoomType.QuickMatch)
                FindObjectOfType<Launcher>().multiplayerMcComponentsHolder.SetActive(PhotonNetwork.IsMasterClient);
            else
                FindObjectOfType<Launcher>().multiplayerMcComponentsHolder.SetActive(false);

            FindObjectOfType<Launcher>().swarmMcComponentsHolder.SetActive(false);
            FindObjectOfType<Launcher>().levelToLoadIndex = 1;
        }
        Launcher.instance.gameModeText.text = $"Game Mode: {GameManager.instance.gameMode}";

        if (CurrentRoomManager.instance.roomType != CurrentRoomManager.RoomType.QuickMatch)
            FindObjectOfType<Launcher>().gameModeBtns.SetActive(PhotonNetwork.IsMasterClient);
        else
        {
            Launcher.instance.multiplayerMcComponentsHolder.SetActive(false);
            FindObjectOfType<Launcher>().gameModeBtns.SetActive(false);
            FindObjectOfType<Launcher>().swarmModeBtns.SetActive(false);
            FindObjectOfType<Launcher>().swarmMcComponentsHolder.SetActive(false);
        }
    }
}
