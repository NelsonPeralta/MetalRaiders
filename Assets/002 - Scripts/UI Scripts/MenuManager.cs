using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.IO;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

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
        this.menus = GetComponentsInChildren<Menu>(true);

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
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                Debug.Log($"Opem {menuName}");
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
        // Close all menus first
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
        menu.Open(); // Then open the one we need
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void OpenMainMenu()
    {
        Debug.Log(WebManager.webManagerInstance.playerDatabaseAdaptor.PlayerDataIsSet());
        string menuname = "";
        if (WebManager.webManagerInstance.playerDatabaseAdaptor.PlayerDataIsSet())
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