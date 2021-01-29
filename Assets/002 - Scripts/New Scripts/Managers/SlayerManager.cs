using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlayerManager : MonoBehaviour
{
    public GameSettings gameSettings;
    public bool useThisGameMode;

    [Header("Players")]
    public GameObject[] players;

    [Header("Goal")]
    public int killsToWin = 25;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gameOver;

    private void Start()
    {
        if (GameObject.Find("Game Settings") != null)
            gameSettings = GameObject.FindGameObjectWithTag("Game Settings").gameObject.GetComponent<GameSettings>();
        if (gameSettings && gameSettings.loadSlayer)
            GivePlayersThis();
        ConfigureUIPoints();
    }

    public void UpdatePoints(int playerWhoDied, int playerWhoKilled)
    {
        int secondPlaceKills = 0;
        Debug.Log("SLAYER MANAGER; Player Who Died ID: " + playerWhoDied + "; Player Who Killed ID: " + playerWhoKilled);

        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerProperties>().playerRewiredID == playerWhoKilled)
                player.GetComponent<AllPlayerScripts>().playerMPProperties.AddKill(true);

            if (player.GetComponent<AllPlayerScripts>().playerMPProperties.kills > secondPlaceKills)
                secondPlaceKills = player.GetComponent<AllPlayerScripts>().playerMPProperties.kills;

            if (player.GetComponent<PlayerProperties>().playerRewiredID != playerWhoKilled)
                player.GetComponent<AllPlayerScripts>().playerUIComponents.multiplayerPointsBlue.text = secondPlaceKills.ToString();

            if (player.GetComponent<AllPlayerScripts>().playerMPProperties.kills >= killsToWin)
                EndGame();
        }
    }

    void EndGame()
    {
        audioSource.clip = gameOver;
        audioSource.Play();
    }
    void GivePlayersThis()
    {
        if (players.Length > 0)
        {
            foreach (GameObject player in players)
            {
                if (player)
                {
                    player.GetComponent<AllPlayerScripts>().playerMPProperties.slayerManager = this;
                }
            }
        }
    }
    void ConfigureUIPoints()
    {
        if (players.Length > 0)
        {
            foreach (GameObject player in players)
            {
                if (player)
                {
                    player.GetComponent<AllPlayerScripts>().playerUIComponents.swarmPoints.SetActive(false);
                    player.GetComponent<AllPlayerScripts>().playerUIComponents.multiplayerPoints.SetActive(true);
                    player.GetComponent<AllPlayerScripts>().playerUIComponents.multiplayerPointsBlue.text = 0.ToString();
                    player.GetComponent<AllPlayerScripts>().playerUIComponents.multiplayerPointsRed.text = 0.ToString();
                }
            }
        }
    }
}
