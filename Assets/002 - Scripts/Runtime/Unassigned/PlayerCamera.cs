using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerCamera : MonoBehaviour
{
    public PlayerRagdoll ragdollPrefab { get { return _ragdollPrefab; } set { _ragdollPrefab = value; } }

    public float frontEndMouseSens;
    public float backEndMouseSens;
    public Transform playerCameraScriptParent;
    public Transform horizontalAxisTarget;
    public Transform verticalAxisTarget;
    public float xRotation = 0f;
    public float yRotation = 0f;
    float minXClamp = -85;
    float maxXClamp = 85;


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
    float sway, _deathCameraLookAtTimer;
    PlayerRagdoll _ragdollPrefab;

    //public AimAssistCapsule aimAssistCapsule;
    public AimAssistCone aimAssistCapsule;










    [SerializeField] ControllerType _controllerType;


    private void Awake()
    {

    }

    void Start()
    {
        frontEndMouseSens = player.playerDataCell.sens;
        //transform.parent = null;
        player.OnPlayerRespawnEarly -= OnRespawnEarly_Delegate;
        player.OnPlayerRespawnEarly += OnRespawnEarly_Delegate;

        player.OnPlayerDeath -= OnDeath_Delegate;
        player.OnPlayerDeath += OnDeath_Delegate;

        try
        {
            GameManager.instance.OnCameraSensitivityChanged -= OnCameraSensitivityChanged;
            GameManager.instance.OnCameraSensitivityChanged += OnCameraSensitivityChanged;
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


        frontEndMouseSens = player.playerDataCell.sens;
        backEndMouseSens = frontEndMouseSens * 30;


        if (_controllerType == ControllerType.Joystick)
            backEndMouseSens *= 1.1f;
        else
            backEndMouseSens /= 10;



        //if (aimAssistCapsule.reticuleFriction)
        backEndMouseSens *= (1f - (aimAssistCapsule.reticuleFrictionTick / 100f));

        if (pController.isAiming)
        {
            backEndMouseSens *= 0.65f;

            if (_controllerType == ControllerType.Joystick && player.playerInventory.activeWeapon.scopeFov == 20)
                backEndMouseSens *= 0.6f;
        }

        //if (player.aimAssist.redReticuleIsOn && (pController.activeControllerType == ControllerType.Custom || player.GetComponent<PlayerController>().activeControllerType == ControllerType.Joystick))
        {
            //backEndMouseSens *= 0.5f;
            backEndMouseSens *= (1f - (player.aimAssist.redReticuleTick / 100f));
        }

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

            mouseX = _xAxisInput * backEndMouseSens * Time.deltaTime + HorizontalSway();
            mouseY = _yAxisInput * backEndMouseSens * 0.75f * Time.deltaTime;

            float xDeadzone = mouseX * 0.1f;
            float yDeadzone = mouseY * 0.1f;



            //if (controllerType == "controller") ;
            //mouseY = _yAxisInput * mouseSensitivity * 0.5f * Time.deltaTime;

            yRotation -= mouseX + HorizontalSway();
            xRotation -= mouseY + VerticalSway();
            xRotation = Mathf.Clamp(xRotation, minXClamp, maxXClamp);

            if (xRotation > minXClamp && xRotation < maxXClamp)
                _clampedMouseY = Mathf.Clamp(mouseY, minXClamp, maxXClamp);
            else _clampedMouseY = 0;















            // PROCESS PLAYER INPUT
            if ((player.isDead || player.isRespawning) && player.playerThatKilledMe)
            {
                _deathCameraLookAtTimer -= Time.deltaTime;
                //if (_deathCameraLookAtTimer > 0)
                {
                    //var lookDir = player.lastPlayerSource.transform.position - transform.position;
                    //lookDir.y = 0; // keep only the horizontal direction
                    //verticalAxisTarget.rotation = Quaternion.LookRotation(lookDir);
                    var p = player.playerThatKilledMe.transform.position + new Vector3(0, 10, 0); /*p.y = 0;*/
                    //playerCameraScriptParent.LookAt(p);
                    verticalAxisTarget.LookAt(p);
                    //verticalAxisTarget.rotation = Quaternion.Euler(0, verticalAxisTarget.rotation.y, verticalAxisTarget.rotation.z);

                    //p = player.lastPlayerSource.transform.position; 
                    //horizontalAxisTarget.LookAt(p);

                    return;
                }
            }




            verticalAxisTarget.Rotate(Vector3.left * _clampedMouseY); // NEW, multiple scripts can now interact with local rotation

            if (horizontalAxisTarget.transform.root == horizontalAxisTarget)
                horizontalAxisTarget.Rotate(Vector3.up * mouseX);
            else
                horizontalAxisTarget.localRotation = Quaternion.Euler(0, -yRotation, 0f);

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
        //frontEndMouseSens = GameManager.instance.camSens;
    }

    void OnDeath_Delegate(Player p)
    {
        _deathCameraLookAtTimer = 2;
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
