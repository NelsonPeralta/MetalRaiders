using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Photon.Pun;

public class Movement : MonoBehaviour, IMoveable
{
    public delegate void PlayerMovementEvent(Movement _movementVect);
    public PlayerMovementEvent OnPlayerStartedMoving, OnPlayerStoppedMoving;
    public enum PlayerMovementDirection { Idle, Left, Right, Forward, Backwards, ForwardLeft, ForwardRight, BackwardsLeft, BackwardsRight }


    [SerializeField]
    Vector3 jumpMovementCorrector;














    public Player player { get { return _player; } }
    public LayerMask groundMask { get { return _groundMask; } }
    public Rewired.Player _rewiredplayer { get { return _pController.rewiredPlayer; } }

    public bool isOnLadder { get { return _isOnLadder; } set { _isOnLadder = value; } }
    public bool canMove
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

                Debug.Log($"Can move while jumping: {canMove}");
            }
        }
    }
    public bool isGrounded
    {
        get
        {
            return _groundCheckScript.isGrounded;
        }
        set
        {
            if (value != _isGrounded)
            {
                _isGrounded = value;
                canMove = true;
            }
        }
    }
    public bool isMoving
    {
        get { return _isMoving; }
        private set
        {
            if (value != _isMoving)
            {
                if (value)
                {
                    _isMoving = true;
                    OnPlayerStartedMoving?.Invoke(this);
                }
                else
                {
                    _isMoving = false;
                    OnPlayerStoppedMoving?.Invoke(this);
                }
            }
        }
    }

    public float currentSpeed
    {
        get { return _currentSpeed; }
        private set
        {
            _currentSpeed = value;
            if (value <= 0.15 * currentMaxSpeed)
                isMoving = false;
            else
                isMoving = true;
        }
    }
    public float currentMaxSpeed
    {
        get { return _currentMaxSpeed; }
        set
        {
            _currentMaxSpeed = value;
            if (_pController.isCrouching)
                _currentMaxSpeed = defaultMaxSpeed * 0.5f;

            if (_pController.isSprinting)
                _currentMaxSpeed = defaultMaxSpeed * 1.33f;
        }
    }
    public float defaultMaxSpeed { get { return _defaultMaxSpeed; } }
    public float jumpForce { get { return _jumpForce; } }
    public float blockMovementCooldown
    {
        get { return _blockMovementCooldown; }
        set
        {
            Debug.Log("Man Cannon Cooldown");
            _blockMovementCooldown = value;
        }
    }
    public float speedRatio { get { return _speedRatio; } }
    public float correctedXInput { get { return _correctedRightInput; } set { _correctedRightInput = value; } }
    public float correctedZInput { get { return _correctedForwardInput; } set { _correctedForwardInput = value; } }

    public Vector3 verticalVector { get { return _verticalVector; } set { _verticalVector = value; } }

    public Rewired.Player rewiredPlayer { get { if (_rewiredPlayer == null) _rewiredPlayer = _pController.rewiredPlayer; return _rewiredPlayer; } }
    public PlayerMovementDirection movementDirection { get { return _playerMovementDirection; } private set { _playerMovementDirection = value; } }
    public PlayerMotionTracker playerMotionTracker { get { return _playerMotionTracker; } }
    public PlayerImpactReceiver playerImpactReceiver { get { return _playerImpactReceiver; } }

    public float animationSpeed
    {
        get
        {
            _animationSpeed = 0;
            int _c = 0;

            if (correctedXInput != 0) { _c++; _animationSpeed += Mathf.Abs(correctedXInput); }
            if (correctedZInput != 0) { _c++; _animationSpeed += Mathf.Abs(correctedZInput); }

            if (_c > 0)
                _animationSpeed /= _c;

            Mathf.Clamp(_c * 1.3f, _c, 1);

            return Mathf.Abs(_animationSpeed);
        }
    }


    [SerializeField] PlayerMotionTracker _playerMotionTracker;
    [SerializeField] ThirdPersonScript _thirdPersonScript;
    [SerializeField] ThirdPersonLookAt _tpLookAt;
    [SerializeField] GroundCheck _groundCheckScript;
    [SerializeField] GroundCheck _roofCheckScript;
    [SerializeField] GroundCheck _edgeCheck;
    [SerializeField] LayerMask _groundMask;

    [SerializeField] float _rawRightInput, _rawForwardInput;
    [SerializeField] float _correctedRightInput, _correctedForwardInput;

    [SerializeField] Vector3 _direction, _movementInput, _verticalVector, _calulatedVelocity;

    [SerializeField] float _defaultMaxSpeed, _currentMaxSpeed, _currentSpeed, _speedRatio, _jumpForce = 8f, defaultGravity = -13;
    [SerializeField] float _maxRightSpeed, _maxForwardSpeed;
    [SerializeField] float _correctedRightSpeed, _correctedForwardSpeed;
    [SerializeField] float _acceleration = 7f, _deceleration = 7f;
    [SerializeField] float _animationSpeed;
    [SerializeField] PlayerMovementDirection _playerMovementDirection;
    [SerializeField] float _blockMovementCooldown;




    Player _player;
    CharacterController _cController;
    PlayerController _pController;
    PlayerImpactReceiver _playerImpactReceiver;
    Rewired.Player _rewiredPlayer;

    bool _canMoveWhileJumping, _isGrounded, _isMoving, _isOnLadder;
    float _defaultSlopeLimit, _defaultStepOffset, _canMoveWhileJumpingCooldown,
        _rightDeadzone = 0.2f, _forwardDeadzone = 0.2f,
        _defaultTestMaxSpeed = 4f, _currentGravity = -9.81f;
    float defaultSlopeLimit, defaultStepOffset, _crouchJumpTime = 0.2f, crouchJumpTime = 0.2f, _lastCalulatedGroundedSpeed;
    int _terminalVelocity = -200;
    Vector3 _lastPos;





















    private void OnEnable()
    {
        _blockMovementCooldown = 0;
        _canMoveWhileJumpingCooldown = 0;
    }

    private void Awake()
    {
        _player = GetComponent<Player>();
        _cController = GetComponent<CharacterController>();
        _pController = GetComponent<PlayerController>();
        _playerImpactReceiver = GetComponent<PlayerImpactReceiver>();

        _currentMaxSpeed = defaultMaxSpeed;
        _rewiredPlayer = _pController.rewiredPlayer;
        _verticalVector = new Vector3(0, defaultGravity, 0);
    }
    void Start()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            _defaultMaxSpeed *= 0.7f;

        _cController = gameObject.GetComponent<CharacterController>();
        _pController = gameObject.GetComponent<PlayerController>();
        defaultSlopeLimit = GetComponent<CharacterController>().slopeLimit;
        defaultStepOffset = GetComponent<CharacterController>().stepOffset;
    }

    // IMPORTANT
    // Update() check for input
    // FixedUpdate() move gameObjects that have riggidbody/apply forces
    // LateUpdate() move Camera

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.gameStarted) return;
        if (!_pController.PV.IsMine) return;
        if (_player.isDead || _player.isRespawning)
        {
            _correctedRightInput = _correctedForwardInput = _maxRightSpeed = _maxForwardSpeed = _correctedRightSpeed = _correctedForwardSpeed = 0;

            _movementInput = _verticalVector = _direction = Vector3.zero;
            return;
        }


        CheckIfStuckInEdge();

        CalculateCrouchSpeed(); CalculateSprintSpeed(); ManCannonJumpCooldwon(); CalculateCurrentSpeed();

        ApplyGravityOnGravityVector();

        Jump(); CalculateInput(); CalculateCorrectedDirectionSpeeds();



        CheckDirection(_rawRightInput, _rawForwardInput);
        WalkAnimation();
        LadderMaxSpeedChange();
        ControlAnimationSpeed();

        CrouchJump();

        ApplyResidualMovementWhileNotGrounded();
        ApplyInAirMovement();

        if (!_pController.cameraisFloating)
            ApplyMovement();
    }



    float noSlipDistance = .05f, edgeFallFactor = 5;
    private void LateUpdate()
    {
        //RaycastHit hitInfo;
        //if (Physics.SphereCast(transform.position + _cController.center, _cController.radius - _cController.skinWidth, Vector3.down, out hitInfo, _cController.height * 0.7f))
        //{
        //    Vector3 relativeHitPoint = hitInfo.point - (transform.position + _cController.center);
        //    float hitHeight = relativeHitPoint.y;
        //    relativeHitPoint.y = 0;
        //    if (relativeHitPoint.magnitude > noSlipDistance)
        //    {
        //        Vector3 edgeFallMovement = transform.position - hitInfo.point;
        //        edgeFallMovement.y = 0;
        //        _movementInput += (edgeFallMovement * Time.deltaTime * edgeFallFactor);
        //    }
        //}
    }


    float _edgePushCountdown;
    void CheckIfStuckInEdge()
    {
        if (_edgeCheck.touch && !isGrounded)
        {
            Debug.Log("Edge");
            Vector3 _dir = transform.position - _edgeCheck.touch.transform.position;
            _dir.y = 0;

            if (_edgePushCountdown > 0)
                _edgePushCountdown -= Time.deltaTime;

            if (_edgePushCountdown <= 0)
            {
                Push(_dir, 20, PushSource.Edge, true);
                _edgePushCountdown = 0.1f;
            }

            //_cController.Move(_dir * 10 * Time.deltaTime);
        }

    }
    void ApplyMovement()
    {
        // Movement
        #region
        if (!_player.isDead && !_player.isRespawning)
        {
            Vector3 currentMovementInput = transform.right * _correctedRightInput + transform.forward * _correctedForwardInput;
            currentMovementInput = transform.right * Mathf.Abs(_correctedRightInput) * _maxRightSpeed + transform.forward * Mathf.Abs(_correctedForwardInput) * _maxForwardSpeed;
            if (isGrounded && blockMovementCooldown <= 0)
            {

                _movementInput = transform.right * _rawRightInput + transform.forward * _rawForwardInput;
                if (!_pController.isCrouching)
                {
                    if (_pController.isSprinting)
                    {
                        currentMaxSpeed = 6;
                        Vector3 motion = ((transform.forward * Mathf.Abs(_correctedForwardInput) * _correctedForwardSpeed) +
            (transform.right * Mathf.Abs(correctedXInput) * _correctedRightSpeed));
                        _cController.Move(motion * Time.deltaTime);
                    }
                    else
                    {

                        Vector3 motion = ((transform.forward * Mathf.Abs(_correctedForwardInput) * _correctedForwardSpeed) +
            (transform.right * Mathf.Abs(_correctedRightInput) * _correctedRightSpeed));
                        _cController.Move(motion * Time.deltaTime);
                    }
                }
                else
                {
                    Vector3 motion = _movementInput = ((transform.forward * Mathf.Abs(_correctedForwardInput) * _correctedForwardSpeed) +
            (transform.right * Mathf.Abs(_correctedRightInput) * _correctedRightSpeed));
                    _cController.Move(_movementInput * Time.deltaTime);
                }
            }
            else if (!isGrounded && canMove)
            {
                currentMovementInput = transform.right * _correctedRightInput + transform.forward * _correctedForwardInput;

                if (Mathf.Sign(_movementInput.x) == Mathf.Sign(currentMovementInput.x))
                    currentMovementInput.x = 0;
                if (Mathf.Sign(_movementInput.z) == Mathf.Sign(currentMovementInput.z))
                    currentMovementInput.z = 0;

                _cController.Move(_movementInput * _currentMaxSpeed * Time.deltaTime);
                _cController.Move(_movementInput * 0.65f * _currentMaxSpeed * Time.deltaTime);
            }
        }


        _cController.Move(_verticalVector * Time.deltaTime);

        #endregion
    }









    void CalculateInput()
    {
        try
        {
            if (!_pController.pauseMenuOpen)
            {
                _rawRightInput = _correctedRightInput = rewiredPlayer.GetAxis("Move Horizontal");
                _rawForwardInput = _correctedForwardInput = rewiredPlayer.GetAxis("Move Vertical");

                if (Mathf.Abs(_correctedRightInput) <= _rightDeadzone) _correctedRightInput = 0;
                if (Mathf.Abs(_correctedForwardInput) <= _forwardDeadzone) _correctedForwardInput = 0;

                _maxRightSpeed = Mathf.Abs(_correctedRightInput * currentMaxSpeed);
                _maxForwardSpeed = Mathf.Abs(_correctedForwardInput * currentMaxSpeed);
            }
            else
            {
                _rawRightInput = 0;
                _rawForwardInput = 0;
            }
        }
        catch { }
    }
    void CalculateCurrentSpeed()
    {
        Vector3 curPos = transform.position; curPos.y = 0;
        Vector3 lasPos = _lastPos; lasPos.y = 0;

        currentSpeed = ((curPos - lasPos).magnitude / Time.deltaTime);
        currentSpeed = Mathf.Clamp(Mathf.Round(currentSpeed * 10f) / 10f, 0, _currentMaxSpeed);
        if (isGrounded)
            _lastCalulatedGroundedSpeed = currentSpeed;
        _lastPos = transform.position;
        CalculateSpeedRatio();
    }

    void CalculateSpeedRatio()
    {
        _speedRatio = Mathf.Clamp(Mathf.Round((currentSpeed / _currentMaxSpeed) * 10f) / 10f, 0, 1);
        if (_pController.pauseMenuOpen && isGrounded) _speedRatio = 1;
    }

    void CalculateCorrectedDirectionSpeeds()
    {
        if (!_pController.pauseMenuOpen)
        {
            if (_correctedRightInput < 0 && (_correctedRightSpeed < _maxRightSpeed))
            {
                _correctedRightSpeed = Mathf.Clamp(_correctedRightSpeed - _acceleration * Time.deltaTime, -_maxRightSpeed, _maxRightSpeed);
            }
            else if (_correctedRightInput > 0 && (_correctedRightSpeed > -_maxRightSpeed))
            {
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
        else
        {
            _correctedForwardInput = 0;
            _correctedRightInput = 0;
        }
    }














    void CalculateSprintSpeed()
    {
        if (_pController.isSprinting)
            currentMaxSpeed *= 2;
    }
    void CalculateCrouchSpeed()
    {
        if (_pController.isCrouching)
            currentMaxSpeed *= 0.5f;
    }

    void CrouchJump()
    {
        if (crouchJumpTime > 0)
            crouchJumpTime -= Time.deltaTime;

        if (!isGrounded && _rewiredplayer.GetButton("Crouch"))
        {
            if (crouchJumpTime <= 0)
                if (verticalVector.y > jumpForce * 0.6f)
                {
                    _verticalVector.y += 1;
                    crouchJumpTime = 1;
                    print("Courch Jump");
                }
        }


        //if (!isGrounded && _rewiredplayer.GetButton("Crouch"))
        //{
        //    crouchJumpTime -= Time.deltaTime;
        //    if (crouchJumpTime > 0)
        //    {
        //        _currentGravity = 0;
        //    }
        //}
        //else
        //{
        //    _currentGravity = defaultGravity;
        //}

        //if (_rewiredplayer.GetButtonUp("Crouch"))
        //{
        //    crouchJumpTime = _crouchJumpTime;
        //}
    }

    void Jump()
    {
        _thirdPersonScript = _pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel;

        if (isGrounded)
        {
            _thirdPersonScript.GetComponent<Animator>().SetBool("Jump", false);
            currentMaxSpeed = _defaultMaxSpeed;
        }
        else if (!isGrounded && _thirdPersonScript.GetComponent<Animator>() && !_thirdPersonScript.GetComponent<Animator>().GetBool("Crouch"))
        {
            _thirdPersonScript.GetComponent<Animator>().SetBool("Jump", true);
        }

        if (isGrounded && _rewiredplayer.GetButtonDown("Jump"))
        {
            if (blockMovementCooldown > 0 || _pController.pauseMenuOpen)
                return;

            float _jumpForce = jumpForce;
            Debug.Log("Jump");
            Debug.Log(_groundCheckScript.touch);

            if (GameManager.instance.gameMode == GameManager.GameMode.Swarm) { _jumpForce = jumpForce * 0.7f; }

            Vector3 v = verticalVector;
            v.y = _jumpForce;
            verticalVector = v;
        }



        if (_roofCheckScript.touch && _verticalVector.y > 0)
            _verticalVector.y = 0;
    }

    PlayerMovementDirection CheckDirection(float xValue, float zValue)
    {
        PlayerMovementDirection pmd = PlayerMovementDirection.Idle;

        if (xValue == -1 && zValue == 0)
        {
            movementDirection = PlayerMovementDirection.Left;
        }
        else if (xValue == 0 && zValue == 1)
        {
            movementDirection = PlayerMovementDirection.Forward;
        }
        else if (xValue == 1 && zValue == 0)
        {
            movementDirection = PlayerMovementDirection.Right;
        }
        else if (xValue == 0 && zValue == -1)
        {
            movementDirection = PlayerMovementDirection.Backwards;
        }

        else if (zValue > 0)
        {
            if (xValue < 0) //Second Quarter of Cartesian Map
            {
                if (zValue <= -0.5 * xValue)
                {
                    movementDirection = PlayerMovementDirection.Left;
                }
                else if (zValue > -0.5 * xValue && zValue < -2 * xValue)
                {
                    movementDirection = PlayerMovementDirection.ForwardLeft;
                }
                else if (zValue >= -2 * xValue)
                {
                    movementDirection = PlayerMovementDirection.Forward;
                }
            }
            else if (xValue > 0) //First Quarter of Cartesian Map
            {

                if (zValue >= 2 * xValue)
                {
                    movementDirection = PlayerMovementDirection.Forward;
                }
                else if (zValue > 0.5 * xValue && zValue < 2 * xValue)
                {
                    movementDirection = PlayerMovementDirection.ForwardRight;
                }
                else if (zValue <= 0.5 * xValue)
                {
                    movementDirection = PlayerMovementDirection.Right;
                }
            }

        }
        if (zValue < 0)
        {
            if (xValue < 0) //Third Quarter of Cartesian Map
            {
                if (zValue >= 0.5 * xValue)
                {
                    movementDirection = PlayerMovementDirection.Left;
                }
                else if (zValue < 0.5 * xValue && zValue > 2 * xValue)
                {
                    movementDirection = PlayerMovementDirection.BackwardsLeft;
                }
                else if (zValue <= 2 * xValue)
                {
                    movementDirection = PlayerMovementDirection.Backwards;
                }
            }
            else if (xValue > 0) //Fourth Quarter of Cartesian Map
            {

                if (zValue <= -2 * xValue)
                {
                    movementDirection = PlayerMovementDirection.Backwards;
                }
                else if (zValue < -0.5 * xValue && zValue > -2 * xValue)
                {
                    movementDirection = PlayerMovementDirection.BackwardsRight;
                }
                else if (zValue >= -0.5 * xValue)
                {
                    movementDirection = PlayerMovementDirection.Right;
                }
            }
        }
        else if (zValue == 0 && xValue == 0)
        {
            movementDirection = PlayerMovementDirection.Idle;
        }

        if (zValue > 0 || xValue > 0)
        {
            //if (isGrounded && !_pController.isCrouching)
            //{
            //    if (!walkingSoundPlaying)
            //    {
            //        PlayWalkingSound();
            //        walkingSoundPlaying = true; ;
            //    }
            //}
            //else
            //{
            //    PauseWalkingSound();
            //    walkingSoundPlaying = false;
            //}
        }


        return pmd;
    }

    void ControlAnimationSpeed()
    {
        if (_pController != null)
        {
            if (_pController.weaponAnimator != null)
            {
                if (isGrounded)
                {

                    if (_pController.weaponAnimator.GetBool("Walk"))
                    {
                        //Debug.Log("Here");
                        if (!_pController.isReloading && !_pController.isDrawingWeapon && !_pController.isThrowingGrenade &&
                            !_pController.isMeleeing && !_pController.isFiring)
                        {
                            _pController.weaponAnimator.speed = animationSpeed;
                            if (_pController.weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Draw"))
                                _pController.weaponAnimator.speed = 1;
                            _tpLookAt.anim.speed = animationSpeed;
                        }
                        else if (_pController.isReloading || _pController.isDrawingWeapon || _pController.isThrowingGrenade ||
                            _pController.isMeleeing || _pController.isFiring)
                        {
                            _speedRatio = 1;
                            _pController.weaponAnimator.speed = 1;
                            _tpLookAt.anim.speed = 1;
                        }
                    }
                    else
                    {
                        _speedRatio = 1;
                        _pController.weaponAnimator.speed = 1;
                        if (_tpLookAt.anim)
                            _tpLookAt.anim.speed = 1;
                    }
                }
                else if (!isGrounded && _pController.weaponAnimator.GetBool("Run"))
                {
                    _pController.weaponAnimator.speed = 0.1f;

                }
            }
        }
    }
    void WalkAnimation()
    {
        if (_rawRightInput != 0 || _rawForwardInput != 0)
        {
            if (isGrounded)
            {
                if (_pController.weaponAnimator)
                    _pController.weaponAnimator.SetBool("Walk", true);
            }
            else
                try { _pController.weaponAnimator.SetBool("Walk", false); } catch { }
        }
        else
            try { _pController.weaponAnimator.SetBool("Walk", false); } catch { }
    }
    void LadderMaxSpeedChange()
    {
        if (!_pController.pauseMenuOpen)
            if (isOnLadder)
                _currentMaxSpeed = _defaultMaxSpeed / 8;
    }
    public void ResetCharacterControllerProperties()
    {
        _cController.slopeLimit = defaultSlopeLimit;
        _cController.stepOffset = defaultStepOffset;
        currentMaxSpeed = _defaultMaxSpeed;
        isOnLadder = false;
    }
    public void ApplyResidualMovementWhileNotGrounded()
    {
        if (!isGrounded && _movementInput.magnitude > 0)
            _cController.Move(_movementInput * 0.9f * _lastCalulatedGroundedSpeed * Time.deltaTime);
    }

    void ApplyInAirMovement()
    {
        //if (!_groundCheckScript.isGrounded)
        //{
        //    Vector3 currentMovementInput = transform.right * _correctedRightInput + transform.forward * _correctedForwardInput;
        //    if (_movementInput.z + currentMovementInput.z > 1)
        //        currentMovementInput.z = 0;
        //    _cController.Move(currentMovementInput * 0.3f * _defaultMaxSpeed * Time.deltaTime);
        //}
    }
    public void ApplyGravityOnGravityVector()
    {
        _verticalVector.y += Mathf.Clamp(defaultGravity * Time.deltaTime, _terminalVelocity, 100);
        if (_verticalVector.y < -3 && isGrounded)
            _verticalVector.y = -3;
    }
    public float GetDefaultSpeed()
    {
        return _defaultMaxSpeed;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        // TODO: Redo this, causes lagspikes
        //if (RayGrounded(GetRayAtPos(0, 0)) && GetComponent<CharacterController>().isGrounded)
        //{
        //    //Debug.Log("OnControllerColliderHit");
        //    Vector3 edgeFallMovement = transform.position - hit.point;
        //    edgeFallMovement.y = 0;
        //    float edgeFallFactor = 10;
        //    _direction += (edgeFallMovement * Time.deltaTime * edgeFallFactor);
        //}
    }



    private Ray GetRayAtPos(float x, float z)
    {
        Vector3 rayPos = transform.position;
        rayPos.y -= GetComponent<CharacterController>().height / 2 - GetComponent<CharacterController>().radius;
        rayPos.x += x;
        rayPos.z += z;
        return new Ray(rayPos, Vector3.down);
    }

    private bool RayGrounded(Ray ray)
    {
        float groundRayLength = 0.5f;
        RaycastHit[] rayResults = new RaycastHit[5];
        return Physics.RaycastNonAlloc(ray, rayResults, groundRayLength, groundMask) < 1;
    }

    void ManCannonJumpCooldwon()
    {
        if (_canMoveWhileJumpingCooldown > 0)
        {
            _canMoveWhileJumpingCooldown -= Time.deltaTime;

            if (_canMoveWhileJumpingCooldown < 0)
                _canMoveWhileJumpingCooldown = 0;
        }

        if (_blockMovementCooldown > 0)
            _blockMovementCooldown -= Time.deltaTime;
    }








    /// <summary>
    ///                         IMoveable
    /// </summary>
    public void Push(Vector3 dir, int pow, PushSource ps, bool blockMovement)
    {
        if (player.isDead || player.isRespawning || !player.isMine)
            return;

        playerImpactReceiver.AddImpact(dir, pow);
        if (ps == PushSource.ManCannon)
            verticalVector = Vector3.zero;

        if (ps == PushSource.Melee)
            blockMovementCooldown = 1f;

        //    manCannonCooldown = 1f;

        //if (blockMovement)
        //    canMove = false;
    }
}

