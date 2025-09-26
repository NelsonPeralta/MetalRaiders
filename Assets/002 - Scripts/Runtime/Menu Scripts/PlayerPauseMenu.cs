using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Rewired;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerPauseMenu : MonoBehaviour
{
    public AllPlayerScripts allPlayerScripts;
    public LocalMpMapChooser localMpMapChooser;
    public LocalMpGametypeChooser localMpGametypeChooser;

    [Header("Options")]
    public GameObject playerOptionsMenu;
    public bool playerOptionsMenuOpen;

    [Header("Quit")]
    public GameObject playerQuitMenu;
    public bool playerQuitMenuOpen;

    [Header("Players")]
    public Rewired.Player player;
    public int playerRID;

    [Header("UI Components")]
    public GameObject Selector;
    public GameObject[] buttons;
    public GameObject selectedButton;
    public int selectedButtonNumber;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioSource selectorAudioSource;
    public AudioClip positiveFeedback;
    public AudioClip negativeFeedback;

    Ray ray;
    RaycastHit hit;

    private void Start()
    {
        player = ReInput.players.GetPlayer(playerRID);

        if (buttons[0] != null)
            selectedButton = buttons[0];
    }

    private void Update()
    {
        Controller controller = player.controllers.GetLastActiveController();
        if (!playerQuitMenuOpen && !playerOptionsMenuOpen)
        {
            clickButton();
            if (!playerQuitMenuOpen)
            {
                selectButtomUP();
                selectButtonDown();
            }
            switch (controller.type)
            {
                case ControllerType.Mouse:
                    mouseScript();
                    break;
            }
        }
    }

    public void OpenCloseMapChooser()
    {
        if (!playerQuitMenuOpen)
        {
            playerQuitMenuOpen = true;
            selectedButton = null;
            playerQuitMenu.SetActive(true);
        }
        else
        {
            playerQuitMenuOpen = false;
            playerQuitMenu.SetActive(false);
        }
    }

    public void OpenCloseGametypeChooser()
    {
        if (!playerOptionsMenuOpen)
        {
            playerOptionsMenuOpen = true;
            playerOptionsMenu.SetActive(true);
        }
        else
        {
            playerOptionsMenuOpen = false;
            playerOptionsMenu.SetActive(false);
        }
    }

    void selectButtomUP()
    {
        float x = player.GetAxis("Move Vertical");

        if (x > 0)
        {
            if (selectedButton != buttons[0])
            {
                Selector.transform.position = buttons[selectedButtonNumber - 1].transform.position;
                selectedButton = buttons[selectedButtonNumber - 1];
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
            if (selectedButton != buttons[buttons.Length - 1])
            {
                Selector.transform.position = buttons[selectedButtonNumber + 1].transform.position;
                selectedButton = buttons[selectedButtonNumber + 1];
                selectedButtonNumber = selectedButtonNumber + 1;

                //selectorAudioSource.clip = positiveFeedback;
                //selectorAudioSource.Play();
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

    void mouseScript()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            Log.Print(() =>$"PLAYER PAUSE: {hit.collider.name}");
            if (hit.collider.gameObject.GetComponent<Button>() != null)
            {
                if (hit.collider.gameObject != selectedButton)
                {
                    if (!playerOptionsMenuOpen)
                        Selector.transform.localPosition = hit.collider.transform.localPosition;
                    selectedButton = hit.collider.gameObject;
                    updateSelectedButtonNumber();
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
            if (selectedButton)
                selectedButton.GetComponent<Button>().onClick.Invoke();
            else if (playerOptionsMenuOpen && !selectedButton)
                OpenCloseGametypeChooser();
            else if (playerQuitMenuOpen && !selectedButton)
                OpenCloseMapChooser();
        }
    }

    void updateSelectedButtonNumber()
    {
        int newNumber = 0;

        foreach (GameObject button in buttons)
        {
            if (button == selectedButton)
            {
                selectedButtonNumber = newNumber;
            }

            newNumber++;
        }
    }
}