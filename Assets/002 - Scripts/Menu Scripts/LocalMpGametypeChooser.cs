using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rewired;
using TMPro;

public class LocalMpGametypeChooser : MonoBehaviour
{
    [Header("Local MP")]
    public LocalMpMenu localMpMenu;

    [Header("Components")]
    public GameObject Selector;
    public GameObject[] buttonsPage1;
    public GameObject selectedButton;
    public int selectedButtonNumber;
    public int pageNumber;
    public bool gametypeChosen;
    public TextMeshProUGUI gametypeChosenWitness;

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
        }
    }

    private void Update()
    {        
        if (localMpMenu.gametypeMenuOpen)
        {
            Controller controller = player.controllers.GetLastActiveController();
            selectButtomUP();
            selectButtonDown();
            buttonClick();
            switch (controller.type)
            {
                case ControllerType.Mouse:
                    mouseScript();
                    break;
            }
        }
    }

    void selectButtomUP()
    {
        float x = player.GetAxis("Move Vertical");

        if (x > 0)
        {
            if (selectedButton != buttonsPage1[0])
            {
                Selector.transform.position = buttonsPage1[selectedButtonNumber - 1].transform.position;
                selectedButton = buttonsPage1[selectedButtonNumber - 1];
                selectedButtonNumber = selectedButtonNumber - 1;

                //selectorAudioSource.clip = positiveFeedback;
                //selectorAudioSource.Play();
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
                Selector.transform.position = buttonsPage1[selectedButtonNumber + 1].transform.position;
                selectedButton = buttonsPage1[selectedButtonNumber + 1];
                selectedButtonNumber = selectedButtonNumber + 1;

                //selectorAudioSource.clip = positiveFeedback;
                //selectorAudioSource.Play();
            }
        }
    }

    void buttonClick()
    {
        if (player.GetButtonDown("Switch Grenades") || player.GetButtonDown("Enter"))
        {
            if (selectedButton.GetComponent<Button>() != null)
            {
                selectedButton.GetComponent<Button>().onClick.Invoke();
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
                foreach (GameObject button in buttonsPage1)
                {
                    if (hit.collider.gameObject == button)
                    {
                        Selector.transform.position = hit.collider.transform.position;
                        selectedButton = hit.collider.gameObject;
                        updateSelectedButtonNumber();

                        //selectorAudioSource.clip = positiveFeedback;
                        //selectorAudioSource.Play();
                    }
                }
            }
            else
            {
                selectedButton = null;
            }
        }
        else
        {
            selectedButton = null;
        }
        mouseClick();
    }

    void mouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (localMpMenu.gametypeMenuOpen && !selectedButton)
                localMpMenu.OpenCloseGametypeChooser();
            if (selectedButton)
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
    /// ///////////////// Setting Maps
    /// </summary>
    /// 

    /// <summary>
    /// ///////////////// Swarm Maps
    /// </summary>
    /// 
}