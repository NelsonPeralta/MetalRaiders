using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerCamera : MonoBehaviour
{
    public float defaultMouseSensitivy;
    public float mouseSensitivity;
    public Transform playerBody;
    public float xRotation = 0f;
    public float yRotation = 0f;
    float minXClamp = -90;
    float maxXClamp = 90;

    public Rewired.Player rewiredPlayer
    {
        get { return pController.rewiredPlayer; }
    }
    public PlayerController pController;
    public Player player;
    public Camera mainCam;
    public Camera gunCam;
    public Vector3 mainCamDefaultLocalPosition;
    public Quaternion mainCamDefaultLocalRotation;

    [SerializeField] float xAxisInput, yAxisInput, mouseX, mouseY;

    // Weapon Sway
    Quaternion defaultLocalRotation;
    float weaponSway = 3f;
    float targetVerticalSway;
    float currentVerticalSway;
    float targetHorizontalSway;
    float currentHorizontalSway;
    float sway;

    //public AimAssistCapsule aimAssistCapsule;
    public AimAssistCone aimAssistCapsule;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            defaultMouseSensitivy = GameManager.instance.camSens;
            GameManager.instance.OnCameraSensitivityChanged -= OnCameraSensitivityChanged;
            GameManager.instance.OnCameraSensitivityChanged += OnCameraSensitivityChanged;
        }
        catch { }

        try
        {
            defaultMouseSensitivy = GameManager.instance.camSens;
        }
        catch { }

        defaultLocalRotation = transform.localRotation;
        mainCamDefaultLocalPosition = mainCam.transform.localPosition;
        mainCamDefaultLocalRotation = mainCam.transform.localRotation;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pController.PV.IsMine)
            return;

        string controllerType = "";

        if (pController.activeControllerType.ToString() != "Keyboard" && pController.activeControllerType.ToString() != "Mouse")
        {
            controllerType = "controller";
            mouseSensitivity = defaultMouseSensitivy * 1.15f;
        }
        else if (pController.activeControllerType.ToString() == "Keyboard" || pController.activeControllerType.ToString() == "Mouse")
        {
            controllerType = "m&k";
            mouseSensitivity = defaultMouseSensitivy / 10;
        }

        if (aimAssistCapsule.reticuleFriction)
            mouseSensitivity /= 2;

        if (pController.isAiming)
            mouseSensitivity /= 2;

        //if (pProperties.aimAssist.redReticuleIsOn && (pProperties.GetComponent<PlayerController>().activeControllerType == ControllerType.Custom || pProperties.GetComponent<PlayerController>().activeControllerType == ControllerType.Joystick))
        //    mouseSensitivity /= 3;

        if (!pController.pauseMenuOpen)
        {
            xAxisInput = rewiredPlayer.GetAxis("Mouse X");
            yAxisInput = rewiredPlayer.GetAxis("Mouse Y");

            float _xAxisInput = xAxisInput, _yAxisInput = yAxisInput;

            if (controllerType == "controller")
            {
                if (Mathf.Abs(xAxisInput) <= 0.1f)
                    _xAxisInput = 0;
                if (Mathf.Abs(yAxisInput) <= 0.1f)
                    _yAxisInput = 0;

                _yAxisInput *= 0.75f;
            }
            float xExtremeDeadzone = 0.98f;
            float yExtremeDeadzone = 0.98f;

            if (Mathf.Abs(xAxisInput) >= xExtremeDeadzone)
                if (Mathf.Abs(yAxisInput) <= 0.2f || yAxisInput == 0)
                    _xAxisInput *= 2.5f;

            if (Mathf.Abs(yAxisInput) >= yExtremeDeadzone)
                if (Mathf.Abs(xAxisInput) <= 0.2f || yAxisInput == 0)
                    _yAxisInput *= 2.5f;

            mouseX = _xAxisInput * mouseSensitivity * Time.deltaTime + HorizontalSway();
            mouseY = _yAxisInput * mouseSensitivity * 0.75f * Time.deltaTime;

            float xDeadzone = mouseX * 0.1f;
            float yDeadzone = mouseY * 0.1f;



            //if (controllerType == "controller") ;
            //mouseY = _yAxisInput * mouseSensitivity * 0.5f * Time.deltaTime;

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
        if (!pController.isAiming || player.playerInventory.activeWeapon.scopeSway <= 0)
            return 0;

        weaponSway = player.playerInventory.activeWeapon.scopeSway;
        if (pController.isCrouching)
            weaponSway = player.playerInventory.activeWeapon.scopeSway / 2f;

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
        if (!pController.isAiming || player.playerInventory.activeWeapon.scopeSway <= 0)
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
        float factorX = (rewiredPlayer.GetAxis("Mouse Y")) * weaponSway;
        float factorY = -(rewiredPlayer.GetAxis("Mouse X")) * weaponSway;
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

    public void RotateCameraBy(float rotationAmount)
    {
        playerBody.Rotate(Vector3.up * rotationAmount);
    }

    void OnCameraSensitivityChanged()
    {
        defaultMouseSensitivy = GameManager.instance.camSens;
    }
}
