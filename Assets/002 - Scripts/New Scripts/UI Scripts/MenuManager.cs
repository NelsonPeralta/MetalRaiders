using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] Menu[] menus;

    void Awake()
    {
        Instance = this;
    }

    public void OpenMenu(string menuName) // Open a menu GO using the name from its Menu script
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)
            {
                menus[i].Open();
            }
            else if (menus[i].open)
            {
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
}