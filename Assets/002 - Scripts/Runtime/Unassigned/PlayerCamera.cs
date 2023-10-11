using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerCamera : MonoBehaviour
{
    public PlayerRagdoll ragdollPrefab { get { return _ragdollPrefab; } set { _ragdollPrefab = value; } }

    public float defaultMouseSensitivy;
    public float mouseSensitivity;
    public Transform playerCameraScriptParent;
    public Transform horizontalAxisTarget;
    public Transform verticalAxisTarget;
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

    [SerializeField] float xAxisInput, yAxisInput, mouseX, mouseY, _clampedMouseY;

    // Weapon Sway
    Quaternion defaultLocalRotation;
    float weaponSway = 3f;
    float targetVerticalSway;
    float currentVerticalSway;
    float targetHorizontalSway;
    float currentHorizontalSway;
    float sway;
    PlayerRagdoll _ragdollPrefab;

    //public AimAssistCapsule aimAssistCapsule;
    public AimAssistCone aimAssistCapsule;










    ControllerType _controllerType;




    void Start()
    {
        player.OnPlayerRespawnEarly -= OnRespawnEarly_Delegate;
        player.OnPlayerRespawnEarly += OnRespawnEarly_Delegate;

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
        if (!GameManager.instance.gameStarted) return;
        if (!pController.PV.IsMine) return;
        if (pController.cameraisFloating) return;

        try
        {
            //if (ragdollPrefab)
            //    ragdollPrefab.cameraHolderParent.transform.localPosition = ragdollPrefab.ragdollRigidBody.transform.localPosition + new Vector3(0, 1, 0);
        }
        catch { }


        _controllerType = pController.activeControllerType;

        if (_controllerType == ControllerType.Joystick)
        {
            mouseSensitivity = defaultMouseSensitivy * 1.1f;
        }
        else
        {
            mouseSensitivity = defaultMouseSensitivy / 10;
        }

        if (aimAssistCapsule.reticuleFriction)
            mouseSensitivity *= 0.65f;

        if (pController.isAiming)
        {
            mouseSensitivity *= 0.65f;

            if (_controllerType == ControllerType.Joystick && player.playerInventory.activeWeapon.scopeFov == 20)
                mouseSensitivity *= 0.6f;
        }

        if (player.aimAssist.redReticuleIsOn && (pController.activeControllerType == ControllerType.Custom || player.GetComponent<PlayerController>().activeControllerType == ControllerType.Joystick))
            mouseSensitivity *= 0.5f;

        if (!pController.pauseMenuOpen)
        {
            xAxisInput = rewiredPlayer.GetAxis("Mouse X");
            yAxisInput = rewiredPlayer.GetAxis("Mouse Y");

            float _xAxisInput = xAxisInput, _yAxisInput = yAxisInput;

            if (_controllerType == ControllerType.Joystick)
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
                //if (Mathf.Abs(yAxisInput) <= 0.2f || yAxisInput == 0)
                _xAxisInput *= 1.6f;

            if (Mathf.Abs(yAxisInput) >= yExtremeDeadzone)
                //if (Mathf.Abs(xAxisInput) <= 0.2f || yAxisInput == 0)
                _yAxisInput *= 1.6f;

            mouseX = _xAxisInput * mouseSensitivity * Time.deltaTime + HorizontalSway();
            mouseY = _yAxisInput * mouseSensitivity * 0.75f * Time.deltaTime;

            float xDeadzone = mouseX * 0.1f;
            float yDeadzone = mouseY * 0.1f;



            //if (controllerType == "controller") ;
            //mouseY = _yAxisInput * mouseSensitivity * 0.5f * Time.deltaTime;

            yRotation -= mouseX + HorizontalSway();
            xRotation -= mouseY + VerticalSway();
            xRotation = Mathf.Clamp(xRotation, minXClamp, maxXClamp);

            _clampedMouseY = Mathf.Clamp(mouseY, minXClamp, maxXClamp);

            //verticalAxisTarget.localRotation = Quaternion.Euler(xRotation, 0, 0f); // OLD, prevents other script from changing localRotation
            verticalAxisTarget.Rotate(Vector3.left * _clampedMouseY); // NEW, multiple scripts can now interact with local rotation
            //Debug.Log(_clampedMouseY);


            if (horizontalAxisTarget.transform.root == horizontalAxisTarget)
                horizontalAxisTarget.Rotate(Vector3.up * mouseX);
            else
            {
                //Debug.Log(yRotation);
                horizontalAxisTarget.localRotation = Quaternion.Euler(0, yRotation, 0f);
            }

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
        horizontalAxisTarget.Rotate(Vector3.up * rotationAmount);
    }

    void OnCameraSensitivityChanged()
    {
        defaultMouseSensitivy = GameManager.instance.camSens;
    }

    void OnRespawnEarly_Delegate(Player p)
    {
        xRotation = yRotation = 0;
        ragdollPrefab = null;
        player.mainCamera.transform.parent = player.mainCamera.GetComponent<PlayerCameraSplitScreenBehaviour>().orignalParent;

        this.transform.parent = playerCameraScriptParent;
        horizontalAxisTarget = player.transform;
        verticalAxisTarget = player.playerInventory.transform;

        verticalAxisTarget.localRotation = Quaternion.Euler(0, 0, 0f);
    }
}
