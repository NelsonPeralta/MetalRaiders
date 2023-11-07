using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rewired;

public class MainMenu : MonoBehaviour
{
    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None; // Must Unlock Cursor so it can detect buttons
        Cursor.visible = true;
    }
}
