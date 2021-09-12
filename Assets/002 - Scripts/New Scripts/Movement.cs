using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Photon.Pun;

public class Movement : MonoBehaviour
{
    public PhotonView PV;
    public AllPlayerScripts allPlayerScripts;
    public CharacterController cController;
    public Rigidbody rBody;
    public PlayerController pController;
    public ThirdPersonScript tPersonScripts;
    public GameObject thirdPersonRoot;
    public GameObject thirdPersonModels;
    public ChildManager cManager;
    public ThirdPersonLookAt tpLookAt;
    public PlayerProperties pProperties;
    public PlayerSFXs sfx;
    public GroundCheck groundCheckScript;
    public GroundCheck roofCheckScript;
    float defaultSpeed; // Default = 5
    public float speed;
    public float playerSpeed;
    public float jumpForce = 6f;

    public float defaultGravity = -12f;
    float gravity = -12f; // -9.81f

    public Vector3 movement;
    public Vector3 velocity;
    public Vector3 calulatedVelocity;
    public Vector3 lastPos;

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

    public Player player;
    public int playerRewiredID;

    [Header("Audio")]
    public AudioSource walkingSoundAS;
    public bool walkingSoundPlaying;

    //Velocity variables
    Vector3 previousPosition;

    // Characater Controller Default Properties
    float defaultSlopeLimit;
    float defaultStepOffset;

    // Start is called before the first frame update
    void Start()
    {
        gravity = defaultGravity;
        SetPlayerIDInInput();
        cManager = gameObject.GetComponent<ChildManager>();
        //tPersonScripts = cManager.FindChildWithTagScript("Third Person GO").GetComponent<ThirdPersonScript>();
        tPersonScripts.movement = gameObject.GetComponent<Movement>();
        cController = gameObject.GetComponent<CharacterController>();
        rBody = gameObject.GetComponent<Rigidbody>();
        pController = gameObject.GetComponent<PlayerController>();
        pProperties = GetComponent<PlayerProperties>();
        defaultSpeed = speed;
        defaultSlopeLimit = GetComponent<CharacterController>().slopeLimit;
        defaultStepOffset = GetComponent<CharacterController>().stepOffset;
        //StartCoroutine(CalcVelocity());
    }

    // Update is called once per frame
    void Update()
    {
        if (!pController.PV.IsMine || pController.pauseMenuOpen)
            return;

        //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        isGrounded = groundCheckScript.isGrounded;
        float x = player.GetAxis("Move Horizontal");
        float z = player.GetAxis("Move Vertical");
        Vector3 direction = new Vector3(x, 0f, z).normalized;
        xDirection = direction.x;
        zDirection = direction.z;
        CalculateVelocity();

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -3f;
        }

        if (x != 0 || z != 0)
        {
            if (isGrounded)
            {
                if (pController.anim)
                    pController.anim.SetBool("Walk", true);

                if (pController.isDualWielding)
                {
                    if (pController.animDWRight != null)
                        pController.animDWRight.SetBool("Walk", true);
                    if (pController.animDWLeft != null)
                        pController.animDWLeft.SetBool("Walk", true);
                }
            }
            else
            {
                if (pController.anim != null)
                {
                    pController.anim.SetBool("Walk", false);
                }

                if (pController.isDualWielding)
                {
                    pController.animDWRight.SetBool("Walk", false);
                    pController.animDWLeft.SetBool("Walk", false);
                }
            }
        }
        else
        {
            if (pController.anim != null)
            {
                pController.anim.SetBool("Walk", false);
            }

            if (pController.isDualWielding)
            {
                if (pController.animDWRight != null)
                    pController.animDWRight.SetBool("Walk", false);
                if (pController.animDWLeft != null)
                    pController.animDWLeft.SetBool("Walk", false);
            }
        }

        if (isOnLadder)
            speed = defaultSpeed / 8;

        if (!pProperties.isDead)
        {
            Vector3 move = transform.right * x + transform.forward * z;
            if (isGrounded)
            {
                movement = transform.right * x + transform.forward * z;
                if (!pController.isCrouching)
                {
                    if (pController.isSprinting)
                    {
                        cController.Move(move * speed * 1.5f * Time.deltaTime);
                    }
                    else
                    {
                        cController.Move(move * speed * Time.deltaTime);
                    }
                }
                else
                {
                    cController.Move(move * speed * .5f * Time.deltaTime);
                }
            }
            else
            {
                cController.Move(movement * speed * Time.deltaTime);
                if (z > 0)
                    z = 0;
                move = transform.right * x + transform.forward * z;
                cController.Move(move * 0.5f * speed * Time.deltaTime);
            }
        }



        CheckDirection(direction.x, direction.z);

        velocity.y += gravity * Time.deltaTime;

        if (cController.gameObject.activeSelf)
            cController.Move(velocity * Time.deltaTime);

        if (!CalculatingPlayerSpeed)
            StartCoroutine(CalculatePlayerSpeed());

        Jump();
        CheckMovingForward();
        ControlAnimationSpeed();

    }

    void CalculateVelocity()
    {
        calulatedVelocity = ((transform.position - previousPosition)) / Time.deltaTime;
        previousPosition = transform.position;
    }

    public void SetPlayerIDInInput()
    {
        player = ReInput.players.GetPlayer(playerRewiredID);
    }

    void Jump()
    {
        if (isGrounded)
        {
            tPersonScripts.anim.SetBool("Jump", false);
            speed = defaultSpeed;
        }
        else if (!isGrounded && tPersonScripts.anim && !tPersonScripts.anim.GetBool("Crouch"))
        {
            tPersonScripts.anim.SetBool("Jump", true);
            //speed = defaultSpeed * 2 / 3;
            /*
            Vector3 move = transform.right * 0 + transform.forward * 1;
            cController.Move(move * speed * Time.deltaTime);
            */
        }

        if (isGrounded && player.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;

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
            if (pController.anim != null)
            {
                if (isGrounded)
                {

                    if (pController.anim.GetBool("Walk"))
                    {
                        //Debug.Log("Here");
                        if (!pController.isReloading && !pController.isDrawingWeapon && !pController.isThrowingGrenade &&
                            !pController.isMeleeing && !pController.isFiring)
                        {
                            pController.anim.speed = playerSpeed;
                            tpLookAt.anim.speed = playerSpeed;
                        }
                        else if (pController.isReloading || pController.isDrawingWeapon || pController.isThrowingGrenade ||
                            pController.isMeleeing || pController.isFiring)
                        {
                            playerSpeed = 1;
                            pController.anim.speed = 1;
                            tpLookAt.anim.speed = 1;
                        }
                    }
                    else
                    {
                        playerSpeed = 1;
                        pController.anim.speed = 1;
                        if (tpLookAt.anim)
                            tpLookAt.anim.speed = 1;
                    }
                }
                else if (!isGrounded && pController.anim.GetBool("Run"))
                {
                    pController.anim.speed = 0.1f;

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
        playerSpeed = (Mathf.Ceil(Vector3.Distance(gameObject.transform.position, lastPos) / 0.1f)) / 5f;

        if (playerSpeed > 1)
        {
            playerSpeed = 1;
        }
        if (pController.isCrouching)
        {
            playerSpeed *= 2;

            if (playerSpeed > 1)
                playerSpeed = 1;
        }
        CalculatingPlayerSpeed = false;
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
}

