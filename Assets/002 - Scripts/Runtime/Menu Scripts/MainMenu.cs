using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rewired;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject _quickMatchBtn;


    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None; // Must Unlock Cursor so it can detect buttons
        Cursor.visible = true;


        _quickMatchBtn.SetActive(GameManager.instance.connection == GameManager.Connection.Online);
    }
}
