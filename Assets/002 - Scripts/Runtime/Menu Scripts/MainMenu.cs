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

        _quickMatchBtn.SetActive(GameManager.instance.connection == GameManager.Connection.Online);
    }
}
