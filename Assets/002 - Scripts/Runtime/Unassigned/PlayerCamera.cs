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

    [SerializeField] float xAxisInput, yAxisInput, mouseX, mouseY, _clampedMouseY, _correctedXAxisInput, _correctedYAxisInput;

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

    public float _blockTime, _trueLocalX;


    [SerializeField] Transform _weaponOffset;
    [SerializeField] int _offsetTickRightRotation, _offsetTickUpRotation;
    [SerializeField] float _angleBetweenPlayerForwardAndVertAxis;
    [SerializeField] Vector3 _weaponOffsetLocalPosition = new Vector3(0, 0, 0);
    [SerializeField] Vector3 _weaponOffsetLocalRotation = new Vector3();






    private void Awake()
    {

    }

    void Start()
    {
        transform.root.GetComponent<Player>().OnPlayerIdAssigned -= OnPlayerIdAndRewiredIdAssigned_Delegate;
        transform.root.GetComponent<Player>().OnPlayerIdAssigned += OnPlayerIdAndRewiredIdAssigned_Delegate;
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
        _angleBetweenPlayerForwardAndVertAxis = Vector3.SignedAngle(verticalAxisTarget.forward, transform.root.forward, verticalAxisTarget.right);



        if (_blockTime > 0)
        {
            _blockTime -= Time.deltaTime;
        }


        if (!GameManager.instance.gameStarted) return;
        if (!pController.PV.IsMine) return;
        if (pController.cameraIsFloating) return;

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



        if (aimAssistCapsule.reticuleFriction)
            backEndMouseSens *= (1f - (aimAssistCapsule.reticuleFrictionTick / 70f)); // reticuleFrictionTick = 30 max

        if (pController.isAiming)
        {
            backEndMouseSens *= 0.65f;

            if (player.playerInventory.activeWeapon.scopeMagnification == WeaponProperties.ScopeMagnification.Long)
            {
                backEndMouseSens *= 0.4f;


                if (_controllerType == ControllerType.Joystick) backEndMouseSens *= 1.2f;
            }
        }

        //if (player.aimAssist.redReticuleIsOn && (pController.activeControllerType == ControllerType.Custom || player.GetComponent<PlayerController>().activeControllerType == ControllerType.Joystick))
        //{
        //    //backEndMouseSens *= 0.5f;
        //    backEndMouseSens *= (1f - (player.aimAssist.redReticuleTick / 100f));
        //}

        if (!pController.pauseMenuOpen && _blockTime <= 0)
        {
            xAxisInput = rewiredPlayer.GetAxis("Mouse X");
            yAxisInput = rewiredPlayer.GetAxis("Mouse Y");

            _correctedXAxisInput = xAxisInput; _correctedYAxisInput = yAxisInput;

            if (_controllerType == ControllerType.Joystick)
            {
                if (Mathf.Abs(xAxisInput) <= 0.1f)
                    _correctedXAxisInput = 0;
                if (Mathf.Abs(yAxisInput) <= 0.1f)
                    _correctedYAxisInput = 0;

                _correctedYAxisInput *= 0.75f;
            }
            float xExtremeDeadzone = 0.98f;
            float yExtremeDeadzone = 0.98f;

            if (Mathf.Abs(xAxisInput) >= xExtremeDeadzone)
                //if (Mathf.Abs(yAxisInput) <= 0.2f || yAxisInput == 0)
                _correctedXAxisInput *= 1.6f;

            if (Mathf.Abs(yAxisInput) >= yExtremeDeadzone)
                //if (Mathf.Abs(xAxisInput) <= 0.2f || yAxisInput == 0)
                _correctedYAxisInput *= 1.6f;

            mouseX = _correctedXAxisInput * backEndMouseSens * Time.deltaTime + HorizontalSway();
            mouseY = _correctedYAxisInput * backEndMouseSens * 0.75f * Time.deltaTime;

            float xDeadzone = mouseX * 0.1f;
            float yDeadzone = mouseY * 0.1f;



            //if (controllerType == "controller") ;
            //mouseY = _correctedYAxisInput * mouseSensitivity * 0.5f * Time.deltaTime;

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
                    //var p = player.playerThatKilledMe.transform.position + new Vector3(0, 10, 0); /*p.y = 0;*/
                    //verticalAxisTarget.LookAt(p);

                    return;
                }
            }




            verticalAxisTarget.Rotate(Vector3.left * mouseY); // NEW, multiple scripts can now interact with local rotation
            //localeulerangles return over 300 when looking up and returns below 90 when looking down
            _trueLocalX = 0;


            if (verticalAxisTarget.localEulerAngles.x > 0 && verticalAxisTarget.localEulerAngles.x < 180) _trueLocalX -= verticalAxisTarget.localEulerAngles.x;
            if (verticalAxisTarget.localEulerAngles.x > 180 && verticalAxisTarget.localEulerAngles.x <= 360) _trueLocalX = 360 - verticalAxisTarget.localEulerAngles.x;

            _trueLocalX *= -1;

            if (_trueLocalX > maxXClamp) verticalAxisTarget.localRotation = Quaternion.Euler(new Vector3(maxXClamp, verticalAxisTarget.localRotation.y, verticalAxisTarget.localRotation.z));
            else if (_trueLocalX < minXClamp) verticalAxisTarget.localRotation = Quaternion.Euler(new Vector3(minXClamp, verticalAxisTarget.localRotation.y, verticalAxisTarget.localRotation.z));



            if (horizontalAxisTarget.transform.root == horizontalAxisTarget)
                horizontalAxisTarget.Rotate(Vector3.up * mouseX);
            else
                horizontalAxisTarget.localRotation = Quaternion.Euler(0, -yRotation, 0f);

        }

        //WeaponSway();
    }

    private void FixedUpdate()
    {
        WeaponOffset();
    }


    void WeaponOffset()
    {
        //_angleBetweenPlayerForwardAndVertAxis = Vector3.SignedAngle(verticalAxisTarget.forward, transform.root.forward, Vector3.right);
        //if (Vector3.Cross(verticalAxisTarget.forward, transform.root.forward).y < 0) _angleBetweenPlayerForwardAndVertAxis = -_angleBetweenPlayerForwardAndVertAxis;



        if (!player.isAlive || !player.movement.isGrounded)
        {

            if (_offsetTickRightRotation > 0)
            {
                if (_offsetTickRightRotation >= 3)
                    _offsetTickRightRotation -= 3;
                else
                    _offsetTickRightRotation -= _offsetTickRightRotation;
            }
            else if (_offsetTickRightRotation < 0)
            {
                if (_offsetTickRightRotation <= -3)
                    _offsetTickRightRotation += 3;
                else
                    _offsetTickRightRotation -= _offsetTickRightRotation;
            }

            if (_offsetTickUpRotation > 0)
            {
                if (_offsetTickUpRotation >= 3)
                    _offsetTickUpRotation -= 3;
                else
                    _offsetTickUpRotation -= _offsetTickUpRotation;
            }
            else if (_offsetTickUpRotation < 0)
            {
                if (_offsetTickUpRotation <= -3)
                    _offsetTickUpRotation += 3;
                else
                    _offsetTickUpRotation -= _offsetTickUpRotation;
            }

        }
        else
        {
            if (_correctedYAxisInput == 0)
            {
                if (_offsetTickRightRotation > 0)
                {
                    if (_offsetTickRightRotation >= 3)
                        _offsetTickRightRotation -= 3;
                    else
                        _offsetTickRightRotation -= _offsetTickRightRotation;
                }
                else if (_offsetTickRightRotation < 0)
                {
                    if (_offsetTickRightRotation <= -3)
                        _offsetTickRightRotation += 3;
                    else
                        _offsetTickRightRotation -= _offsetTickRightRotation;
                }
            }
            else if (_correctedYAxisInput > 0) _offsetTickRightRotation = Mathf.Clamp(_offsetTickRightRotation + 3, -GameManager.DEFAULT_FRAMERATE / 2, GameManager.DEFAULT_FRAMERATE / 2);
            else if (_correctedYAxisInput < 0) _offsetTickRightRotation = Mathf.Clamp(_offsetTickRightRotation - 3, -GameManager.DEFAULT_FRAMERATE / 2, GameManager.DEFAULT_FRAMERATE / 2);


            if (_correctedXAxisInput == 0)
            {
                if (_offsetTickUpRotation > 0)
                {
                    if (_offsetTickUpRotation >= 2)
                        _offsetTickUpRotation -= 2;
                    else
                        _offsetTickUpRotation -= _offsetTickUpRotation;
                }
                else if (_offsetTickUpRotation < 0)
                {
                    if (_offsetTickUpRotation <= -2)
                        _offsetTickUpRotation += 2;
                    else
                        _offsetTickUpRotation -= _offsetTickUpRotation;
                }
            }
            else if (_correctedXAxisInput > 0) _offsetTickUpRotation = Mathf.Clamp(_offsetTickUpRotation + 2, -GameManager.DEFAULT_FRAMERATE / 2, GameManager.DEFAULT_FRAMERATE / 2);
            else if (_correctedXAxisInput < 0) _offsetTickUpRotation = Mathf.Clamp(_offsetTickUpRotation - 2, -GameManager.DEFAULT_FRAMERATE / 2, GameManager.DEFAULT_FRAMERATE / 2);
        }



        _weaponOffsetLocalPosition.z = -_angleBetweenPlayerForwardAndVertAxis * 0.001f;
        _weaponOffset.localPosition = _weaponOffsetLocalPosition;
        _weaponOffset.localRotation = Quaternion.Euler(_weaponOffset.localRotation.x, _offsetTickUpRotation * 0.1f, _weaponOffset.localRotation.z);
    }

    void OnPlayerIdAndRewiredIdAssigned_Delegate(Player p)
    {
        frontEndMouseSens = player.playerDataCell.sens;
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


    public void BlockPlayerCamera(float t)
    {
        _blockTime = t;
    }
}
