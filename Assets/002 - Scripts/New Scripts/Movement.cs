using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Movement : MonoBehaviour
{
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
    public float defaultSpeed = 5f;
    public float speed;
    public float playerSpeed;
    public float jumpForce = 6f;

    float gravity = -9.81f;

    public Vector3 velocity;
    public Vector3 lastPos;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public bool isGrounded;
    public bool isMovingForward;
    bool CalculatingPlayerSpeed;

    public float xDirection;
    public float zDirection;
    public string direction;
    public int directionIndicator;

    public Player player;
    public int playerRewiredID;

    [Header("Audio")]
    public AudioSource walkingSound;
    public bool walkingSoundPlaying;

    // Start is called before the first frame update
    void Start()
    {
        SetPlayerIDInInput();
        cManager = gameObject.GetComponent<ChildManager>();
        //tPersonScripts = cManager.FindChildWithTagScript("Third Person GO").GetComponent<ThirdPersonScript>();
        tPersonScripts.movement = gameObject.GetComponent<Movement>();
        cController = gameObject.GetComponent<CharacterController>();
        rBody = gameObject.GetComponent<Rigidbody>();
        pController = gameObject.GetComponent<PlayerController>();
        pProperties = GetComponent<PlayerProperties>();
        //StartCoroutine(CalcVelocity());
    }

    // Update is called once per frame
    void Update()
    {
        if (pController.PV.IsMine)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            float x = player.GetAxis("Move Horizontal");
            float z = player.GetAxis("Move Vertical");
            Vector3 direction = new Vector3(x, 0f, z).normalized;
            xDirection = direction.x;
            zDirection = direction.z;

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

            if (!pProperties.isDead)
            {
                if (!pController.isCrouching)
                {
                    Vector3 move = transform.right * x + transform.forward * z;
                    cController.Move(move * defaultSpeed * Time.deltaTime);
                }
                else
                {
                    Vector3 move = transform.right * x + transform.forward * z;
                    cController.Move(move * defaultSpeed * .5f * Time.deltaTime);
                }
            }



            CheckDirection(direction.x, direction.z);

            velocity.y += gravity * Time.deltaTime;

            cController.Move(velocity * Time.deltaTime);

            if (!CalculatingPlayerSpeed)
                StartCoroutine(CalculatePlayerSpeed());

            Jump();
            CheckMovingForward();
            ControlAnimationSpeed();
        }
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
        else if(!isGrounded && tPersonScripts.anim && !tPersonScripts.anim.GetBool("Crouch"))
        {
            tPersonScripts.anim.SetBool("Jump", true);
            speed = defaultSpeed * 2 / 3;
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
            walkingSound.Pause();
            walkingSoundPlaying = false;
        }

        if (zValue > 0 || xValue > 0)
        {
            if (isGrounded)
            {
                if (!walkingSoundPlaying)
                {
                    walkingSound.Play();
                    walkingSoundPlaying = true; ;
                }
            }
            else
            {
                walkingSound.Pause();
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
                    tpLookAt.anim.speed = 1;
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
}

