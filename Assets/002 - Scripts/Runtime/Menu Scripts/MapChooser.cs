﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rewired;

public class MapChooser : MonoBehaviour
{
    [Header("Map Chooser Components")]
    public GameObject Selector;
    public GameObject[] buttonsPage1;
    public GameObject selectedButton;
    public int selectedButtonNumber;
    public int pageNumber;
    public bool mapChosen;
    public Text mapChosenWitness;
    public Text countdownText;
    public float defaultCountdown = 5;
    public float countdown;
    public bool countdownInProgress;
    public bool countdownCanceled;

    [Header("Map Image and Location")]
    public Image mapImage;
    public Text mapInfo;

    [Header("Players")]
    public Rewired.Player player;
    public int playerRID;

    [Header("Audio")]
    public AudioSource selectorAudioSource;
    public AudioClip positiveFeedback;
    public AudioClip negativeFeedback;

    Ray ray;
    RaycastHit hit;

    private void Start()
    {
        player = ReInput.players.GetPlayer(playerRID);

        if (buttonsPage1[0] != null)
        {
            selectedButton = buttonsPage1[0];
            updateMapImageAndInfo();
        }
    }

    private void Update()
    {
        selectButtomUP();
        selectButtonDown();
        clickButton();
        startCountdown();
        mouseScript();
    }

    void selectButtomUP()
    {
        float x = player.GetAxis("Move Vertical");

        if (x > 0)
        {
            if (selectedButton != buttonsPage1[0])
            {
                Selector.transform.localPosition = buttonsPage1[selectedButtonNumber - 1].transform.localPosition;
                selectedButton = buttonsPage1[selectedButtonNumber - 1];
                selectedButtonNumber = selectedButtonNumber - 1;

                selectorAudioSource.clip = positiveFeedback;
                selectorAudioSource.Play();
                updateMapImageAndInfo();
            }
        }
    }

    void selectButtonDown()
    {
        float x = player.GetAxis("Move Vertical");

        if (x < 0)
        {
            if (selectedButton != buttonsPage1[buttonsPage1.Length - 1])
            {
                Selector.transform.localPosition = buttonsPage1[selectedButtonNumber + 1].transform.localPosition;
                selectedButton = buttonsPage1[selectedButtonNumber + 1];
                selectedButtonNumber = selectedButtonNumber + 1;

                selectorAudioSource.clip = positiveFeedback;
                selectorAudioSource.Play();
                updateMapImageAndInfo();
            }
        }
    }

    void clickButton()
    {
        if (player.GetButtonDown("Switch Grenades") || player.GetButtonDown("Enter"))
        {
            if (selectedButton.GetComponent<Button>() != null)
            {
                selectedButton.GetComponent<Button>().onClick.Invoke();
            }
        }
    }

    void updateMapImageAndInfo()
    {
        mapImage.sprite = selectedButton.GetComponent<MapInfo>().mapImage;
        mapInfo.text = selectedButton.GetComponent<MapInfo>().mapInfo;
    }

    public void chooseMap()
    {
        mapChosenWitness.text = "Map: " + selectedButton.gameObject.name.ToString();
        countdownText.text = "Countdown: Press START or ENTER.";
        mapChosen = true;
    }

    public void startCountdown()
    {
        if (countdownInProgress)
        {
            countdown -= Time.deltaTime;
            countdownText.text = countdownText.text = "Countdown: " + Mathf.Ceil(countdown);

            if (countdown <= 0)
            {
                StartSwarmGame();
                countdownInProgress = false;
                countdown = 0;
            }

            if (countdownCanceled)
            {
                countdownInProgress = false;
                countdown = defaultCountdown;
                countdownText.text = "Countdown: Press START or ENTER.";
            }
        }
    }

    void mouseScript()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.collider.name);
            if (hit.collider.gameObject.GetComponent<Button>() != null)
            {
                Selector.transform.localPosition = hit.collider.transform.localPosition;
                selectedButton = hit.collider.gameObject;
                updateSelectedButtonNumber();
                updateMapImageAndInfo();

                selectorAudioSource.clip = positiveFeedback;
                selectorAudioSource.Play();
            }
        }
        mouseClick();
    }

    void mouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectedButton.GetComponent<Button>().onClick.Invoke();
        }
    }

    void updateSelectedButtonNumber()
    {
        int newNumber = 0;

        foreach (GameObject button in buttonsPage1)
        {
            if (button == selectedButton)
            {
                selectedButtonNumber = newNumber;
            }

            newNumber++;
        }
    }

    /// <summary>
    /// ///////////////// Multiplayer Maps
    /// </summary>
    /// 


    /// <summary>
    /// ///////////////// Swarm Maps
    /// </summary>
    /// 

    public void StartSwarmGame()
    {
        if(selectedButton.GetComponent<MapInfo>().mapName == "Downpour")
        {
            Downpour();
        }

        if (selectedButton.GetComponent<MapInfo>().mapName == "Tumbleweed")
        {
            Tumbleweed();
        }
    }

    public void Downpour()
    {
        SceneManager.LoadScene("Coop - 001 - Downpour");
    }

    public void Tumbleweed()
    {
        SceneManager.LoadScene("Coop - 002 - Tumbleweed");
    }

}
