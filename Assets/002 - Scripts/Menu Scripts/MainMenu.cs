using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rewired;

public class MainMenu : MonoBehaviour
{
    public GameSettings gameSettings;

    [Header("UI Components")]
    public GameObject Selector;
    public GameObject[] buttons;
    public GameObject selectedButton;
    public int selectedButtonNumber;

    [Header("Players")]
    public Rewired.Player player;
    public int playerRID;    

    [Header("Swarm Menu")]    
    public GameObject swarmMenu;
    public LocalMpMenu swarmMenuScript;
    public bool swarmMenuOpen;
    public ParticleSystem[] swarmSmoke;
    public Light planeLight;

    [Header("Local MP Menu")]
    public GameObject LocalMpMenu;
    public LocalMpMenu LocalMpScript;
    public bool LocalMpMenuOpen;

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

        foreach (ParticleSystem smoke in swarmSmoke)
            smoke.Stop();

        ClearOtherGameSettings();
    }

    private void Update()
    {
        if (!swarmMenuOpen && !LocalMpMenuOpen)
        {
            selectButtomUP();
            selectButtonDown();
            clickButton();
            mouseScript();
        }
        //Debug.Log("X value = " + player.GetAxis("Move Vertical"));
    }

    public void openAndCloseSwarmMenu()
    {
        StaticVariables.numberOfPlayers = 1;        

        if (!swarmMenuOpen)
        {
            swarmMenu.SetActive(true);
            gameSettings.loadSwarm = true;
            swarmMenuOpen = true;
            foreach (ParticleSystem smoke in swarmSmoke)
                smoke.Play();
            planeLight.color = Color.HSVToRGB(0, 0.25f, 1);
            audioSource.clip = swarmMenuMusic;
            audioSource.Play();
        }
        else
        {
            swarmMenu.SetActive(false);
            swarmMenuOpen = false;
            foreach (ParticleSystem smoke in swarmSmoke)
                smoke.Stop();
            planeLight.color = Color.white;
            audioSource.clip = mainMenuMusic;
            audioSource.Play();
        }
    }

    public void OpenAndCloseLocalMPMenu()
    {
        StaticVariables.numberOfPlayers = 1;

        if (!LocalMpMenuOpen)
        {
            LocalMpMenu.SetActive(true);
            LocalMpMenuOpen = true;
            Selector.SetActive(false);
            foreach (GameObject button in buttons)
            {
                button.gameObject.SetActive(false);
            }
        }
        else
        {
            LocalMpMenu.SetActive(false);
            LocalMpMenuOpen = false;
            Selector.SetActive(true);
            foreach (GameObject button in buttons)
            {
                button.gameObject.SetActive(true);
            }
        }
    }

    void selectButtomUP()
    {
        float x = player.GetAxis("Move Vertical");

        if (x > 0)
        {
            if (selectedButton != buttons[0])
            {
                Selector.transform.localPosition = buttons[selectedButtonNumber - 1].transform.localPosition;
                selectedButton = buttons[selectedButtonNumber - 1];
                selectedButtonNumber = selectedButtonNumber - 1;

                selectorAudioSource.clip = positiveFeedback;
                selectorAudioSource.Play();
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
                Selector.transform.localPosition = buttons[selectedButtonNumber + 1].transform.localPosition;
                selectedButton = buttons[selectedButtonNumber + 1];
                selectedButtonNumber = selectedButtonNumber + 1;

                selectorAudioSource.clip = positiveFeedback;
                selectorAudioSource.Play();
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
            Debug.Log(hit.collider.name);
            if(hit.collider.gameObject.GetComponent<Button>() != null)
            {
                if (hit.collider.gameObject != selectedButton)
                {
                    Selector.transform.localPosition = hit.collider.transform.localPosition;
                    selectedButton = hit.collider.gameObject;
                    updateSelectedButtonNumber();
                }
            }
        }
        mouseClick();
    }

    void mouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectedButton.GetComponent<Button>().onClick.Invoke();
            Debug.Log("Clicked Mouse");
        }
    }

    void updateSelectedButtonNumber()
    {
        int newNumber = 0;
        
        foreach(GameObject button in buttons)
        {
            if(button == selectedButton)
            {
                selectedButtonNumber = newNumber;
            }

            newNumber++;
        }
    }

    void ClearOtherGameSettings()
    {
        var gameSettingsList = FindObjectsOfType<GameSettings>();
        Debug.Log($"Number of Game Settings {gameSettingsList.Length}");

        foreach(GameSettings gs in gameSettingsList)
        {
            if (!gs.swarmMenu && !gs.localMpMenu)
                Destroy(gs.gameObject);
        }
    }
}
