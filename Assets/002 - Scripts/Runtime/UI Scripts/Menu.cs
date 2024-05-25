using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour
{
    Launcher launcher;
    public string menuName;
    public bool open;

    Button _loginButton;

    private void Start()
    {
        launcher = Launcher.instance;
    }

    public void Open()
    {
        print($"Opened {menuName} Menu");
        open = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        open = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!open)
            return;

        if (menuName == "login")
        {
            if (!_loginButton)
                _loginButton = transform.Find("Login Button").GetComponent<Button>();

            if (EventSystem.current.currentSelectedGameObject == launcher.loginUsernameText.gameObject && Input.GetKeyDown(KeyCode.Tab))
                launcher.loginPasswordText.Select();

            if (EventSystem.current.currentSelectedGameObject == launcher.loginPasswordText.gameObject && Input.GetKeyDown(KeyCode.Tab))
                _loginButton.Select();

            if (EventSystem.current.currentSelectedGameObject == launcher.loginPasswordText.gameObject && Input.GetKeyDown(KeyCode.Return))
                _loginButton.onClick.Invoke();

            if (EventSystem.current.currentSelectedGameObject == _loginButton.gameObject && Input.GetKeyDown(KeyCode.Return))
                launcher.Login();





            if (EventSystem.current.currentSelectedGameObject == launcher.loginUsernameText.gameObject)
                launcher.loginUsernameText.placeholder.GetComponent<TMPro.TMP_Text>().text = "";
            else
                launcher.loginUsernameText.placeholder.GetComponent<TMPro.TMP_Text>().text = "Username";

            if (EventSystem.current.currentSelectedGameObject == launcher.loginPasswordText.gameObject)
                launcher.loginPasswordText.placeholder.GetComponent<TMPro.TMP_Text>().text = "";
            else
                launcher.loginPasswordText.placeholder.GetComponent<TMPro.TMP_Text>().text = "Password";
        }
    }
}