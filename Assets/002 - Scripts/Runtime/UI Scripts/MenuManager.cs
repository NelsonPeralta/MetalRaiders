using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    public TMP_Text loadingMenuText;

    [SerializeField] Menu[] menus;

    void Awake()
    {
        if (Instance)
        {
            Debug.Log("There is a MenuManager Instance");
            Destroy(gameObject);
            return;
        }
        //DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void OnEnable()
    {
        Debug.Log("MenuManager OnEnable");
        //this.menus = GetComponentsInChildren<Menu>(true);

        //GameManager.instance.OnSceneLoadedEvent -= OnSceneLoaded;
        //GameManager.instance.OnSceneLoadedEvent += OnSceneLoaded;
    }

    private void Start()
    {
        //GameManager.instance.OnSceneLoadedEvent -= OnSceneLoaded;
        //GameManager.instance.OnSceneLoadedEvent += OnSceneLoaded;
        //OnSceneLoaded();
    }

    public void OpenMenu(string menuName) // Open a menu GO using the name from its Menu script
    {

        Debug.Log($"Opem {menuName}");
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                menus[i].Open();
            }
            else if (menus[i].open)
            {
                Debug.Log($"Closing {menuName}");
                CloseMenu(menus[i]);
            }
        }
    }

    public void OpenMenu(Menu menu) // Open a menu GO using the Menu script itself
    {
        ResetLoadingMenu();

        Debug.Log($"Opem {menu.menuName}");
        // Close all menus first
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                Debug.Log($"Closing {menus[i].menuName}");
                CloseMenu(menus[i]);
            }
        }
        menu.Open(); // Then open the one we need
    }

    public void CloseMenu(Menu menu)
    {
        Debug.Log($"Closing {menu.menuName}");
        menu.Close();
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void OpenMainMenu()
    {
        string menuname = "";
        if (WebManager.webManagerInstance.pda.PlayerDataIsSet())
            menuname += "online ";
        else
            menuname += "offline ";
        menuname += "title";
        Debug.Log($"MenuManager menuname: {menuname}");
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuname)
            {
                menus[i].Open();
            }
            else if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    public void OpenLoadingMenu(string message = "Loading...")
    {
        loadingMenuText.text = message;

        OpenMenu("loading");
    }

    void ResetLoadingMenu()
    {
        loadingMenuText.text = "Loading...";
    }

    public void OnSceneLoaded()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            try
            {
                foreach (Menu menu in menus)
                    menu.gameObject.SetActive(false); // Error here when changing scenes
            }
            catch { }
        }
        else
        {
            Debug.Log("Heressssssssssrsrsr");
            try
            {
                gameObject.SetActive(true);
            }
            catch { }
            Launcher.instance.ConnectToPhotonMasterServer();
            OpenMainMenu();
        }
    }
}