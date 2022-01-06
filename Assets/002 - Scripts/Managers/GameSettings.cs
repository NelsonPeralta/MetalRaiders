using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    [Header("Managers")]
    public LocalMpMenu swarmMenu;
    public LocalMpMenu localMpMenu;

    [Header("Game Modes")]
    public bool loadSwarm;
    public bool loadMultiplayer;    

    [Header("Swarm Maps")]
    public bool loadDownpoor;
    public bool loadTumbleweed;

    [Header("Multiplayer Game Types")]
    public bool loadSlayer;
    public bool loadTeamSlayer;

    [Header("Multiplayer Maps")]    
    public bool loadPitchfork;
    public bool loadTestingRoom;
    public bool test;
    
    private void Awake()
    {
            DontDestroyOnLoad(this.gameObject);
    }

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
        if (!Input.GetMouseButtonUp(0))
        {
            Debug.Log("Clicked UP from GameSettings");

            if (!loadSlayer)
            {
                disableAllGamemodes();
                loadSlayer = true;
                localMpMenu.updateGametypeWitness("Slayer");
                checkIfReady();
                Debug.Log("Enalbe Salyer");
            }
            else
            {
                Debug.Log("Disable Salyer");
                disableAllGamemodes();
            }
        }
    }

    public void toggleTeamSlayer()
    {
        if (!Input.GetMouseButtonUp(0))
        {
            if (!loadTeamSlayer)
            {
                disableAllGamemodes();
                loadTeamSlayer = true;
                checkIfReady();
                Debug.Log("Enalbe TEAM Slayer");
            }
            else
            {
                disableAllGamemodes();
            }
        }
    }

    public void toggleDownpoor()
    {
        if (!loadDownpoor)
        {
            disableAllMaps();
            loadDownpoor = true;
            updateMapWitnesses("Downpoor");
            checkIfReady();
        }
        else
            disableAllMaps();
    }

    public void toggleTumbleweed()
    {
        if (!loadTumbleweed)
        {
            disableAllMaps();
            loadTumbleweed = true;
            updateMapWitnesses("Tumbleweed");
            checkIfReady();
        }
        else
            disableAllMaps();
    }

    public void togglePitchfork()
    {
        if (!loadPitchfork)
        {
            disableAllMaps();
            loadPitchfork = true;
            updateMapWitnesses("Pitchfork");
            checkIfReady();
        }
        else
            disableAllMaps();
    }

    public void toggleTestingRoom()
    {
        if (!loadPitchfork)
        {
            disableAllMaps();
            loadTestingRoom = true;
            updateMapWitnesses("Testing Room");
            checkIfReady();
        }
        else
            disableAllMaps();
    }

    public void startGame()
    {
        if (loadSwarm)
        {

        }
        else if (loadMultiplayer)
        {
            if (loadPitchfork)
                SceneManager.LoadScene("PVP - 001 - Pitchfork");
        }

    }

    public void disableAllMaps()
    {
        loadDownpoor = false;
        loadTumbleweed = false;

        loadPitchfork = false;
    }

    public void disableAllGametypes()
    {
        loadSlayer = false;
        loadTeamSlayer = false;
    }

    public void disableAllGamemodes()
    {
        loadSwarm = false;
        loadMultiplayer = false;
    }

    void updateMapWitnesses(string mapName)
    {
        swarmMenu.updateMapWitness(mapName);
        localMpMenu.updateMapWitness(mapName);
    }

    void enableCountdowns()
    {
        localMpMenu.enableCountdownButton();
        swarmMenu.enableCountdownButton();
    }

    void disableCountdowns()
    {
        localMpMenu.disableCountdownButton();
        swarmMenu.disableCountdownButton();
    }

    void checkIfReady()
    {
        if (loadSlayer || loadTeamSlayer)
            if (loadPitchfork)
                enableCountdowns();
            else if (loadTestingRoom)
                enableCountdowns();
            else
                disableCountdowns();
        if (loadSwarm)
            if (loadDownpoor)
                enableCountdowns();
            else if (loadTumbleweed)
                enableCountdowns();
            else
                disableCountdowns();

    }

    public void StartGame()
    {
        // Swarm Maps
        if (loadDownpoor)
            SceneManager.LoadScene("Coop - 001 - Downpour");
        else if(loadTumbleweed)
            SceneManager.LoadScene("Coop - 002 - Tumbleweed");

        // Multiplayer Maps
        if (loadPitchfork)
                SceneManager.LoadScene("PVP - 001 - Pitchfork");
        else if (loadTestingRoom)
            SceneManager.LoadScene("Testing Room");
    }
}
