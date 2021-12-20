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

    // Weapon Sway
    Quaternion defaultLocalRotation;
    float weaponSway = 3f;
    float targetVerticalSway;
    float currentVerticalSway;
    float targetHorizontalSway;
    float currentHorizontalSway;
    float sway;

    // Start is called before the first frame update
    void Start()
    {
        defaultLocalRotation = transform.localRotation;
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

            mouseX = player.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime + HorizontalSway();
            mouseY = player.GetAxis("Mouse Y") * mouseSensitivity * 0.75f * Time.deltaTime;

            yRotation -= mouseX + HorizontalSway();
            xRotation -= mouseY + VerticalSway();
            xRotation = Mathf.Clamp(xRotation, minXClamp, maxXClamp);

            transform.localRotation = Quaternion.Euler(xRotation, 0, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }

        //WeaponSway();
    }

    float HorizontalSway()
    {
        if (!pController.isAiming || pProperties.pInventory.activeWeapon.weaponSway <= 0)
            return 0;

        weaponSway = pProperties.pInventory.activeWeapon.weaponSway;
        if (pController.isCrouching)
            weaponSway = pProperties.pInventory.activeWeapon.weaponSway / 2f;

        if (targetHorizontalSway == 0)
        {
            targetHorizontalSway = Random.Range(-weaponSway, weaponSway);
            if (Mathf.Abs(targetHorizontalSway) <= weaponSway / 2f)
                targetHorizontalSway = (targetHorizontalSway / Mathf.Abs(targetHorizontalSway)) * 0.5f * weaponSway;
        }
        else
        {
            float swaySpeed = 1f * Time.deltaTime;
            sway = targetHorizontalSway * swaySpeed;

            if (currentHorizontalSway + sway > weaponSway || currentHorizontalSway + sway < -weaponSway)
            {
                sway *= -1;
                targetHorizontalSway = 0;
            }

            currentHorizontalSway += sway;
            if (targetHorizontalSway > 0)
                if (currentHorizontalSway >= targetHorizontalSway)
                    targetHorizontalSway = 0;
                else
                    ; // Do nothing
            else
                if (currentHorizontalSway <= targetHorizontalSway)
                targetHorizontalSway = 0;

            return sway;
        }
        return 0;
    }

    float VerticalSway()
    {
        if (!pController.isAiming || pProperties.pInventory.activeWeapon.weaponSway <= 0)
            return 0;
        if (targetVerticalSway == 0)
        {
            targetVerticalSway = Random.Range(-weaponSway, weaponSway);
            if (Mathf.Abs(targetVerticalSway) <= weaponSway / 2f)
                targetVerticalSway = (targetVerticalSway / Mathf.Abs(targetVerticalSway)) * 0.5f * weaponSway;
        }
        else
        {
            float swaySpeed = 1f * Time.deltaTime;
            sway = targetVerticalSway * swaySpeed;

            if (currentVerticalSway + sway > weaponSway || currentVerticalSway + sway < -weaponSway)
            {
                sway *= -1;
                targetVerticalSway = 0;
            }

            currentVerticalSway += sway;

            if (targetVerticalSway > 0)
            {
                if (currentVerticalSway >= targetVerticalSway)
                    targetVerticalSway = 0;
            }
            else
            {
                if (currentVerticalSway <= targetVerticalSway)
                    targetVerticalSway = 0;
            }

            return sway;
        }

        return 0;
    }
    void WeaponSway()
    {
        float maxamount = weaponSway * 1.1f;
        float factorX = (player.GetAxis("Mouse Y")) * weaponSway;
        float factorY = -(player.GetAxis("Mouse X")) * weaponSway;
        //float factorZ = -Input.GetAxis("Vertical") * amount;
        float factorZ = 0 * weaponSway;

        if (factorX > maxamount)
            factorX = maxamount;

        if (factorX < -maxamount)
            factorX = -maxamount;

        if (factorY > maxamount)
            factorY = maxamount;

        if (factorY < -maxamount)
            factorY = -maxamount;

        if (factorZ > maxamount)
            factorZ = maxamount;

        if (factorZ < -maxamount)
            factorZ = -maxamount;

        Quaternion Final = Quaternion.Euler(defaultLocalRotation.x + factorX, defaultLocalRotation.y + factorY, defaultLocalRotation.z + factorZ);
        transform.localRotation = transform.localRotation * Quaternion.Slerp(transform.localRotation, Final, (Time.time * 3));
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
