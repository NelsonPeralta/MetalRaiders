using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitCurrentGameButton : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] GameObject button;

    public void QuitCurrentGame()
    {
        if (!CurrentRoomManager.instance.gameOver)
        {
            CurrentRoomManager.instance.leftRoomManually = true;
            button.SetActive(false);
            playerController.QuitMatch();
        }
    }
}
