using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionsMenu : MonoBehaviour
{
    [SerializeField] GameObject _gamepadControls, _keyboardControls;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _gamepadControls.SetActive(ReInput.controllers.GetLastActiveControllerType() == ControllerType.Joystick);
        _keyboardControls.SetActive(ReInput.controllers.GetLastActiveControllerType() != ControllerType.Joystick);
    }
}
