using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class NewMovement : MonoBehaviour
{
    public delegate void PlayerMovementEvent(NewMovement movement);
    public PlayerMovementEvent OnPlayerStartedMoving, OnPlayerStoppedMoving;
    public enum PlayerMovementDirection { Idle, Left, Right, Forward, Backwards, ForwardLeft, ForwardRight, BackwardsLeft, BackwardsRight }





    public bool isOnLadder { get { return _isOnLadder; } set { _isOnLadder = value; } }
    public bool canMoveWhileJumping
    {
        get { return _canMoveWhileJumping; }
        set
        {
            if (value != _canMoveWhileJumping)
            {
                if (!value)
                {
                    _canMoveWhileJumpingCooldown = 0.5f;
                    _canMoveWhileJumping = value;
                }
                else if (value && _canMoveWhileJumpingCooldown <= 0)
                    _canMoveWhileJumping = value;

                Debug.Log($"Can move while jumping: {canMoveWhileJumping}");
            }
        }
    }
    public bool isGrounded
    {
        get { return _groundCheckScript.isGrounded; }
        set
        {
            if (value != _isGrounded)
            {
                _isGrounded = value;
                canMoveWhileJumping = true;
            }
        }
    }
    public bool isMoving
    {
        get { return _isMoving; }
        private set
        {
            if (value && !_isMoving)
            {
                _isMoving = true;
                OnPlayerStartedMoving?.Invoke(this);
            }
            else if (!value && _isMoving)
            {
                _isMoving = false;
                OnPlayerStoppedMoving?.Invoke(this);
            }
        }
    }

    public int directionIndicator
    {
        get { return _directionIndicator; }
        set
        {
            _directionIndicator = value;

            if (value == 0)
                movementDirection = PlayerMovementDirection.Idle;
            else if (value == 1)
                movementDirection = PlayerMovementDirection.Left;
            else if (value == 2)
                movementDirection = PlayerMovementDirection.ForwardLeft;
            else if (value == 3)
                movementDirection = PlayerMovementDirection.Forward;
            else if (value == 4)
                movementDirection = PlayerMovementDirection.ForwardRight;
            else if (value == 5)
                movementDirection = PlayerMovementDirection.Right;
            else if (value == 6)
                movementDirection = PlayerMovementDirection.BackwardsRight;
            else if (value == 7)
                movementDirection = PlayerMovementDirection.Backwards;
            else if (value == 8)
                movementDirection = PlayerMovementDirection.BackwardsLeft;

        }
    }

    public float currentMaxSpeed { get { return _currentMaxSpeed; } set { _currentMaxSpeed = value; } }
    public float defaultMaxSpeed { get { return _defaultMaxSpeed; } }
    public float jumpForce { get { return _jumpForce; } }
    public float manCannonCooldown
    {
        get { return _manCannonCooldown; }
        set
        {
            Debug.Log("Man Cannon Cooldown");
            _manCannonCooldown = 0.5f;
        }
    }
    public float speedPercent { get { return _speedRatio; } }
    public float correctedXInput { get { return _correctedRightInput; } set { _correctedRightInput = value; } }
    public float correctedZInput { get { return _correctedForwardInput; } set { _correctedForwardInput = value; } }

    public Vector3 velocity { get { return _verticalVector; } set { _verticalVector = value; } }

    public Rewired.Player rewiredPlayer { get { return _rewiredPlayer; } }
    public PlayerMovementDirection movementDirection { get { return _playerMovementDirection; } private set { _playerMovementDirection = value; } }




    [SerializeField] ThirdPersonLookAt _tpLookAt;
    [SerializeField] NewGroundCheck _groundCheckScript;
    [SerializeField] NewGroundCheck _roofCheckScript;
    [SerializeField] LayerMask _groundMask;

    [SerializeField] float _rawRightInput, _rawForwardInput;
    [SerializeField] float _correctedRightInput, _correctedForwardInput;

    [SerializeField] Vector3 _movementVect, _verticalVector, _calulatedVelocity;

    [SerializeField] float _defaultMaxSpeed, _currentMaxSpeed, _currentSpeed, _speedRatio, _jumpForce = 8f, defaultGravity = -13;
    [SerializeField] float _maxRightSpeed, _maxForwardSpeed;
    [SerializeField] float _correctedRightSpeed, _correctedForwardSpeed;
    [SerializeField] float _acceleration = 7f, _deceleration = 7f;






    Player _player;
    CharacterController _cController;
    PlayerController _pController;
    Rewired.Player _rewiredPlayer;

    bool _canMoveWhileJumping, _isGrounded, _isMoving, _isOnLadder;
    float _defaultSlopeLimit, _defaultStepOffset, _canMoveWhileJumpingCooldown, _manCannonCooldown = 0.5f,
        _rightDeadzone = 0.2f, _forwardDeadzone = 0.2f,
        _defaultTestMaxSpeed = 4f, _currentGravity = -9.81f;
    int _directionIndicator = 0, _terminalVelocity = -200;
    PlayerMovementDirection _playerMovementDirection;
    Vector3 _lastPos;






    ////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 
    /// </summary>
    /// 
    // Start is called before the first frame update
    void Start()
    {
        _currentMaxSpeed = defaultMaxSpeed;
        _cController = GetComponent<CharacterController>();
        _rewiredPlayer = ReInput.players.GetPlayer(0);
        _verticalVector = new Vector3(0, defaultGravity, 0);
    }

    // Update is called once per frame
    void Update()
    {
        CalculateCurrentSpeed();
        CalculateCorrectedSpeed();
        Jump();

        _verticalVector.y += Mathf.Clamp(defaultGravity * Time.deltaTime, _terminalVelocity, 100);
        if (_verticalVector.y < 0 && isGrounded)
            _verticalVector.y = 0;


        _rawRightInput = _correctedRightInput = rewiredPlayer.GetAxis("Move Horizontal");
        _rawForwardInput = _correctedForwardInput = rewiredPlayer.GetAxis("Move Vertical");

        if (Mathf.Abs(_correctedRightInput) <= _rightDeadzone) _correctedRightInput = 0;
        if (Mathf.Abs(_correctedForwardInput) <= _forwardDeadzone) _correctedForwardInput = 0;

        _maxRightSpeed = Mathf.Abs(_correctedRightInput * _defaultMaxSpeed);
        _maxForwardSpeed = Mathf.Abs(_correctedForwardInput * _defaultMaxSpeed);


        Vector3 currentMovementInput = (transform.forward * _correctedForwardInput + transform.right * -_correctedRightInput);
        Vector3 motion = ((transform.forward * Mathf.Abs(_correctedForwardInput) * _correctedForwardSpeed) +
            (transform.right * Mathf.Abs(_correctedRightInput) * _correctedRightSpeed));

        _cController.Move(_verticalVector * Time.deltaTime);
        _cController.Move(motion * Time.deltaTime);

        //_cController.Move(currentMovementInput * currentMaxSpeed * Time.deltaTime);
    }

    void Jump()
    {
        bool _isGrounded = _groundCheckScript.isGrounded;

        if (isGrounded && rewiredPlayer.GetButtonDown("Jump"))
        {
            float _jumpForce = jumpForce;

            _verticalVector.y = _jumpForce;
        }
    }

    void CalculateCurrentSpeed()
    {
        _currentSpeed = ((transform.position - _lastPos).magnitude / Time.deltaTime);
        _currentSpeed = Mathf.Clamp(Mathf.Round(_currentSpeed * 10f) / 10f, 0, _currentMaxSpeed);
        _lastPos = transform.position;

        CalculateSpeedRatio();
    }

    void CalculateSpeedRatio()
    {
        _speedRatio = Mathf.Clamp(Mathf.Round((_currentSpeed / _currentMaxSpeed) * 10f) / 10f, 0, 1);
    }
    void CalculateCorrectedSpeed()
    {
        if (_correctedRightInput < 0 && (_correctedRightSpeed < _maxRightSpeed))
        {
            //if(_correctedRightSpeed > 0)
            //    _correctedRightSpeed = 0;
            _correctedRightSpeed = Mathf.Clamp(_correctedRightSpeed - _acceleration * Time.deltaTime, -_maxRightSpeed, _maxRightSpeed);
        }
        else if (_correctedRightInput > 0 && (_correctedRightSpeed > -_maxRightSpeed))
        {
            //if (_correctedRightSpeed < 0)
            //    _correctedRightSpeed = 0;
            _correctedRightSpeed = Mathf.Clamp(_correctedRightSpeed + _acceleration * Time.deltaTime, -_maxRightSpeed, _maxRightSpeed);
        }
        else if (_correctedRightInput == 0)
        {
            if (_correctedRightSpeed > _deceleration * Time.deltaTime)
                _correctedRightSpeed = _correctedRightSpeed - _deceleration * Time.deltaTime;
            else if (_correctedRightSpeed < -_deceleration * Time.deltaTime)
                _correctedRightSpeed = _correctedRightSpeed + _deceleration * Time.deltaTime;
            else
                _correctedRightSpeed = 0;
        }

        if (_correctedForwardInput < 0 && (_correctedForwardSpeed < _maxForwardSpeed))
            _correctedForwardSpeed = Mathf.Clamp(_correctedForwardSpeed - _acceleration * Time.deltaTime, -_maxForwardSpeed, _maxForwardSpeed);
        else if (_correctedForwardInput > 0 && (_correctedForwardSpeed > -_maxForwardSpeed))
            _correctedForwardSpeed = Mathf.Clamp(_correctedForwardSpeed + _acceleration * Time.deltaTime, -_maxForwardSpeed, _maxForwardSpeed);
        else if (_correctedForwardInput == 0)
        {
            if (_correctedForwardSpeed > _deceleration * Time.deltaTime)
                _correctedForwardSpeed = _correctedForwardSpeed - _deceleration * Time.deltaTime;
            else if (_correctedForwardSpeed < -_deceleration * Time.deltaTime)
                _correctedForwardSpeed = _correctedForwardSpeed + _deceleration * Time.deltaTime;
            else
                _correctedForwardSpeed = 0;
        }
    }
}
