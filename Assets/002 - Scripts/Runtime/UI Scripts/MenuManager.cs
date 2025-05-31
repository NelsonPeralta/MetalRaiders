using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using Steamworks;
using static GameManager;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get { return FindObjectOfType<MenuManager>(); } }

    public TMP_Text loadingMenuText;

    [SerializeField] Menu[] menus;


    void Awake()
    {
        name += $" {Random.Range(100, 999)}";
        print($"MenuManager Awake {name}");
    }

    private void OnEnable()
    {
        print("MenuManager OnEnable");
    }

    private void Start()
    {
        print("MenuManager Start");
    }

    public string GetOpenMenu()
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                return menus[i].menuName;
            }
        }

        return "";
    }

    public void OpenMenu(string menuName, bool closeOthers = true) // Open a menu GO using the name from its Menu script
    {

        Debug.Log($"OPEN MENU: {menuName} {closeOthers} {PhotonNetwork.InRoom} {PhotonNetwork.IsMasterClient}");

        if (GameManager.instance.previousScenePayloads.Contains(GameManager.PreviousScenePayload.OpenCarnageReportAndCredits))
        {
            menuName = "carnage report"; print($"Changed to Carnage Report {menuName}");
        }


        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                Debug.Log($"Open {menus[i].menuName}");
                menus[i].Open();
            }
            else if (menus[i].open && closeOthers)
            {
                Debug.Log($"Closing {menus[i].menuName}");
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

    public void OpenPopUpMenu(Menu menu) // Open a menu GO using the Menu script itself, used for connecting with buttons
    {
        print($"OpenPopUpMenu {menu.menuName}");
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i] == menu)
                menus[i].Open();
        }
    }

    public void OpenPopUpMenu(string menuName) // Open a menu GO using the Menu script itself, used for connecting with buttons
    {
        print($"OpenPopUpMenu {menuName}");
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                Debug.Log($"Open {menus[i].menuName}");
                menus[i].Open();
            }
        }
    }

    public void OpenErrorMenu(string mess)
    {
        print($"OpenErrorMenu");
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName.Equals("error"))
            {
                Launcher.instance.errorMenuText.text = mess;
                Debug.Log($"OpenErrorMenu {menus[i].menuName}");
                menus[i].Open();
            }
        }
    }

    public void CloseMenu(Menu menu)
    {
        Debug.Log($"Closing {menu.menuName}");


        if (GameManager.instance.previousScenePayloads.Contains(PreviousScenePayload.LoadTimeOutOpenErrorMenu)) { GameManager.instance.RemoveFromPreviousScenePayload(PreviousScenePayload.LoadTimeOutOpenErrorMenu); }



        menu.Close();

        if (GameManager.instance.previousScenePayloads.Contains(PreviousScenePayload.ErrorWhileCreatingRoom))
        {
            GameManager.instance.RemoveFromPreviousScenePayload(PreviousScenePayload.ErrorWhileCreatingRoom);
            OpenMenu("room browser");
        }
    }
    public void CloseMenu(string m)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName.Equals(m))
            {
                menus[i].Close();
            }
        }
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void OpenMainMenu()
    {
        string menuname = "";
        //if (WebManager.webManagerInstance.pda.PlayerDataIsSet())
        //    menuname += "online ";
        //else
        //    menuname += "offline ";
        //menuname += "title";
        menuname = "online title";
        Debug.Log($"MenuManager OpenMainMenu");
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

    public Menu GetMenu(string n)
    {
        print($"GetMenu {n}");
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName.Equals(n))
                return menus[i];
        }
        return null;
    }

    public void OpenLoadingMenu(string message = "Loading...")
    {
        print("OpenLoadingMenu");
        loadingMenuText.text = message;

        OpenMenu("loading");
    }

    void ResetLoadingMenu()
    {
        print("ResetLoadingMenu");
        loadingMenuText.text = "Loading...";
    }


    public void CloseCarnageReportMenu()
    {
        print($"CloseCarnageReportMenu {GameManager.instance.nbLocalPlayersPreset}");

        if (GameManager.instance.connection == GameManager.Connection.Local)
        {
            Launcher.instance.nbLocalPlayersHolder.SetActive(true);
            Launcher.instance.nbLocalPlayersText.text = GameManager.instance.nbLocalPlayersPreset.ToString();
        }

        if (GameManager.instance.previousScenePayloads.Contains(GameManager.PreviousScenePayload.OpenMultiplayerRoomAndCreateNamePlates))
        {
            GameManager.instance.RemoveFromPreviousScenePayload(GameManager.PreviousScenePayload.OpenMultiplayerRoomAndCreateNamePlates);
            Launcher.instance.TriggerOnJoinedRoomBehaviour();
            //MenuManager.Instance.OpenMenu("multiplayer_room");
            //Launcher.instance.DestroyNameplates();
            //Launcher.instance.CreateNameplates();
        }
        else
        {
            OpenMainMenu();
        }

        CurrentRoomManager.instance.ResetPlayerDataCellsCurrentGameScoreOnly();
    }

    public void OpenCarnageReportMenu()
    {
        print("OpenCarnageReportMenu");
        OpenMenu("carnage report");
    }

    public void OpenRoomBrowserMenu()
    {
        print("OpenRoomBrowserMenu");
        OpenMenu("room browser");
    }

    public List<string> GetActiveMenusName()
    {
        List<string> _menus = new List<string>();

        for (int i = 0; i < menus.Length; i++)
            if (menus[i].open)
            {
                print($"GetActiveMenuName {menus[i].menuName}");
                _menus.Add(menus[i].name);
            }

        return _menus;
    }

    public bool APopUpMenuisOpen()
    {
        for (int i = 0; i < menus.Length; i++)
            if (menus[i].popup && menus[i].open)
            {
                return true;
            }

        return false;
    }
}