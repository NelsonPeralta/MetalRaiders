using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StaticVariables : MonoBehaviour
{
    public static int numberOfPlayers = 0;

    [Header("Gametypes")]
    public static bool loadSwarm;
    public static bool loadMultiplayer;
    public static bool loadSlayer;
    public static bool loadTeamSlayer;

    [Header("Swarm Maps")]
    public static bool loadDownpoor;
    public static bool loadTumbleweed;

    [Header("Multiplayer Maps")]
    public static bool loadPitchfork;


    public void toggleMultiplayer()
    {
        if (!loadMultiplayer)
            loadMultiplayer = true;
        else
            loadMultiplayer = false;
    }

    public void toggleSwarm()
    {
        if (!loadSwarm)
            loadSwarm = true;
        else
            loadSwarm = false;
    }

    public void toggleSlayer()
    {
        if (!loadSlayer)
        {
            loadSwarm = false;
            loadTeamSlayer = false;
            loadSlayer = true;
        }
        else
        {
            loadSlayer = false;
        }
    }

    public void toggleTeamSlayer()
    {
        if (!loadTeamSlayer)
            loadTeamSlayer = true;
        else
            loadTeamSlayer = false;
    }

    public void toggleDownpoor()
    {
        if (!loadDownpoor)
            loadDownpoor = true;
        else
            loadDownpoor = false;
    }

    public void toggleTumbleweed()
    {
        if (!loadTumbleweed)
            loadTumbleweed = true;
        else
            loadTumbleweed = false;
    }

    public void togglePitchfork()
    {
        if (!loadPitchfork)
            loadPitchfork = true;
        else
            loadPitchfork = false;

        Log.Print(() =>"STATIC VARIABLES: Toggled PITCHFORK: " + loadPitchfork);
    }

    public void startGame()
    {
        if(loadSwarm)
        {

        }
        else if(loadMultiplayer)
        {
            if(loadPitchfork)
                SceneManager.LoadScene("PVP - 001 - Pitchfork");
        }
        
    }
}
