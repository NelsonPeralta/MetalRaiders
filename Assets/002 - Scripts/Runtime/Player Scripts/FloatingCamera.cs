using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// REFERENCE: https://stackoverflow.com/questions/58328209/how-to-make-a-free-fly-camera-script-in-unity-with-acceleration-and-decceleratio
public class FloatingCamera : MonoBehaviour
{
    public PlayerController playerController;
    public int counter
    {
        get { return _counter; }
        set
        {
            if (_counter == 99)
            {
                _counter = 0;

                transform.parent = GameManager.instance.gameplayRecorderPoints[_counter].transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                _counter = value;

                if (_counter > GameManager.instance.gameplayRecorderPoints.Count - 1)
                {
                    _counter = 99;
                }
                else
                {
                    transform.parent = GameManager.instance.gameplayRecorderPoints[_counter].transform;
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                }
            }
        }
    }


    private int zoomCounter
    {
        get { return _zoomCounter; }
        set
        {
            _zoomCounter = Mathf.Clamp(value, 0, GameManager.DEFAULT_FRAMERATE / 2);
        }
    }



    [Header("Constants")]

    //unity controls and constants input
    public float AccelerationMod;
    public float XAxisSensitivity;
    public float YAxisSensitivity;
    public float DecelerationMod;

    [Space]

    [Range(0, 89)] public float MaxXAngle = 60f;

    [Space]

    public float MaximumMovementSpeed = 0.7f;

    [Header("Controls")]

    public KeyCode Forwards = KeyCode.W;
    public KeyCode Backwards = KeyCode.S;
    public KeyCode Left = KeyCode.A;
    public KeyCode Right = KeyCode.D;
    public KeyCode Up = KeyCode.Q;
    public KeyCode Down = KeyCode.E;

    private Vector3 _moveSpeed;


    float _changeCameraCd;
    [SerializeField] int _counter, _zoomCounter;

    Camera _cam;



    private void Awake()
    {
        _changeCameraCd = 0.5f;
        _cam = GetComponent<Camera>();
    }



    private void Start()
    {
        _moveSpeed = Vector3.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!playerController.cameraIsFloating) return;

        if (playerController.rewiredPlayer.GetButton("Aim")) { zoomCounter += 2; } else zoomCounter -= 2;

        _cam.fieldOfView = 60 - zoomCounter;
        //var acceleration = HandleKeyInput();
        var acceleration = Vector3.zero;

        acceleration.z = playerController.rewiredPlayer.GetAxis("Move Vertical");
        acceleration.x = playerController.rewiredPlayer.GetAxis("Move Horizontal");


        if (playerController.rewiredPlayer.GetButton("Sprint")) acceleration *= 3;

        if (!playerController.cameraIsFloating || counter != 99) return;

        HandleMouseRotation();

        _moveSpeed += acceleration;

        HandleDeceleration(acceleration);

        // clamp the move speed
        if (_moveSpeed.magnitude > MaximumMovementSpeed)
        {
            _moveSpeed = _moveSpeed.normalized * MaximumMovementSpeed;
        }

        transform.Translate(_moveSpeed);
    }

    private Vector3 HandleKeyInput()
    {
        var acceleration = Vector3.zero;

        //key input detection
        if (Input.GetKey(Forwards))
        {
            acceleration.z += 0.3f;
        }

        if (Input.GetKey(Backwards))
        {
            acceleration.z -= 0.3f;
        }

        if (Input.GetKey(Left))
        {
            acceleration.x -= 0.3f;
        }

        if (Input.GetKey(Right))
        {
            acceleration.x += 0.3f;
        }

        if (Input.GetKey(Up))
        {
            acceleration.y += 0.3f;
        }

        if (Input.GetKey(Down))
        {
            acceleration.y -= 0.3f;
        }

        return acceleration.normalized * AccelerationMod;
    }

    private float _rotationX;

    private void HandleMouseRotation()
    {
        //mouse input
        var rotationHorizontal = playerController.rewiredPlayer.GetAxis("Mouse X") / 2;
        var rotationVertical = playerController.rewiredPlayer.GetAxis("Mouse Y") / 2;

        //applying mouse rotation
        // always rotate Y in global world space to avoid gimbal lock
        transform.Rotate(Vector3.up * rotationHorizontal, Space.World);

        var rotationY = transform.localEulerAngles.y;

        _rotationX += rotationVertical;
        _rotationX = Mathf.Clamp(_rotationX, -MaxXAngle, MaxXAngle);

        transform.localEulerAngles = new Vector3(-_rotationX, rotationY, 0);
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
