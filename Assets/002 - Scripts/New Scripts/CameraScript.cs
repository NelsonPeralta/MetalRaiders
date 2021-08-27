using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CameraScript : MonoBehaviour
{
    public float defaultMouseSensitivy;
    public float mouseSensitivity;
    public Transform playerBody;
    public float xRotation = 0f;
    public float yRotation = 0f;
    float minXClamp = -90;
    float maxXClamp = 90;

    public Player player;
    public int playerRewiredID;
    public PlayerController pController;
    public PlayerProperties pProperties;
    public Camera mainCam;
    public Camera gunCam;
    public MyPlayerManager pManager;
    public Vector3 mainCamDefaultLocalPosition;
    public Quaternion mainCamDefaultLocalRotation;

    float mouseX, mouseY;

    // Start is called before the first frame update
    void Start()
    {
        mainCamDefaultLocalPosition = mainCam.transform.localPosition;
        mainCamDefaultLocalRotation = mainCam.transform.localRotation;
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
        if (!pController.PV.IsMine)
            return;

        if (pController.lastControllerType.ToString() != "Keyboard" && pController.lastControllerType.ToString() != "Mouse")
            mouseSensitivity = defaultMouseSensitivy;
        else if (pController.lastControllerType.ToString() == "Keyboard" || pController.lastControllerType.ToString() == "Mouse")
        {
            mouseSensitivity = defaultMouseSensitivy / 25;
        }

        if (pController.isAiming)
            mouseSensitivity = mouseSensitivity / 2;

        if (pProperties.aimAssist.redReticuleIsOn)
            mouseSensitivity = mouseSensitivity / 3;

        if (pController.playerProperties != null && !pController.pauseMenuOpen)
        {

            mouseX = player.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = player.GetAxis("Mouse Y") * mouseSensitivity * 0.75f * Time.deltaTime;

            xRotation -= mouseY;
            yRotation -= mouseX;
            xRotation = Mathf.Clamp(xRotation, minXClamp, maxXClamp);

            transform.localRotation = Quaternion.Euler(xRotation, 0, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }

    }

    public void SetPlayerIDInInput()
    {
        player = ReInput.players.GetPlayer(playerRewiredID);
    }

    public void RotateCameraBy(float rotationAmount)
    {
        playerBody.Rotate(Vector3.up * rotationAmount);
    }
}
