using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Photon.Pun;

public class Movement : MonoBehaviour, IPunObservable
{
    public delegate void PlayerMovementEvent(Movement movement);
    public PlayerMovementEvent OnPlayerStartedMoving, OnPlayerStoppedMoving;

    public PhotonView PV;
    public AllPlayerScripts allPlayerScripts;
    public CharacterController cController;
    public Rigidbody rBody;
    public PlayerController pController;
    public GameObject thirdPersonRoot;
    public GameObject thirdPersonModels;
    public ThirdPersonLookAt tpLookAt;
    public Player pProperties;
    public PlayerSFXs sfx;
    public GroundCheck groundCheckScript;
    public GroundCheck roofCheckScript;
    public float defaultSpeed; // Default = 5
    public float speed;
    public float playerSpeedPercent;
    public float jumpForce = 6f;

    public float _defaultGravity = -9.81f;
    public float defaultGravity = -9.81f;
    float gravity = -9.81f;

    public Vector3 movement;
    public Vector3 velocity;
    public Vector3 calulatedVelocity;
    public Vector3 lastPos;

    [SerializeField]
    Vector3 jumpMovementCorrector;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public bool isGrounded;
    public bool isMovingForward;
    public bool isOnLadder;
    bool CalculatingPlayerSpeed;

    public float xDirection;
    public float zDirection;
    public string direction;
    public int directionIndicator;

    public Rewired.Player player
    {
        get { return pController.rewiredPlayer; }
    }

    [Header("Audio")]
    public AudioSource walkingSoundAS;
    public bool walkingSoundPlaying;

    //Velocity variables
    Vector3 previousPosition;

    // Characater Controller Default Properties
    float defaultSlopeLimit;
    float defaultStepOffset;

    [Header("Third Person Models")]
    public ThirdPersonScript noArmorThirdPersonScript;
    public ThirdPersonScript armorThirdPersonScript;

    bool _isMoving;
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
    private void Awake()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        {
            armorThirdPersonScript.gameObject.SetActive(true);
            armorThirdPersonScript.EnableSkinnedMeshes();
            noArmorThirdPersonScript.DisableSkinnedMeshes();
            if (!PV.IsMine)
            {
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.gameObject.layer = 0;

                foreach (SkinnedMeshRenderer s in pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.meshes)
                    s.gameObject.layer = 0;
            }
        }
        else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            armorThirdPersonScript.DisableSkinnedMeshes();
            noArmorThirdPersonScript.EnableSkinnedMeshes();
        }
    }
    void Start()
    {
        gravity = defaultGravity;
        //tPersonScripts = cManager.FindChildWithTagScript("Third Person GO").GetComponent<ThirdPersonScript>();
        cController = gameObject.GetComponent<CharacterController>();
        rBody = gameObject.GetComponent<Rigidbody>();
        pController = gameObject.GetComponent<PlayerController>();
        pProperties = GetComponent<Player>();
        defaultSpeed = speed;
        defaultSlopeLimit = GetComponent<CharacterController>().slopeLimit;
        defaultStepOffset = GetComponent<CharacterController>().stepOffset;
        //StartCoroutine(CalcVelocity());
        StartCoroutine(CalculatePlayerSpeed());
    }

    // IMPORTANT
    // Update() check for input
    // FixedUpdate() move gameObjects that have riggidbody/apply forces
    // LateUpdate() move Camera

    // Update is called once per frame
    void Update()
    {
        {
            var rotationVector = transform.rotation.eulerAngles;
            rotationVector.z = 0;
            rotationVector.x = 0;
            gameObject.transform.rotation = Quaternion.Euler(rotationVector);
        }

        isGrounded = groundCheckScript.isGrounded;
        CalculateVelocity();

        if (!pController.PV.IsMine || pController.pauseMenuOpen)
            return;

        // Axis Calculation
        #region
        float xAxis = player.GetAxis("Move Horizontal");
        float zAxis = player.GetAxis("Move Vertical");
        Vector3 direction = new Vector3(xAxis, 0f, zAxis).normalized;
        xDirection = direction.x;
        zDirection = direction.z;
        #endregion

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -3f;
        }

        // Walk animation
        #region
        if (xAxis != 0 || zAxis != 0)
        {
            if (isGrounded)
            {
                if (pController.weaponAnimator)
                    pController.weaponAnimator.SetBool("Walk", true);
            }
            else
                try { pController.weaponAnimator.SetBool("Walk", false); } catch { }
        }
        else
            try { pController.weaponAnimator.SetBool("Walk", false); } catch { }
        #endregion

        if (isOnLadder)
            speed = defaultSpeed / 8;

        // Movement
        #region
        if (!pProperties.isDead)
        {
            Vector3 currentMovementInput = transform.right * xAxis + transform.forward * zAxis;
            if (isGrounded)
            {
                movement = transform.right * xAxis + transform.forward * zAxis;
                if (!pController.isCrouching)
                {
                    if (pController.isSprinting)
                        cController.Move(currentMovementInput * (speed + 2f) * Time.deltaTime);
                    else
                        cController.Move(currentMovementInput * speed * Time.deltaTime);
                }
                else
                    cController.Move(currentMovementInput * speed * .5f * Time.deltaTime);
            }
            else
            {
                currentMovementInput = transform.right * xAxis + transform.forward * zAxis;

                if (Mathf.Sign(movement.x) == Mathf.Sign(currentMovementInput.x))
                    currentMovementInput.x = 0;
                if (Mathf.Sign(movement.z) == Mathf.Sign(currentMovementInput.z))
                    currentMovementInput.z = 0;

                cController.Move(movement * speed * Time.deltaTime);
                cController.Move(currentMovementInput * 0.65f * speed * Time.deltaTime);
            }
        }
        #endregion


        CheckDirection(direction.x, direction.z);

        velocity.y += Mathf.Clamp( gravity * Time.deltaTime, -3f, 100);  

        if (cController.gameObject.activeSelf)
            cController.Move(velocity * Time.deltaTime);

        Jump();
        CrouchJump();
        CheckMovingForward();
        ControlAnimationSpeed();
    }

    void CalculateVelocity()
    {
        calulatedVelocity = ((transform.position - previousPosition)) / Time.deltaTime;
        previousPosition = transform.position;

        if (calulatedVelocity.magnitude > 0f)
            isMoving = true;
        else
            isMoving = false;
    }


    float _crouchJumpTime = 0.2f;
    float crouchJumpTime = 0.2f;
    void CrouchJump()
    {
        if (!isGrounded && player.GetButton("Crouch"))
        {
            crouchJumpTime -= Time.deltaTime;
            if (crouchJumpTime > 0)
            {
                gravity = 0;
            }
        }
        else
        {
            gravity = _defaultGravity;
        }

        if (player.GetButtonUp("Crouch"))
        {
            crouchJumpTime = _crouchJumpTime;
        }

        //if (!isGrounded && player.GetButtonDown("Crouch"))
        //{
        //    Debug.Log("Crouch jumping");
        //    Debug.Log(velocity.y);

        //    float newForce = Mathf.Ceil(velocity.y + jumpForce;
        //    velocity.y += jumpForce;
        //    Debug.Log(velocity.y);
        //}
    }

    void Jump()
    {
        ThirdPersonScript thirdPersonScript = null;
        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            thirdPersonScript = pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel;
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            thirdPersonScript = pController.GetComponent<PlayerThirdPersonModelManager>().humanModel;

        if (isGrounded)
        {
            thirdPersonScript.GetComponent<Animator>().SetBool("Jump", false);
            speed = defaultSpeed;
        }
        else if (!isGrounded && thirdPersonScript.GetComponent<Animator>() && !thirdPersonScript.GetComponent<Animator>().GetBool("Crouch"))
        {
            thirdPersonScript.GetComponent<Animator>().SetBool("Jump", true);
            //speed = defaultSpeed * 2 / 3;
            /*
            Vector3 move = transform.right * 0 + transform.forward * 1;
            cController.Move(move * speed * Time.deltaTime);
            */
        }

        if (isGrounded && player.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
            Debug.Log(player.name);
            //rBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }



        if (roofCheckScript.isGrounded)
            gravity = defaultGravity * 10;
        else
            gravity = defaultGravity;
    }

    int CheckDirection(float xValue, float zValue)
    {
        if (xValue == -1 && zValue == 0)
        {
            directionIndicator = 1;
            direction = "Left";
        }
        else if (xValue == 0 && zValue == 1)
        {
            directionIndicator = 3;
            direction = "Forward";
        }
        else if (xValue == 1 && zValue == 0)
        {
            directionIndicator = 5;
            direction = "Right";
        }
        else if (xValue == 0 && zValue == -1)
        {
            directionIndicator = 7;
            direction = "Backwards";
        }

        else if (zValue > 0)
        {
            if (xValue < 0) //Second Quarter of Cartesian Map
            {
                if (zValue <= -0.5 * xValue)
                {
                    directionIndicator = 1;
                    direction = "Left";
                }
                else if (zValue > -0.5 * xValue && zValue < -2 * xValue)
                {
                    directionIndicator = 2;
                    direction = "Forward-Left";
                }
                else if (zValue >= -2 * xValue)
                {
                    directionIndicator = 3;
                    direction = "Forward";
                }
            }
            else if (xValue > 0) //First Quarter of Cartesian Map
            {

                if (zValue >= 2 * xValue)
                {
                    directionIndicator = 3;
                    direction = "Forward";
                }
                else if (zValue > 0.5 * xValue && zValue < 2 * xValue)
                {
                    directionIndicator = 4;
                    direction = "Forward-Right";
                }
                else if (zValue <= 0.5 * xValue)
                {
                    directionIndicator = 5;
                    direction = "Right";
                }
            }

        }
        if (zValue < 0)
        {
            if (xValue < 0) //Third Quarter of Cartesian Map
            {
                if (zValue >= 0.5 * xValue)
                {
                    directionIndicator = 1;
                    direction = "Left";
                }
                else if (zValue < 0.5 * xValue && zValue > 2 * xValue)
                {
                    directionIndicator = 8;
                    direction = "Backwards-Left";
                }
                else if (zValue <= 2 * xValue)
                {
                    directionIndicator = 7;
                    direction = "Backwards";
                }
            }
            else if (xValue > 0) //Fourth Quarter of Cartesian Map
            {

                if (zValue <= -2 * xValue)
                {
                    directionIndicator = 7;
                    direction = "Backwards";
                }
                else if (zValue < -0.5 * xValue && zValue > -2 * xValue)
                {
                    directionIndicator = 6;
                    direction = "Backwards-Right";
                }
                else if (zValue >= -0.5 * xValue)
                {
                    directionIndicator = 5;
                    direction = "Right";
                }
            }
        }
        else if (zValue == 0 && xValue == 0)
        {
            directionIndicator = 0;
            direction = "Idle";
            PauseWalkingSound();
            walkingSoundPlaying = false;
        }

        if (zValue > 0 || xValue > 0)
        {
            if (isGrounded && !pController.isCrouching)
            {
                if (!walkingSoundPlaying)
                {
                    PlayWalkingSound();
                    walkingSoundPlaying = true; ;
                }
            }
            else
            {
                PauseWalkingSound();
                walkingSoundPlaying = false;
            }
        }


        return directionIndicator;
    }

    void CheckMovingForward()
    {
        if (directionIndicator == 2 || directionIndicator == 3 || directionIndicator == 4)
        {
            isMovingForward = true;
        }
        else
        {
            isMovingForward = false;
        }
    }

    void ControlAnimationSpeed()
    {
        if (pController != null)
        {
            if (pController.weaponAnimator != null)
            {
                if (isGrounded)
                {

                    if (pController.weaponAnimator.GetBool("Walk"))
                    {
                        //Debug.Log("Here");
                        if (!pController.isReloading && !pController.isDrawingWeapon && !pController.isThrowingGrenade &&
                            !pController.isMeleeing && !pController.isFiring)
                        {
                            pController.weaponAnimator.speed = playerSpeedPercent;
                            tpLookAt.anim.speed = playerSpeedPercent;
                        }
                        else if (pController.isReloading || pController.isDrawingWeapon || pController.isThrowingGrenade ||
                            pController.isMeleeing || pController.isFiring)
                        {
                            playerSpeedPercent = 1;
                            pController.weaponAnimator.speed = 1;
                            tpLookAt.anim.speed = 1;
                        }
                    }
                    else
                    {
                        playerSpeedPercent = 1;
                        pController.weaponAnimator.speed = 1;
                        if (tpLookAt.anim)
                            tpLookAt.anim.speed = 1;
                    }
                }
                else if (!isGrounded && pController.weaponAnimator.GetBool("Run"))
                {
                    pController.weaponAnimator.speed = 0.1f;

                }
            }
        }
    }

    IEnumerator CalculatePlayerSpeed()
    {
        //Debug.Log("Calculating Player Speed");
        CalculatingPlayerSpeed = true;
        lastPos = gameObject.transform.position;
        yield return new WaitForSeconds(0.1f);
        playerSpeedPercent = (Mathf.Ceil(Vector3.Distance(gameObject.transform.position, lastPos) / 0.1f)) / 5f;

        if (playerSpeedPercent > 1)
        {
            playerSpeedPercent = 1;
        }
        if (pController.isCrouching)
        {
            playerSpeedPercent *= 2;

            if (playerSpeedPercent > 1)
                playerSpeedPercent = 1;
        }
        CalculatingPlayerSpeed = false;
        StartCoroutine(CalculatePlayerSpeed());
    }

    void PlayWalkingSound()
    {
        PV.RPC("PlayWalkingSound_RPC", RpcTarget.All);
    }

    void PauseWalkingSound()
    {
        PV.RPC("PauseWalkingSoundRPC", RpcTarget.All);
    }

    [PunRPC]
    void PlayWalkingSound_RPC()
    {
        walkingSoundAS.Play();
    }

    [PunRPC]
    void PauseWalkingSoundRPC()
    {
        walkingSoundAS.Pause();
    }

    public void ResetCharacterControllerProperties()
    {
        cController.slopeLimit = defaultSlopeLimit;
        cController.stepOffset = defaultStepOffset;
        speed = defaultSpeed;
        isOnLadder = false;
    }

    public float GetDefaultSpeed()
    {
        return defaultSpeed;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if (stream.IsWriting)
        //{
        //    stream.SendNext(movement);

        //}
        //else
        //{
        //    Debug.Log($"I am reading: {movement}");
        //    movement = (Vector3)stream.ReceiveNext();
        //}
    }
}

