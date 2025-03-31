using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.ProBuilder.Shapes;

public class PlayerCamera : MonoBehaviour
{
    public static Vector3 THIRD_PERSON_LOCAL_OFFSET = new Vector3(0.8f, 0.6f, -2.5f);
    public PlayerRagdoll ragdollPrefab { get { return _ragdollPrefab; } set { _ragdollPrefab = value; } }

    public bool followPlayer { get { return _followPlayer; } set { _followPlayer = value; } }

    public Transform thirdPersonCameraPoint { get { return _playerCameraCenterPointCheck.target; } }

    public float frontEndMouseSens;
    public float backEndMouseSens;
    public Transform playerCameraScriptParent, aimingComponents;
    public Transform horizontalAxisTarget;
    public Transform verticalAxisTarget;
    public float upDownRotation = 0f;
    public float leftRightRotation = 0f;
    float minXClamp = -85;
    float maxXClamp = 85;


    public Rewired.Player rewiredPlayer
    {
        get { return pController.rewiredPlayer; }
    }

    public PlayerCameraCenterPointCheck playerCameraCenterPointCheck { get { return _playerCameraCenterPointCheck; } }


    public PlayerController pController;
    public Player player;
    public Camera mainCam;
    public Vector3 mainCamDefaultLocalPosition;
    public Quaternion mainCamDefaultLocalRotation;

    [SerializeField] float xAxisInput, yAxisInput, mouseX, mouseY, _correctedXAxisInput, _correctedYAxisInput;

    // Weapon Sway
    Quaternion defaultLocalRotation;
    PlayerRagdoll _ragdollPrefab;

    //public AimAssistCapsule aimAssistCapsule;
    public AimAssistCone aimAssistCapsule;



    [SerializeField] ControllerType _controllerType;

    public float _blockTime, _trueLocalX;


    [SerializeField] Transform _weaponOffset, _inventoryGo, _playerCameraHolder, _rigidBodyMoveRotationTarget;
    [SerializeField] int _offsetTickRightRotation, _offsetTickUpRotation;
    [SerializeField] float _angleBetweenPlayerForwardAndVertAxis;
    [SerializeField] Vector3 _weaponOffsetLocalPosition = new Vector3(0, 0, 0);
    [SerializeField] Vector3 _weaponOffsetLocalRotation = new Vector3();


    [SerializeField] bool _followPlayer;
    float xExtremeDeadzone = 0.98f, yExtremeDeadzone = 0.98f;

    [SerializeField] Transform _thirdPersonCameraPivot, _thirdPersonCameraTarget, _thirdPersonAimingComponentsOffset;
    [SerializeField] PlayerCameraCenterPointCheck _playerCameraCenterPointCheck;



    private void Awake()
    {

    }

    void Start()
    {
        followPlayer = true;
        player.OnPlayerIdAssigned -= OnPlayerIdAndRewiredIdAssigned_Delegate;
        player.OnPlayerIdAssigned += OnPlayerIdAndRewiredIdAssigned_Delegate;
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




        mainCam.transform.parent = null;
        mainCam.transform.position = _playerCameraHolder.position;

        print($"PlayerCamera: {Vector3.Angle(transform.forward, mainCam.transform.forward)}");

        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On)
        {
            SetupThirdPersonCamera();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On ||
            player.playerInventory.activeWeapon.weaponType == WeaponProperties.WeaponType.Heavy)
            _thirdPersonCameraPivot.transform.position = _playerCameraHolder.position; ;



        if (!GameManager.instance.gameStarted || !pController.PV.IsMine || pController.cameraIsFloating || player.playerDataCell == null) return;

        _angleBetweenPlayerForwardAndVertAxis = Vector3.SignedAngle(verticalAxisTarget.forward, transform.root.forward, verticalAxisTarget.right);
        if (_blockTime > 0) _blockTime -= Time.deltaTime;

        _controllerType = pController.activeControllerType;
        frontEndMouseSens = player.playerDataCell.sens;
        backEndMouseSens = frontEndMouseSens * 30;

        if (_controllerType == ControllerType.Joystick) backEndMouseSens *= 1.1f; else backEndMouseSens /= 10;

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
    }

    private void FixedUpdate()
    {
        WeaponOffset();
    }



    private float _tmpRotationVelocity;
    private float _rotationSmoothTime = 0.1F;



    // https://www.reddit.com/r/Unity3D/comments/qa6624/why_am_i_getting_camera_jitters_when_rotating/

    //    Oh boy, I've fought with this problem so many times. I'm on phone right now so won't type a long message, but just try ALL of these steps, something will probably work:
    //    Camera should not be child object of rigidbody, instead separate it and use cam.transform.position = player.transform.position in late update.
    //    rotate camera using mouse input in late update, store updated rotation in variables (Important for later use, keep reading)
    //    Rotate player which has rigidbody in FixedUpdate. ATTENTION: rotate rigidbody using MoveRotation, if you use transform.rotation the rigidbody will lag when rotating and moving at the same time.
    //    In the inspector, set rigidbody interpolation to interpolate.
    //    Make sure you follow all these steps carefully. I've made rigidbody controllers many times and i was recently finally able to fix all the jitter

    private void LateUpdate()
    {
        if (followPlayer)
        {
            mainCam.transform.parent = null;
            if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.Off)
                mainCam.transform.position = _playerCameraHolder.position;
            horizontalAxisTarget = mainCam.transform;



            if (player.isMine && !pController.pauseMenuOpen && _blockTime <= 0)
            {
                if (player.allPlayerScripts.scoreboardManager.scoreboardOpen && !CurrentRoomManager.instance.gameOver) return;


                xAxisInput = rewiredPlayer.GetAxis("Mouse X");
                yAxisInput = rewiredPlayer.GetAxis("Mouse Y");

                _correctedXAxisInput = xAxisInput; _correctedYAxisInput = yAxisInput;

                if (_controllerType == ControllerType.Joystick) // stick drift
                {
                    if (Mathf.Abs(xAxisInput) <= 0.1f) _correctedXAxisInput = 0;
                    if (Mathf.Abs(yAxisInput) <= 0.1f) _correctedYAxisInput = 0;
                    _correctedYAxisInput *= 0.75f;
                }

                // turning
                if (Mathf.Abs(xAxisInput) >= xExtremeDeadzone) _correctedXAxisInput *= 1.6f;
                if (Mathf.Abs(yAxisInput) >= yExtremeDeadzone) _correctedYAxisInput *= 1.6f;

                mouseX = _correctedXAxisInput * backEndMouseSens * Time.deltaTime;
                mouseY = _correctedYAxisInput * backEndMouseSens * 0.75f * Time.deltaTime;


                leftRightRotation += mouseX;
                upDownRotation -= mouseY;
                upDownRotation = Mathf.Clamp(upDownRotation, minXClamp, maxXClamp);













                if (pController.cameraIsFloating)
                {
                    leftRightRotation = upDownRotation = 0;
                    _inventoryGo.transform.localRotation = Quaternion.Euler(0, 0, 0);

                }
                // PROCESS PLAYER INPUT
                else if (player.isAlive && !pController.cameraIsFloating)
                {
                    if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On
                        || (player.playerInventory.activeWeapon && player.playerInventory.activeWeapon.weaponType == WeaponProperties.WeaponType.Heavy))
                    {
                        _thirdPersonCameraPivot.transform.localRotation = Quaternion.Euler(upDownRotation, leftRightRotation, 0f);
                        _inventoryGo.transform.localRotation = Quaternion.Euler(upDownRotation, 0, 0f);
                        mainCam.transform.forward = _thirdPersonCameraPivot.transform.forward;
                        mainCam.transform.position = _thirdPersonCameraTarget.position;


                        var targetHorizontalAngle = Mathf.SmoothDampAngle(player.GetComponent<Rigidbody>().rotation.eulerAngles.y,
                                                       mainCam.transform.eulerAngles.y,
                                                       ref _tmpRotationVelocity,
                                                       _rotationSmoothTime,
                                                       float.MaxValue,
                                                       Time.fixedDeltaTime);
                        Quaternion targetRotation = Quaternion.Euler(0.0F, targetHorizontalAngle, 0.0F);
                        player.GetComponent<Rigidbody>().MoveRotation(targetRotation);
                    }
                    else if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.Off)
                    {
                        mainCam.transform.localRotation = Quaternion.Euler(upDownRotation, leftRightRotation, 0f);
                        _inventoryGo.transform.localRotation = Quaternion.Euler(upDownRotation, 0, 0f);


                        var targetHorizontalAngle = Mathf.SmoothDampAngle(player.GetComponent<Rigidbody>().rotation.eulerAngles.y,
                                                       mainCam.transform.eulerAngles.y,
                                                       ref _tmpRotationVelocity,
                                                       _rotationSmoothTime,
                                                       float.MaxValue,
                                                       Time.fixedDeltaTime);
                        Quaternion targetRotation = Quaternion.Euler(0.0F, targetHorizontalAngle, 0.0F);
                        player.GetComponent<Rigidbody>().MoveRotation(targetRotation);
                    }




                    //var localTarget = transform.InverseTransformPoint(_rigidBodyMoveRotationTarget.transform.position);
                    //float angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
                    //Vector3 eulerAngleVelocity = new Vector3(0, angle, 0);
                    //Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime);
                    //player.GetComponent<Rigidbody>().MoveRotation(player.GetComponent<Rigidbody>().rotation * deltaRotation);


                    return;
                    verticalAxisTarget.Rotate(Vector3.left * mouseY); // NEW, multiple scripts can now interact with local rotation
                                                                      //localeulerangles return over 300 when looking up and returns below 90 when looking down
                    mainCam.transform.Rotate(Vector3.left * mouseY);



                    _trueLocalX = 0;


                    if (verticalAxisTarget.localEulerAngles.x > 0 && verticalAxisTarget.localEulerAngles.x < 180) _trueLocalX -= verticalAxisTarget.localEulerAngles.x;
                    if (verticalAxisTarget.localEulerAngles.x > 180 && verticalAxisTarget.localEulerAngles.x <= 360) _trueLocalX = 360 - verticalAxisTarget.localEulerAngles.x;

                    _trueLocalX *= -1;

                    if (_trueLocalX > maxXClamp) verticalAxisTarget.localRotation = Quaternion.Euler(new Vector3(maxXClamp, verticalAxisTarget.localRotation.y, verticalAxisTarget.localRotation.z));
                    else if (_trueLocalX < minXClamp) verticalAxisTarget.localRotation = Quaternion.Euler(new Vector3(minXClamp, verticalAxisTarget.localRotation.y, verticalAxisTarget.localRotation.z));



                    if (horizontalAxisTarget.transform.root == horizontalAxisTarget)
                        horizontalAxisTarget.Rotate(Vector3.up * mouseX);
                    else
                        horizontalAxisTarget.localRotation = Quaternion.Euler(0, -leftRightRotation, 0f);

                }
            }
        }
    }

    public void RotateCameraToRotation(Vector3 dirr)
    {
        //if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.Off)
        {
            mainCam.transform.parent = null;
            mainCam.transform.position = _playerCameraHolder.position;
            mainCam.transform.localRotation = Quaternion.identity;

            print($"PlayerCamera: {dirr} | {mainCam.transform.forward} {Vector3.Angle(dirr, mainCam.transform.forward)}");
            print($"PlayerCamera: {dirr} | {mainCam.transform.forward} {Vector3.SignedAngle(mainCam.transform.forward, dirr, mainCam.transform.up)}");

            //leftRightRotation = Vector3.Angle(dirr, mainCam.transform.forward);
            leftRightRotation = Vector3.SignedAngle(mainCam.transform.forward, dirr, mainCam.transform.up);
        }
    }

    void WeaponOffset()
    {
        if (player.isMine && !player.isAlive || !player.movement.isGrounded)
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
        _weaponOffsetLocalPosition.y = -0.15f;
        _weaponOffset.localPosition = _weaponOffsetLocalPosition;
        _weaponOffset.localRotation = Quaternion.Euler(_weaponOffset.localRotation.x, _offsetTickUpRotation * 0.1f, _weaponOffset.localRotation.z);
    }

    void OnPlayerIdAndRewiredIdAssigned_Delegate(Player p)
    {
        frontEndMouseSens = player.playerDataCell.sens;
    }


    void OnCameraSensitivityChanged()
    {
        //frontEndMouseSens = GameManager.instance.camSens;
    }

    void OnDeath_Delegate(Player p)
    {
        followPlayer = false;
    }

    void OnRespawnEarly_Delegate(Player p)
    {
        //upDownRotation = leftRightRotation = 0;
        upDownRotation = 0;
        ragdollPrefab = null;

        {
            // camera is child of ragdoll. This returns the script go to player go
            player.mainCamera.transform.parent = player.mainCamera.GetComponent<PlayerCameraSplitScreenBehaviour>().orignalParent;

            this.transform.parent = playerCameraScriptParent;
            horizontalAxisTarget = player.transform;
            verticalAxisTarget = player.playerInventory.transform;

            verticalAxisTarget.localRotation = Quaternion.Euler(0, 0, 0f);
        }

        followPlayer = true;

        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On) EnableThirdPersonLayerMask();
    }


    public void BlockPlayerCamera(float t)
    {
        _blockTime = t;
    }

    public void AddToLeftRightRotation(float a)
    {
        if (player.isAlive)
            leftRightRotation += a;
    }

    public void AddToUpDownRotation(float a)
    {
        if (player.isAlive)
            upDownRotation += a;
    }



    public void EnableThirdPersonLayerMask()
    {
        print("EnableThirdPersonLayerMask");
        mainCam.cullingMask = GameManager.instance.thirdPersonMainCameraLayerMask;
    }

    public void SetupThirdPersonCamera()
    {
        print("SetupThirdPersonCamera");
        _thirdPersonCameraPivot.transform.parent = null;
        _thirdPersonAimingComponentsOffset.transform.localPosition = new Vector3(0, 0, 2.4f);
    }

    public void DisableThirdPersonCameraMode()
    {
        _thirdPersonCameraPivot.SetParent(player.mainCamera.transform); _thirdPersonCameraPivot.localPosition = Vector3.zero; _thirdPersonCameraPivot.localRotation = Quaternion.identity;
        _thirdPersonAimingComponentsOffset.localPosition = Vector3.zero; _thirdPersonAimingComponentsOffset.localRotation = Quaternion.identity;

    }
}
