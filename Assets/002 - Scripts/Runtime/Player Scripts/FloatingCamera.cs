using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;


// REFERENCE: https://stackoverflow.com/questions/58328209/how-to-make-a-free-fly-camera-script-in-unity-with-acceleration-and-decceleratio
public class FloatingCamera : MonoBehaviour
{
    public PlayerController playerController;
    public int counter
    {
        get { return _counter; }
        set
        {
            // 0 is always null and is initialised in GameManager on scene 0 loaded
            _counter = value;

            if (_counter == -1)
            {
                _counter = GameManager.instance.gameplayRecorderPoints.Count - 1;
            }
            else if (_counter >= GameManager.instance.gameplayRecorderPoints.Count)
            {
                _counter = 0;
            }

            if (_counter == 0)
            {
                transform.parent = null;
            }
            else
            {
                transform.parent = GameManager.instance.gameplayRecorderPoints[_counter].transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
    }



    public float MaximumMovementSpeed
    {
        get
        {
            if (playerController.activeControllerType != Rewired.ControllerType.Joystick)
            {
                if (playerController.rewiredPlayer.GetButton("Sprint")) return _maximumMovementSpeed * 4;
                else if (playerController.rewiredPlayer.GetButton("Crouch")) return _maximumMovementSpeed / 4;
            }
            else
            {
                if (playerController.rewiredPlayer.GetButton("Shoot")) return _maximumMovementSpeed * 4;
                else if (playerController.rewiredPlayer.GetButton("Throw Grenade")) return _maximumMovementSpeed / 4;
            }


            return _maximumMovementSpeed;
        }
    }


    [Header("Constants")]

    //unity controls and constants input
    public float AccelerationMod;
    public float XAxisSensitivity;
    public float YAxisSensitivity;
    public float DecelerationMod;
    [SerializeField] float _maximumMovementSpeed;

    [Space]

    [Range(0, 89)] public float MaxXAngle = 60f;



    [Header("Controls")]

    public KeyCode Forwards = KeyCode.W;
    public KeyCode Backwards = KeyCode.S;
    public KeyCode Left = KeyCode.A;
    public KeyCode Right = KeyCode.D;
    public KeyCode Up = KeyCode.Q;
    public KeyCode Down = KeyCode.E;

    [SerializeField] Vector3 _moveSpeed, _acceleration;


    float _changeCameraCd, _rotationX;
    [SerializeField] int _counter, _zoomCounter;

    Camera _cam;



    private void Awake()
    {
        if (GameManager.instance.flyingCameraMode == GameManager.FlyingCamera.Disabled) this.enabled = false;

        _changeCameraCd = 0.5f;
        _cam = GetComponent<Camera>();
        _maximumMovementSpeed = 0.05f;
        XAxisSensitivity = YAxisSensitivity = 2;
    }



    private void Start()
    {
        _moveSpeed = Vector3.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        if (GameManager.instance.flyingCameraMode == GameManager.FlyingCamera.Enabled)
        {
            HandleMouseRotation();

            if (!playerController.cameraIsFloating || !playerController.player.isMine) return;

            _acceleration = Vector3.zero;

            _acceleration.z = playerController.rewiredPlayer.GetAxis("Move Vertical");
            _acceleration.x = playerController.rewiredPlayer.GetAxis("Move Horizontal");
            _acceleration.y = 0;

            if (playerController.activeControllerType != Rewired.ControllerType.Joystick)
            {
                if (playerController.rewiredPlayer.GetButton("Melee")) _acceleration.y = -0.5f;
                else if (playerController.rewiredPlayer.GetButton("Interact")) _acceleration.y = 0.5f;

                if (playerController.rewiredPlayer.GetButton("Sprint")) _acceleration *= 3;
            }
            else
            {
                if (playerController.rewiredPlayer.GetButton("Jump")) _acceleration.y = -0.5f;
                else if (playerController.rewiredPlayer.GetButton("Melee")) _acceleration.y = 0.5f;

                if (playerController.rewiredPlayer.GetButton("Shoot")) _acceleration *= 3;
            }


            if (!playerController.cameraIsFloating || counter != 0) return;


            _moveSpeed += _acceleration;

            HandleDeceleration(_acceleration);

            // clamp the move speed
            if (_moveSpeed.magnitude > MaximumMovementSpeed) _moveSpeed = _moveSpeed.normalized * MaximumMovementSpeed;

            transform.Translate(_moveSpeed);
        }
    }










    private void HandleMouseRotation()
    {
        //mouse input
        var rotationHorizontal = playerController.rewiredPlayer.GetAxis("Mouse X") * 2;
        var rotationVertical = playerController.rewiredPlayer.GetAxis("Mouse Y") * 2;

        if (playerController.activeControllerType != Rewired.ControllerType.Joystick)
        {
            rotationHorizontal /= 10;
            rotationVertical /= 10;
        }


        //applying mouse rotation
        // always rotate Y in global world space to avoid gimbal lock
        if (!playerController.pauseMenuOpen) transform.Rotate(Vector3.up * rotationHorizontal, Space.World);

        var rotationY = transform.localEulerAngles.y;

        _rotationX += rotationVertical;
        _rotationX = Mathf.Clamp(_rotationX, -MaxXAngle, MaxXAngle);

        if (!playerController.pauseMenuOpen) transform.localEulerAngles = new Vector3(-_rotationX, rotationY, 0);
    }

    private void HandleDeceleration(Vector3 acceleration)
    {
        //deceleration functionality
        if (Mathf.Approximately(Mathf.Abs(acceleration.x), 0))
        {
            if (Mathf.Abs(_moveSpeed.x) < DecelerationMod)
            {
                _moveSpeed.x = 0;
            }
            else
            {
                _moveSpeed.x -= DecelerationMod * Mathf.Sign(_moveSpeed.x);
            }
        }

        if (Mathf.Approximately(Mathf.Abs(acceleration.y), 0))
        {
            if (Mathf.Abs(_moveSpeed.y) < DecelerationMod)
            {
                _moveSpeed.y = 0;
            }
            else
            {
                _moveSpeed.y -= DecelerationMod * Mathf.Sign(_moveSpeed.y);
            }
        }

        if (Mathf.Approximately(Mathf.Abs(acceleration.z), 0))
        {
            if (Mathf.Abs(_moveSpeed.z) < DecelerationMod)
            {
                _moveSpeed.z = 0;
            }
            else
            {
                _moveSpeed.z -= DecelerationMod * Mathf.Sign(_moveSpeed.z);
            }
        }
    }
}
