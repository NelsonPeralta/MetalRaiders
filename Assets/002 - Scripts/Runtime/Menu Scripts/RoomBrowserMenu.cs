using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBrowserMenu : MonoBehaviour
{
    public static string[] GAMEPAD_ROOM_NAMES = { "Poutine", "Sushi", "Spaghetti", "Burger" };
    public static string[] FORBIDDEN_ROOM_NAMES = { "nigger", "fuck", "faggot", "coon" };



    [SerializeField] GameObject _keyboardCreateRoomHolder, _gamepadCreateRoomHolder, _nbPlayersHolder;




    private void Update()
    {
        _keyboardCreateRoomHolder.SetActive(GameManager.instance.activeControllerType != ControllerType.Joystick);
        _gamepadCreateRoomHolder.SetActive( GameManager.instance.activeControllerType == ControllerType.Joystick);
    }
}
