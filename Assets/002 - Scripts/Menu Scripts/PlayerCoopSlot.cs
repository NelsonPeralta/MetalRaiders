using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerCoopSlot : MonoBehaviour
{
    [Header("Scripts")]
    public MainMenu mainMenu;
    public Player player;
    public Text joinButtonText;

    [Header("Swarm")]
    public LocalMpMenu swarmMenu;

    [Header("Local PVP")]
    public LocalMpMenu localMpMenu;

    public int playerRewiredID;
    public bool hasJoined = false;

    private void Start()
    {
        player = ReInput.players.GetPlayer(playerRewiredID);
        if (playerRewiredID == 0)
        {
            hasJoined = true;
            joinButtonText.text = "Player 1 Joined";
        }
    }

    private void Join()
    {
        if (player.GetButtonDown("Switch Grenades") || player.GetButtonDown("Interact"))
        {
            if (!hasJoined)
            {
                StaticVariables.numberOfPlayers++;

                if (playerRewiredID != 0)
                {
                    joinButtonText.text = "Player " + (playerRewiredID + 1).ToString() + " Joined";
                    hasJoined = true;
                }
            }
        }
    }

    public void UnJoin()
    {
        if (player.GetButtonDown("Crouch") || player.GetButtonDown("Escape"))
        {
            if (hasJoined)
            {
                if (playerRewiredID != 0)
                {
                    StaticVariables.numberOfPlayers--;
                    joinButtonText.text = "Press INTERACT Join";
                    hasJoined = false;
                    Debug.Log($"NUMBER OF PLAYERS: {StaticVariables.numberOfPlayers}");
                }

                if(mainMenu.swarmMenuOpen)
                {
                    if (!mainMenu.swarmMenuScript.mapChooserOpen)
                        mainMenu.openAndCloseSwarmMenu();
                    else if (mainMenu.swarmMenuScript.mapChooserOpen)
                        mainMenu.swarmMenuScript.OpenCloseMapChooser();
                }

                if(mainMenu.LocalMpMenuOpen)
                {
                    if(!mainMenu.LocalMpScript.gametypeMenuOpen && !mainMenu.LocalMpScript.mapChooserOpen)
                    {
                        mainMenu.OpenAndCloseLocalMPMenu();
                    }
                    else if(mainMenu.LocalMpScript.gametypeMenuOpen)
                    {
                        mainMenu.LocalMpScript.OpenCloseGametypeChooser();
                    }
                    else if(mainMenu.LocalMpScript.mapChooserOpen)
                    {
                        mainMenu.LocalMpScript.OpenCloseMapChooser();
                    }
                }
            }
        }
        /*
        if (player.GetButtonDown("Crouch") && hasJoined && !coopMenu.mapChooserOpen)
        {
            else if (playerRewiredID == 0 && multiplayerMenu != null)
            {
                multiplayerMenu.player0Joined = false;
            }

            PlayerCounter.numberOfPlayers = PlayerCounter.numberOfPlayers - 1;
            joinButtonText.text = "Press INTERACT Join";
            hasJoined = false;

            if (!coopMenu.player0Joined)
            {
                coopMenu.pressStartWhenReadyInformer.SetActive(false);
            }
        }
        else if (player.GetButtonDown("Crouch") && hasJoined && coopMenu.mapChooserOpen)
        {
            if (!coopMenu.mapChooser.GetComponent<MapChooser>().countdownInProgress)
            {
                coopMenu.mapChooser.GetComponent<MapChooser>().mapChosenWitness.text = "Map: ";
                coopMenu.mapChooser.GetComponent<MapChooser>().countdownText.text = "Countdown: ";
                coopMenu.closeMapChooser();
            }
            else
            {
                coopMenu.mapChooser.GetComponent<MapChooser>().countdownCanceled = true;
            }
        }
        else if (player.GetButtonDown("Crouch") && !hasJoined)
        {
            mainMenu.openAndCloseSwarmMenu();
        }*/
    }

    public void ChooseLevel()
    {
        /*
        if (multiplayerMenu != null)
        {
            if (player.GetButtonDown("Start") && multiplayerMenu.player0Joined && playerRewiredID == 0)
            {
                SceneManager.LoadScene("PVP - 001");
            }
        }
        else if (coopMenu != null)
        {
            if (player.GetButtonDown("Start") && coopMenu.player0Joined && playerRewiredID == 0 && !coopMenu.mapChooserOpen)
            {
                coopMenu.openMapChooser();
            }

            if (player.GetButtonDown("Start") && coopMenu.player0Joined && playerRewiredID == 0 && coopMenu.mapChooserOpen)
            {
                if (coopMenu.mapChooser.GetComponent<MapChooser>().mapChosen)
                {
                    coopMenu.mapChooser.GetComponent<MapChooser>().countdownInProgress = true;
                    coopMenu.mapChooser.GetComponent<MapChooser>().countdownCanceled = false;
                    coopMenu.mapChooser.GetComponent<MapChooser>().countdown = coopMenu.mapChooser.GetComponent<MapChooser>().defaultCountdown;
                }
            }
        }*/
    }

    public void Update()
    {
        Join();
        UnJoin();
        ChooseLevel();
    }
}
