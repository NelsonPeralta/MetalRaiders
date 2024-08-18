using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// https://www.youtube.com/watch?v=f473C43s8nE&ab_channel=Dave%2FGameDevelopment

public class PlayerMovement : MonoBehaviour
{
    public delegate void PlayerMovementEvent(PlayerMovement _movementVect);
    public PlayerMovementEvent OnPlayerStartedMoving, OnPlayerStoppedMoving;


    public enum PlayerMovementDirection { Idle, Left, Right, Forward, Backwards, ForwardLeft, ForwardRight, BackwardsLeft, BackwardsRight }
    public enum BlockedMovementType { None, ManCannon, Other }

    public Player player { get { return _player; } }
    public Rewired.Player rewiredPlayer { get { return _pController.rewiredPlayer; } }
    public PlayerMovementDirection movementDirection
    {
        get { return _playerMovementDirection; }
        private set
        {
            if (!isGrounded) return;

            _previousMovementDirEnum = _playerMovementDirection;
            _playerMovementDirection = value;

            if ((_previousMovementDirEnum == PlayerMovementDirection.Left && _playerMovementDirection == PlayerMovementDirection.Right) ||
                (_previousMovementDirEnum == PlayerMovementDirection.Right && _playerMovementDirection == PlayerMovementDirection.Left) ||
                (_previousMovementDirEnum == PlayerMovementDirection.Forward && _playerMovementDirection == PlayerMovementDirection.Backwards) ||
                (_previousMovementDirEnum == PlayerMovementDirection.Right && _playerMovementDirection == PlayerMovementDirection.Left))
            {
                Debug.Log("Player changed direction drastically");
                moveSpeed = Mathf.Clamp(moveSpeed, 0, 1);
            }
        }
    }
    public float correctedXInput { get { return _correctedRightInput; } set { _correctedRightInput = value; } }
    public float correctedZInput { get { return _correctedForwardInput; } set { _correctedForwardInput = value; } }
    public bool isGrounded
    {
        get { return _grounded; }
        private set
        {
            _grounded = value;
            if (value && readyToJump) _clickedJumpButtonFromLastGrounded = false;

            if (value && blockPlayerMoveInput <= 0)
            {
                blockedMovementType = BlockedMovementType.None;
            }

            if (value && _pushedByBlockMovement)
            {
                _pushedByBlockMovement = false;
            }
        }
    }
    public Rigidbody rb { get { return _rb; } }
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

            if (Mathf.Abs(correctedXInput) == 1 || Mathf.Abs(correctedZInput) == 1)
                _animationSpeed = 1;

            return Mathf.Abs(_animationSpeed);
        }
    }

    public float currentWorldSpeed
    {
        get { return _currentWorldSpeed; }
        private set
        {
            _currentWorldSpeed = value;
            //if (value <= 0.15 * currentMaxSpeed)
            //    isMoving = false;
            //else
            //    isMoving = true;
        }
    }
    public PlayerMotionTracker playerMotionTracker { get { return _playerMotionTracker; } }

    [SerializeField] GroundCheck _groundCheckScript;
    [SerializeField] ThirdPersonLookAt _tpLookAt;
    public float moveSpeed
    {
        get { return _moveSpeed; }
        set
        {
            _moveSpeed = value;

            if (movementDirection == PlayerMovementDirection.Idle)
                _moveSpeed = 0;
            else
                _moveSpeed = Mathf.Clamp(_moveSpeed, 1, _moveSpeed);

            //if (GameManager.instance.gameMode == GameManager.GameMode.Swarm && !player.hasArmor) Mathf.Clamp(_moveSpeed * 0.7f, 1, _moveSpeed);
        }
    }

    [Header("Movement")]
    [SerializeField] float _moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float desiredMoveSpeed;
    public float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;
    [SerializeField] Vector3 moveDirection;
    [SerializeField] PlayerMovementDirection _playerMovementDirection, _previousMovementDirEnum;


    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight, slopeRayLenght;
    public LayerMask whatIsGround;
    [SerializeField] bool _grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle, currentSlope;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    public Vector3 slopeMovement;


    public Transform orientation, slopeOrientation, playerCapsule;


    [SerializeField] float _animationSpeed, _currentWorldSpeed;
    [SerializeField] float _rawRightInput, _rawForwardInput, _correctedRightInput, _correctedForwardInput;
    [SerializeField] PlayerMotionTracker _playerMotionTracker;
    [SerializeField] bool _clickedJumpButtonFromLastGrounded;
    [SerializeField] PlayerMovementDirection _lastDirectionWhenJumped;


    float _rightDeadzone = 0.2f, _forwardDeadzone = 0.2f, _lastCalulatedGroundedSpeed;

    Rigidbody _rb;
    Player _player;
    PlayerController _pController;
    Rewired.Player _rewiredPlayer;
    ThirdPersonScript _thirdPersonScript;

    Vector3 _lastPos;

    public MovementState state;
    public enum MovementState
    {
        idling,
        walking,
        sprinting,
        crouching,
        air
    }








    [SerializeField] AudioSource _footstepAudioSource;
    float _footstepClipDelay;


    public bool isMoving
    {
        get { return _isMoving; }
        private set
        {
            if (_isMoving != value)
            {
                player.playerController.UpdateIsMoving(value);
            }

            _isMoving = value;
        }
    }


    [SerializeField] bool _isMoving;




    private void Awake()
    {

        _player = GetComponent<Player>();
        _pController = GetComponent<PlayerController>();
        _rewiredPlayer = _pController.rewiredPlayer;
        _thirdPersonScript = _pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel;

        _previousMovementDirEnum = PlayerMovementDirection.Idle;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;

        if (!player.isMine) _footstepAudioSource.volume = 1;

        player.OnPlayerRespawnEarly += OnPlayerRespawnEarly;
    }

    private void Update()
    {
        // ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        isGrounded = _groundCheckScript.isGrounded;

        // handle drag
        if (_grounded)
        {
            //if (Mathf.Abs(rewiredPlayer.GetAxis("Move Vertical")) <= _forwardDeadzone && Mathf.Abs(rewiredPlayer.GetAxis("Move Horizontal")) <= _rightDeadzone)
            //    _rb.drag = groundDrag * 10;
            //else
            _rb.drag = groundDrag;
        }
        else
            _rb.drag = 0;

        if (player.playerController.pauseMenuOpen)
        {
            moveDirection = Vector3.zero;
            desiredMoveSpeed = lastDesiredMoveSpeed = 0;
            _playerMovementDirection = _previousMovementDirEnum = PlayerMovementDirection.Idle;
            _rawRightInput = _correctedRightInput = _rawForwardInput = _correctedForwardInput = 0;
            state = MovementState.idling;
            WalkAnimation();
        }

        if (!CurrentRoomManager.instance.gameStarted || player.playerController.pauseMenuOpen) return;

        _thirdPersonScript.GetComponent<Animator>().SetBool("Jump", !isGrounded);
        CalculateCurrentSpeed();

        try
        {
            //if (OnSlope())
            //    slopeOrientation.forward = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
        }
        catch { }

        MyInput();
        StateHandler();


        CheckDirection(_rawRightInput, _rawForwardInput);




        CheckIsMovingAndPlayFootStepSound();




        WalkAnimation();
        ControlAnimationSpeed();

        if (blockPlayerMoveInput > 0)
        {
            _rb.useGravity = true;
            _rb.drag = 0;
            moveSpeed = 1;
            blockPlayerMoveInput -= Time.deltaTime;

            return;
        }
        else
        {
            SpeedControl();
        }

    }

    private void FixedUpdate()
    {
        if (!player.playerController.pauseMenuOpen)
        {
            MovePlayerUsingInput();
            MovePlayerUsingInputWhileJumping();
        }
    }


    private void MyInput()
    {
        _rawRightInput = _correctedRightInput = rewiredPlayer.GetAxis("Move Horizontal");
        _rawForwardInput = _correctedForwardInput = rewiredPlayer.GetAxis("Move Vertical");

        if (ReInput.controllers.GetLastActiveControllerType() != ControllerType.Joystick)
        {
            _rawRightInput = _correctedRightInput = Mathf.Clamp(rewiredPlayer.GetAxis("Move Horizontal") * 2, -1, 1);
            _rawForwardInput = _correctedForwardInput = Mathf.Clamp(rewiredPlayer.GetAxis("Move Vertical") * 2, -1, 1);
        }

        if (Mathf.Abs(_correctedRightInput) <= _rightDeadzone) _correctedRightInput = 0;
        if (Mathf.Abs(_correctedForwardInput) <= _forwardDeadzone) _correctedForwardInput = 0;

        // when to jump
        if (rewiredPlayer.GetButtonDown("Jump") && readyToJump && _grounded)
        {
            readyToJump = false; _clickedJumpButtonFromLastGrounded = true; _lastDirectionWhenJumped = movementDirection;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //// start crouch
        //if (rewiredPlayer.GetButtonDown("Crouch"))
        //{
        //    playerCapsule.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        //    //player.playerThirdPersonModel.transform.localPosition = new Vector3(player.playerThirdPersonModel.transform.localPosition.x, player.playerThirdPersonModel.transform.transform.localPosition.y + 0.5f, player.playerThirdPersonModel.transform.localPosition.z);

        //    if (grounded)
        //        _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        //}

        //// stop crouch
        //if (rewiredPlayer.GetButtonUp("Crouch"))
        //{
        //    playerCapsule.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
        //    //player.playerThirdPersonModel.transform.localPosition = new Vector3(player.playerThirdPersonModel.transform.localPosition.x, player.playerThirdPersonModel.transform.transform.localPosition.y - 0.5f, player.playerThirdPersonModel.transform.localPosition.z);
        //}
    }

    private void StateHandler()
    {
        // Mode = Idling

        if (correctedXInput == 0 && correctedZInput == 0)
        {
            state = MovementState.idling;
            desiredMoveSpeed = 0;
        }

        // Mode - Crouching
        else if (rewiredPlayer.GetButton("Crouch"))
        {
            state = MovementState.crouching;
            if (isGrounded)
                desiredMoveSpeed = crouchSpeed;
        }
        // Mode - Sprinting
        else if (player.playerController.isSprinting)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        //else if (movementDirection == PlayerMovementDirection.Forward &&
        //    _pController.activeControllerType == Rewired.ControllerType.Joystick &&
        //    rewiredPlayer.GetButtonDown("Sprint"))
        //{
        //    state = MovementState.sprinting;
        //    desiredMoveSpeed = sprintSpeed;
        //}

        // Mode - Walking
        else if (_grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        if ((player.playerController.isHoldingShootBtn) && state == MovementState.sprinting)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        if (GameManager.instance.gameMode == GameManager.GameMode.Coop && !player.hasArmor) desiredMoveSpeed *= 0.7f;


        // check if desiredMoveSpeed has changed drastically
        //if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        //{
        //    StopAllCoroutines();
        //    StartCoroutine(SmoothlyLerpMoveSpeed());
        //}
        //else
        //{
        //    moveSpeed = desiredMoveSpeed;
        //}

        StartCoroutine(SmoothlyLerpMoveSpeed());

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    [SerializeField] float time, difference, startValue;
    [SerializeField] bool processingMoveSpeed;

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        if (isGrounded)
        {
            // smoothly lerp movementSpeed to desired value
            time = 0;
            difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
            startValue = moveSpeed;
            processingMoveSpeed = true;

            while (time < difference)
            {
                processingMoveSpeed = true;
                moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, (time / difference) * 2f);

                if (OnSlope())
                {
                    float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                    float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                    time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
                }
                else
                    time += Time.deltaTime * speedIncreaseMultiplier;

                yield return null;
            }

            moveSpeed = desiredMoveSpeed;
        }
    }
    public float blockPlayerMoveInput
    {
        get
        {
            return _blockPlayerMoveInput;
        }

        set
        {
            _blockPlayerMoveInput = value;

            if (value > 0) _pushedByBlockMovement = true;
        }
    }


    public float _blockPlayerMoveInput;
    public bool _pushedByBlockMovement;
    public BlockedMovementType blockedMovementType;

    private void MovePlayerUsingInput()
    {
        if (blockPlayerMoveInput > 0) return;
        // calculate movement direction
        if (isGrounded)
            moveDirection = orientation.forward * _rawForwardInput + orientation.right * _rawRightInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            //slopeOrientation.forward = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
            GetSlopeMoveDirection(moveDirection);

            Vector3 slopeOrientationMovementFix = Vector3.zero;

            if (slopeMovement.magnitude > 0)
                slopeOrientationMovementFix = slopeOrientation.forward.normalized * moveSpeed * 10;

            _rb.AddForce((slopeMovement * moveSpeed * 10f) + slopeOrientationMovementFix, ForceMode.Force);

            if (_rb.velocity.y > 0)
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (_grounded)
            _rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!_grounded)
            _rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if (!isGrounded)
            _rb.AddForce((orientation.forward * _rawForwardInput + orientation.right * _rawRightInput).normalized * moveSpeed * 1f, ForceMode.Force);


        // turn gravity off while on slope
        if (!player.isDead && !player.isRespawning)
            _rb.useGravity = !OnSlope();
    }



    [SerializeField] Vector3 _jumpCorrectionDirection;
    void MovePlayerUsingInputWhileJumping()
    {
        if (isGrounded) _jumpCorrectionDirection = Vector3.zero;
        if (blockPlayerMoveInput > 0) return;


        if (_clickedJumpButtonFromLastGrounded && !_pushedByBlockMovement)
        {
            _jumpCorrectionDirection = orientation.forward * _correctedForwardInput + orientation.right * _correctedRightInput;

            if (_lastDirectionWhenJumped != PlayerMovementDirection.Idle)
                _rb.AddForce(_jumpCorrectionDirection.normalized * walkSpeed * 2, ForceMode.Force);
            else
                _rb.AddForce(_jumpCorrectionDirection.normalized * walkSpeed * 7, ForceMode.Force);
        }
    }




    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (_rb.velocity.magnitude > moveSpeed)
                _rb.velocity = _rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

            // limit velocity if needed
            if (!_pushedByBlockMovement)
                if (flatVel.magnitude > moveSpeed)
                {
                    Vector3 limitedVel = flatVel.normalized * moveSpeed;
                    _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);


                    //float speed = Vector3.Magnitude(_rb.velocity);  // test current object speed   
                    //if (speed > maximumSpeed)
                    //{
                    //    float brakeSpeed = speed - maximumSpeed;  // calculate the speed decrease
                    //    Vector3 normalisedVelocity = rigidbody.velocity.normalized;
                    //    Vector3 brakeVelocity = normalisedVelocity * brakeSpeed;  // make the brake Vector3 value  
                    //    rigidbody.AddForce(-brakeVelocity);  // apply opposing brake force
                    //}
                }
        }
    }
    float _tempSpeedVal;
    private void Jump()
    {
        if (blockPlayerMoveInput > 0) return;
        exitingSlope = true;

        // reset y velocity
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        _tempSpeedVal = jumpForce; if (GameManager.instance.gameMode == GameManager.GameMode.Coop && !player.hasArmor) _tempSpeedVal = jumpForce * 0.7f;
        _rb.AddForce(transform.up * _tempSpeedVal, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, slopeRayLenght, whatIsGround))
        {
            currentSlope = Vector3.Angle(Vector3.up, slopeHit.normal);
            return currentSlope < maxSlopeAngle && currentSlope != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        slopeMovement = Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;

        if (slopeMovement != Vector3.zero)
            slopeOrientation.forward = slopeMovement;

        return slopeMovement;
    }













    PlayerMovementDirection CheckDirection(float xValue, float zValue)
    {
        PlayerMovementDirection pmd = PlayerMovementDirection.Idle;

        if (xValue < 0 && zValue == 0)
        {
            movementDirection = PlayerMovementDirection.Left;
        }
        else if (xValue == 0 && zValue > 0)
        {
            movementDirection = PlayerMovementDirection.Forward;
        }
        else if (xValue > 0 && zValue == 0)
        {
            movementDirection = PlayerMovementDirection.Right;
        }
        else if (xValue == 0 && zValue < 0)
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
                if (_grounded)
                {

                    if (_pController.weaponAnimator.GetBool("Walk"))
                    {
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
                            //_speedRatio = 1;
                            _pController.weaponAnimator.speed = 1;
                            _tpLookAt.anim.speed = 1;
                        }
                    }
                    else
                    {
                        //_speedRatio = 1;
                        _pController.weaponAnimator.speed = 1;
                        if (_tpLookAt.anim)
                            _tpLookAt.anim.speed = 1;
                    }
                }
                else if (!_grounded && _pController.weaponAnimator.GetBool("Run"))
                {

                    if (_pController.weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                        _pController.weaponAnimator.speed = 0.1f;
                }
            }
        }
    }








    void WalkAnimation()
    {
        if (_rawRightInput != 0 || _rawForwardInput != 0)
        {
            if (_grounded)
            {
                if (_pController.weaponAnimator)
                    _pController.weaponAnimator.SetBool("Walk", true);

                if (_pController.pInventory.isDualWielding)
                {
                    _pController.pInventory.activeWeapon.leftWeapon.animator.SetBool("Walk", true);
                    _pController.pInventory.activeWeapon.rightWeapon.animator.SetBool("Walk", true);
                }
            }
            else
            {
                try { _pController.weaponAnimator.SetBool("Walk", false); } catch { }

                if (_pController.pInventory.isDualWielding)
                {
                    _pController.pInventory.activeWeapon.leftWeapon.animator.SetBool("Walk", false);
                    _pController.pInventory.activeWeapon.rightWeapon.animator.SetBool("Walk", false);
                }
            }
        }
        else
            try
            {
                _pController.weaponAnimator.SetBool("Walk", false);

                if (_pController.pInventory.isDualWielding)
                {
                    _pController.pInventory.activeWeapon.leftWeapon.animator.SetBool("Walk", false);
                    _pController.pInventory.activeWeapon.rightWeapon.animator.SetBool("Walk", false);
                }
            }
            catch { }
    }



    void CalculateCurrentSpeed()
    {
        Vector3 curPos = transform.position; curPos.y = 0;
        Vector3 lasPos = _lastPos; lasPos.y = 0;

        currentWorldSpeed = ((curPos - lasPos).magnitude / Time.deltaTime);
        currentWorldSpeed = Mathf.Clamp(Mathf.Round(currentWorldSpeed * 10f) / 10f, 0, walkSpeed);
        if (isGrounded)
            _lastCalulatedGroundedSpeed = currentWorldSpeed;
        _lastPos = transform.position;
        //CalculateSpeedRatio();
    }


    void CheckIsMovingAndPlayFootStepSound()
    {
        if (_grounded)
        {
            if (player.isMine)
            {
                if (_correctedRightInput == 0 && _correctedForwardInput == 0)
                    isMoving = false;
                else if (_correctedRightInput != 0 || _correctedForwardInput != 0)
                    isMoving = true;
            }



            if (!isMoving)
            {
                _footstepClipDelay = 0.35f;
            }
            else
            {
                _footstepClipDelay -= Time.deltaTime;


                if (_footstepClipDelay <= 0)
                {
                    if (_player.isMine)
                        _footstepAudioSource.volume = 0.2f;
                    else
                        _footstepAudioSource.volume = 1;




                    _footstepAudioSource.Play();
                    _footstepClipDelay = 0.35f;

                    if (player.playerController.isSprinting)
                        _footstepClipDelay = 0.25f;

                    if (player.playerController.isCrouching)
                    {
                        _footstepAudioSource.volume = 0.25f;
                        _footstepClipDelay = 0.45f;
                    }
                }
            }
        }
        else
            if (player.isMine) isMoving = false;

    }


    public void UpdateIsMoving(bool u)
    {
        _isMoving = u;
    }

    void OnPlayerRespawnEarly(Player p)
    {
        _rb.velocity = Vector3.zero; _rb.angularVelocity = Vector3.zero;
        blockedMovementType = BlockedMovementType.None;
        blockPlayerMoveInput = 0;
    }
}
