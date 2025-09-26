using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Rewired;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LocalMpMenu : MonoBehaviour
{
    public MainMenu mainMenu;
    public GameSettings gameSettings;
    public LocalMpMapChooser localMpMapChooser;
    public LocalMpGametypeChooser localMpGametypeChooser;

    [Header("Gametype")]
    public GameObject GametypeMiniMenu;
    public TextMeshProUGUI gametype;
    public bool gametypeMenuOpen;

    [Header("Map")]
    public GameObject mapMiniMenu;
    public TextMeshProUGUI map;
    public bool mapChooserOpen;
    public bool mapChosen;

    [Header("Players")]
    public Rewired.Player player;
    public int playerRID;

    [Header("UI Components")]
    public GameObject Selector;
    public GameObject[] buttons;
    public GameObject selectedButton;
    public int selectedButtonNumber;
    public Text countdownText;
    public float defaultCountdown = 5;
    public float countdown;
    public bool countdownInProgress;
    public bool countdownCanceled;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioSource selectorAudioSource;
    public AudioClip positiveFeedback;
    public AudioClip negativeFeedback;
    public AudioClip mainMenuMusic;
    public AudioClip swarmMenuMusic;

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
        if (!mapChooserOpen && !gametypeMenuOpen)
        {
            clickButton();
            if (!mapChooserOpen)
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
        Countdown();
    }

    public void OpenCloseMapChooser()
    {
        if (!mapChooserOpen)
        {
            mapChooserOpen = true;
            mapMiniMenu.SetActive(true);
        }
        else
        {
            mapChooserOpen = false;
            mapMiniMenu.SetActive(false);
        }
    }

    public void OpenCloseGametypeChooser()
    {
        if (!gametypeMenuOpen)
        {
            gametypeMenuOpen = true;
            GametypeMiniMenu.SetActive(true);
        }
        else
        {
            gametypeMenuOpen = false;
            GametypeMiniMenu.SetActive(false);
        }
    }

    void selectButtomUP()
    {
        float x = player.GetAxis("Move Vertical");

        if (x > 0)
        {
            if (selectedButton != buttons[0])
                if (buttons[selectedButtonNumber - 1].gameObject.activeSelf)
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
                if (buttons[selectedButtonNumber + 1].gameObject.activeSelf)
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
            Log.Print(() =>hit.collider.name);
            if (hit.collider.gameObject.GetComponent<Button>() != null)
            {
                if (hit.collider.gameObject != selectedButton)
                {
                    if (!gametypeMenuOpen)
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
            else if (gametypeMenuOpen && !selectedButton)
                OpenCloseGametypeChooser();
            else if (mapChooserOpen && !selectedButton)
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

    public void updateGametypeWitness(string gametypeName)
    {
        gametype.text = "GAMETYPE: " + gametypeName;
    }

    public void updateMapWitness(string mapName)
    {
        map.text = "MAP: " + mapName;
    }

    public void enableCountdownButton()
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = "READY";
    }

    public void disableCountdownButton()
    {
        countdownText.gameObject.SetActive(false);
    }

    public void StartCountdown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!countdownInProgress)
                countdownInProgress = true;
            else
                countdownCanceled = true;
        }
    }

    void Countdown()
    {
        if (countdownInProgress)
        {
            countdown -= Time.deltaTime;
            countdownText.text = countdownText.text = "Game starting in : " + Mathf.Ceil(countdown);

            if (countdown <= 0)
            {
                gameSettings.StartGame();
                countdownInProgress = false;
                countdown = 0;
            }

            if (countdownCanceled)
            {
                countdownInProgress = false;
                countdown = defaultCountdown;
                countdownText.text = "Ready";
                countdownCanceled = false;
            }
        }
    }

    private void OnEnable()
    {
        gameSettings.disableAllGametypes();
        gameSettings.disableAllMaps();
        gameSettings.disableAllGamemodes();
        StaticVariables.numberOfPlayers = 1;
        gametype.text = "GAMETYPE: ";
        map.text = "MAP: ";
        countdown = defaultCountdown;
        countdownInProgress = false;
        countdownCanceled = false;
        countdownText.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        //gameSettings.disableAllGametypes();
        //gameSettings.disableAllMaps();
        //gameSettings.disableAllGamemodes();
        mainMenu.gameObject.SetActive(true);
    }
}
