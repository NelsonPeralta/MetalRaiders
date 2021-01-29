using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerScript : MonoBehaviour
{
    PlayerControls controls;
    Rigidbody rBody;
    public Animator anim;
    public PlayerInventory pInventory;
    public WeaponProperties wProperties;
    public GeneralWeapProperties gwProperties;
    public PlayerController playerController;
    public PlayerInventory piManager;
    public FPSControllerLPFP.FpsControllerLPFP notMyFPSController;

    public float jumpForce = 35f;

    public bool InteractButtonPressed = false;
    public bool SwitchWeaponsButtonPressed = false;
    public bool hasRTriggerDown = false;

    Vector2 move;
    Vector2 rotate;

    [HideInInspector]
    public bool hasBeenHolstered = false, holstered, isRunning, isAiming, isWalking;
    [HideInInspector]
    public bool isInspecting, isReloading, isShooting, aimSoundHasPlayed = false, hasFoundComponents = false;

    private void Awake()
    {

        controls = new PlayerControls();

        /*
        controls.Gameplay.Jump.performed += ctx => Jump();
        controls.Gameplay.Fire.performed += ctx => Fire();
        controls.Gameplay.Fire.canceled += ctx => CancelFire();
        controls.Gameplay.ThrowGrenade.performed += ctx => ThrowGrenade();
        controls.Gameplay.Melee.performed += ctx => Melee();
        controls.Gameplay.Reload.performed += ctx => Reload();
        controls.Gameplay.SwitchWeapons.performed += ctx => SwitchWeapons();

        controls.Gameplay.Interact.performed += ctx => Interact();
        controls.Gameplay.Interact.canceled += ctx => CancelInteract();

        controls.Gameplay.Rotate.performed += ctx => rotate = ctx.ReadValue<Vector2>();
        controls.Gameplay.Rotate.canceled += ctx => rotate = Vector2.zero;
        */

    }

    private void Start()
    {
        pInventory = GameObject.FindGameObjectWithTag("Player Inventory").GetComponent<PlayerInventory>();
        wProperties = GameObject.FindGameObjectWithTag("Weapon").GetComponent<WeaponProperties>();
        pInventory = GameObject.FindGameObjectWithTag("Player Inventory").GetComponent<PlayerInventory>();
        gwProperties = GetComponent<GeneralWeapProperties>();
        playerController = GetComponent<PlayerController>();
        rBody = GetComponent<Rigidbody>();

        notMyFPSController = GetComponent<FPSControllerLPFP.FpsControllerLPFP>();
    }

    private void Update()
    {
        Move();

        Rotate();

        if (pInventory.activeWeapIs == 0)
        {
            wProperties = pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>();
            anim = pInventory.weaponsEquiped[0].gameObject.GetComponent<Animator>();
        }

        else if (pInventory.activeWeapIs == 1)
        {
            wProperties = pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>();
            anim = pInventory.weaponsEquiped[1].gameObject.GetComponent<Animator>();
        }

        if (notMyFPSController._isGrounded && notMyFPSController._rigidbody.velocity.sqrMagnitude > 0.1f)
        {
            anim.SetBool("Walk", true);
        }
        else
        {
            anim.SetBool("Walk", false);
        }
    }

    private void Move()
    {
        Vector3 movement = new Vector3(move.x, 0, move.y) * Time.deltaTime * 0f;
        transform.Translate(movement);
    }

    private void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    private void Rotate()
    {
        Vector3 rotation = new Vector3(-rotate.y, rotate.x, 0) * Time.deltaTime * 120f;
        transform.Rotate(rotation);
    }

    private void OnRotate(InputValue value)
    {
        rotate = value.Get<Vector2>();
    }

    void Jump()
    {
        Debug.Log("Player JUMPED with controller");
        rBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void Fire()
    {
        if (!wProperties.outOfAmmo && !isReloading && !isShooting && !isInspecting /*&& !isRunning && burstEnabler*/)
        {
            Debug.Log("Right Trigger pushed");
            isShooting = true;
            StartCoroutine(FiringCountdown());
            
        }
        
    }

    void CancelFire()
    {
        Debug.Log("Right Trigger released");
        isShooting = false;
        hasRTriggerDown = false;
    }

    void ThrowGrenade()
    {
        Debug.Log("Player THREW A GRENADE with controller");
        StartCoroutine(GrenadeSpawnDelay());
        //Play grenade throw animation
        anim.Play("GrenadeThrow", 0, 0.0f);
    }

    void Melee()
    {
        Debug.Log("Player MELEED with controller");
        anim.Play("Knife Attack 2", 0, 0f);
    }

    void Reload()
    {
        Debug.Log("Player RELOADED with controller");
        //playerController.StartCoroutine(Reload());
    }

    void SwitchWeapons()
    {
        Debug.Log("Player SWITCHED WEAPONS with controller");

        SwitchWeaponsButtonPressed = true;
        StartCoroutine(InteractionAndSwapWeaponsCountdown());
    }

    void Interact()
    {
        Debug.Log("Player INTERACTED with controller");
        InteractButtonPressed = true;
        Debug.Log(InteractButtonPressed);
        StartCoroutine(InteractionAndSwapWeaponsCountdown());
    }

    void CancelInteract()
    {
        InteractButtonPressed = false;
        Debug.Log(InteractButtonPressed);
    }

    private void OnEnable()
    {
        //controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        //controls.Gameplay.Disable();
    }



    private IEnumerator InteractionAndSwapWeaponsCountdown()
    {
        yield return new WaitForEndOfFrame ();
        CancelInteract();
        SwitchWeaponsButtonPressed = false;
    }

    private IEnumerator FiringCountdown()
    {
        yield return new WaitForEndOfFrame();
        hasRTriggerDown = true;
    }

    private IEnumerator GrenadeSpawnDelay()
    {

        //Wait for set amount of time before spawning grenade
        yield return new WaitForSeconds(pInventory.grenadeSpawnDelay);
        //Spawn grenade prefab at spawnpoint
        Instantiate(gwProperties.pInventory,
            gwProperties.grenadeSpawnPoint.transform.position,
            gwProperties.grenadeSpawnPoint.transform.rotation);
    }
}
