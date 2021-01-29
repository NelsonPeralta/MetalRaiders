using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CameraScript : MonoBehaviour
{
    public float mouseSensitivity;
    public Transform playerBody;
    public float xRotation = 0f;
    float minXClamp = -90;
    float maxXClamp = 90;

    public Player player;
    public int playerRewiredID;
    public PlayerController pController;
    public PlayerProperties pProperties;
    public Camera mainCam;
    public Camera gunCam;
    public PlayerManager pManager;

    float mouseX, mouseY;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (pManager == null)
        {
            player = ReInput.players.GetPlayer(pProperties.playerRewiredID);
            //mainCam.cullingMask &= ~(1 << 28);
            //gunCam.cullingMask |= (1 << 24);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pController.lastControllerType.ToString() != "Keyboard" && pController.lastControllerType.ToString() != "Mouse")
            mouseSensitivity = pProperties.activeSensitivity;
        else
        {
            mouseSensitivity = pProperties.activeSensitivity / 25;
            //Debug.Log("LAst Controller is M&K");
        }

        if (pController.playerProperties != null)
        {
            if (!pController.playerProperties.isDead)
            {
                mouseX = player.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                mouseY = player.GetAxis("Mouse Y") * mouseSensitivity * 0.5f * Time.deltaTime;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, minXClamp, maxXClamp);

                transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                playerBody.Rotate(Vector3.up * mouseX);
            }
        }
    }

    public void SetPlayerIDInInput()
    {
        player = ReInput.players.GetPlayer(playerRewiredID);
    }
}
