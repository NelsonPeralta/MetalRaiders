using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Other Scripts")]
    public PlayerSFXs sfxManager;
    public CrosshairScript crosshairScript;
    public PlayerController pController;
    public PlayerProperties pProperties;
    public ChildManager cManager;
    public GeneralWeapProperties gwProperties;
    public ReloadScript rScript;
    public DualWielding dWielding;

    [Space(20)]
    [Header("Data")]
    public string StartingWeapon = "M4";
    public int activeWeapIs = 0;
    public GameObject activeWeapon;
    public bool hasSecWeap = false;

    [Space(20)]
    [Header("Equipped Weapons")]
    public GameObject[] weaponsEquiped = new GameObject[2];
    public int currentAmmo = 0;
    public int currentExtraAmmo = 0;

    [Header("Grenades")]
    public int grenades = 0;
    public int maxGrenades = 4;
    public Transform grenadePrefab;
    public Transform stickyGrenadePrefab;
    public float grenadeSpawnDelay = 0.25f;

    [Header("Dual Wielding")]
    public GameObject rightWeapon;
    public GameObject leftWeapon;
    public int rightWeaponCurrentAmmo;
    public int leftWeaponCurrentAmmo;

    [Space(20)]
    [Header("Unequipped Weapons")]
    public GameObject[] Unequipped = new GameObject[25];

    [Space(20)]
    [Header("Ammo")]
    public int smallAmmo = 0;
    public int heavyAmmo = 0;
    public int powerAmmo = 0;
    
    [Space(10)]
    public int maxSmallAmmo = 72;
    public int maxHeavyAmmo = 60;
    public int maxPowerAmmo = 8;
    

    [Header("Weapon Meshes On Player")]
    public GameObject weaponMesh1;
    public GameObject weaponMesh2;
    public GameObject weaponMesh1Location2;
    public GameObject weaponMesh2Location2;
    public GameObject weaponMesh1Location3;
    public GameObject weaponMesh2Location3;

    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    public void Start()
    {
        cManager = GetComponent<ChildManager>();

        //FindAllWeaponsInPlayer();
        StartCoroutine(EquipStartingWeapon());

        //cScript = GameObject.FindGameObjectWithTag("Player").GetComponent<ControllerScript>();

        /*
        sfxManager.mainAudioSource.clip = sfxManager.cockingClip1;
        sfxManager.mainAudioSource.Play();
        */
    }

    private void Update()
    {
        AmmoManager();

        if (pController.player.GetButtonDown("Switch Weapons") && !pController.isDualWielding /*Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetAxis("Mouse ScrollWheel") < 0 /*|| cScript.SwitchWeaponsButtonPressed*/)
        {
            pController.Unscope();

            if (pController.isReloading && pController.pInventory.weaponsEquiped[1] != null)
            {
                rScript.reloadIsCanceled = true;

            }

            if (pController.pInventory.weaponsEquiped[1] != null && !pProperties.isDead)
            {
                SwapWeapons();
            }
        }

        UpdateActiveWeapon();
        gwProperties.CheckFireMode();

        if (pController.isDualWielding)
        {
            UpdateDualWieldedWeaponAmmo();

            if (pController.player.GetButtonDown("Switch Weapons"))
            {
                pController.isDualWielding = false;
                pProperties.DropActiveWeapon(leftWeapon);

                activeWeapon.GetComponent<WeaponProperties>().currentAmmo = rightWeapon.GetComponent<WeaponProperties>().currentAmmo;
                activeWeapon.SetActive(true);

                rightWeapon.SetActive(false);
                leftWeapon.SetActive(false);

                rightWeapon = null;
                leftWeapon = null;
            }
        }
    }

    public void SwapWeapons()
    {
        if (hasSecWeap == true)
        {
            if (weaponsEquiped[0].gameObject.activeSelf)
            {
                weaponsEquiped[1].gameObject.SetActive(true);
                weaponsEquiped[0].gameObject.SetActive(false);

                sfxManager.mainAudioSource.clip = sfxManager.cockingClip1;
                sfxManager.mainAudioSource.Play();

                activeWeapon = weaponsEquiped[1].gameObject;
                activeWeapIs = 1;

                StartCoroutine(ToggleTPPistolIdle(0));
            }

            else if (weaponsEquiped[1].gameObject.activeSelf)
            {
                weaponsEquiped[1].gameObject.SetActive(false);
                weaponsEquiped[0].gameObject.SetActive(true);

                sfxManager.mainAudioSource.clip = sfxManager.cockingClip2;
                sfxManager.mainAudioSource.Play();

                activeWeapon = weaponsEquiped[0].gameObject;
                activeWeapIs = 0;

                StartCoroutine(ToggleTPPistolIdle(1));
            }
        }
    }

    public void UpdateActiveWeapon()
    {
        if (activeWeapIs == 0)
        {
            if (weaponsEquiped[0] != null)
            {
                activeWeapon = weaponsEquiped[0].gameObject;
            }
        }
        else if (activeWeapIs == 1)
        {
            if (weaponsEquiped[1] != null)
            {
                activeWeapon = weaponsEquiped[1].gameObject;
            }
        }
    }

    void FindAllWeaponsInPlayer()
    {
        int childCounter = 0;

        foreach (GameObject child in cManager.allChildren)
        {
            if ((child.GetComponent<Tags>() != null) && (child.GetComponent<Tags>().hasTag("Weapon")) && (child.gameObject.GetComponent<WeaponProperties>() != null))
            {
                Unequipped[childCounter] = child.gameObject;
                Unequipped[childCounter].gameObject.GetComponent<WeaponProperties>().weaponName = Unequipped[childCounter].gameObject.name;
                Unequipped[childCounter].gameObject.GetComponent<WeaponProperties>().storedWeaponNumber = childCounter;
                childCounter = childCounter + 1;
            }
        }

        /*
        foreach (Transform child in transform)
        {
            if (child.tag == "Weapon Binder")
            {
                foreach (Transform childOFchild in child)
                {
                    if (childOFchild.tag == "Weapon")
                    {
                        if (childOFchild.gameObject.GetComponent<WeaponProperties>() != null)
                        {
                            Unequipped[childCounter] = childOFchild.gameObject;
                            Unequipped[childCounter].gameObject.GetComponent<WeaponProperties>().weaponName = Unequipped[childCounter].gameObject.name;
                            Unequipped[childCounter].gameObject.GetComponent<WeaponProperties>().storedWeaponNumber = childCounter;
                            childCounter = childCounter + 1;
                        }


                    }

                }
            }


        }
        */
    }

    public IEnumerator EquipStartingWeapon()
    {
        yield return new WaitForEndOfFrame(); // Withou this it will think the Array is Empty

        for (int i = 0; i < Unequipped.Length; i++)
        {
            if (Unequipped[i] != null)
            {
                if (Unequipped[i].name == StartingWeapon)
                {
                    Unequipped[i].gameObject.SetActive(true);
                    weaponsEquiped[0] = Unequipped[i].gameObject;
                    //Debug.Log("Check 1");
                    activeWeapon = weaponsEquiped[0];
                    weaponsEquiped[0] = activeWeapon;
                    activeWeapIs = 0;
                    activeWeapon.GetComponent<WeaponProperties>().currentAmmo = activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon;
                    StartCoroutine(ToggleTPPistolIdle(1));
                }

                else if (Unequipped[i].name != StartingWeapon)
                {
                    Unequipped[i].gameObject.SetActive(false);
                }
            }
        }

        gwProperties.CheckFireMode();
    }

    void AmmoManager()
    {
        if (activeWeapon != null)
        {
            currentAmmo = activeWeapon.gameObject.GetComponent<WeaponProperties>().currentAmmo;
            //Debug.Log($"Active Weapon: {activeWeapon}\nAmmo: {activeWeapon.gameObject.GetComponent<WeaponProperties>().currentAmmo}");

            if (activeWeapon.GetComponent<WeaponProperties>().smallAmmo)
            {
                currentExtraAmmo = smallAmmo;
            }

            else if (activeWeapon.GetComponent<WeaponProperties>().heavyAmmo)
            {
                currentExtraAmmo = heavyAmmo;
            }

            else if (activeWeapon.GetComponent<WeaponProperties>().powerAmmo)
            {
                currentExtraAmmo = powerAmmo;
            }
        }
    }

    void UpdateDualWieldedWeaponAmmo()
    {
        rightWeaponCurrentAmmo = rightWeapon.GetComponent<WeaponProperties>().currentAmmo;
        leftWeaponCurrentAmmo = leftWeapon.GetComponent<WeaponProperties>().currentAmmo;
    }

    public void SwapGunsOnCharacter(int secondaryWeapon)
    {
        Debug.Log("Active weapon:" + activeWeapon.name);

        foreach(GameObject weap in Unequipped)
        {
            if(weap.GetComponent<WeaponProperties>().thirdPersonModelEquipped)
                weap.GetComponent<WeaponProperties>().thirdPersonModelEquipped.SetActive(false);
            if(weap.GetComponent<WeaponProperties>().thirdPersonModelUnequipped)
                weap.GetComponent<WeaponProperties>().thirdPersonModelUnequipped.SetActive(false);
        }

        if (activeWeapon.GetComponent<WeaponProperties>().thirdPersonModelEquipped &&
            activeWeapon.GetComponent<WeaponProperties>().thirdPersonModelUnequipped)
        {
            activeWeapon.GetComponent<WeaponProperties>().thirdPersonModelEquipped.SetActive(true);
            activeWeapon.GetComponent<WeaponProperties>().thirdPersonModelUnequipped.SetActive(false);
        }

        if (weaponsEquiped[1])
        {
            if (weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().thirdPersonModelEquipped &&
                weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().thirdPersonModelUnequipped)
            {
                weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().thirdPersonModelEquipped.SetActive(false);
                weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().thirdPersonModelUnequipped.SetActive(true);
            }
        }

        /*
        Debug.Log("Active weapon:" + activeWeapon.name);
        this.weaponMesh1.SetActive(false);
        this.weaponMesh2.SetActive(false);
        this.weaponMesh1Location2.SetActive(false);
        this.weaponMesh2Location2.SetActive(false);

        Mesh weaponMesh1 = activeWeapon.GetComponent<WeaponProperties>().weaponMesh;
        Material weaponMaterial1 = activeWeapon.GetComponent<WeaponProperties>().weaponMaterial;

        if (!activeWeapon.GetComponent<WeaponProperties>().location2 && !activeWeapon.GetComponent<WeaponProperties>().location3)
        {
            this.weaponMesh1.SetActive(true);

            this.weaponMesh1.GetComponent<MeshFilter>().mesh = weaponMesh1;
            this.weaponMesh1.GetComponent<MeshRenderer>().material = weaponMaterial1;
        }
        else if (activeWeapon.GetComponent<WeaponProperties>().location2)
        {
            this.weaponMesh1Location2.SetActive(true);

            this.weaponMesh1Location2.GetComponent<MeshFilter>().mesh = weaponMesh1;
            this.weaponMesh1Location2.GetComponent<MeshRenderer>().material = weaponMaterial1;
        }
        else if (activeWeapon.GetComponent<WeaponProperties>().location3)
        {
            this.weaponMesh1Location3.GetComponent<MeshFilter>().mesh = weaponMesh1;
            this.weaponMesh1Location3.GetComponent<MeshRenderer>().material = weaponMaterial1;
        }

        if (weaponsEquiped[1])
        {
            Mesh weaponMesh2 = weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().weaponMesh;
            Material weaponMaterial2 = weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().weaponMaterial;

            if (!weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().location2 &&
                !weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().location3)
            {
                this.weaponMesh2.SetActive(true);

                this.weaponMesh2.GetComponent<MeshFilter>().mesh = weaponMesh2;
                this.weaponMesh2.GetComponent<MeshRenderer>().material = weaponMaterial2;
            }
            else if (weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().location2)
            {
                this.weaponMesh2Location2.SetActive(true);

                this.weaponMesh2Location2.GetComponent<MeshFilter>().mesh = weaponMesh2;
                this.weaponMesh2Location2.GetComponent<MeshRenderer>().material = weaponMaterial2;
            }
            else if (weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().location3)
            {
                this.weaponMesh2Location3.GetComponent<MeshFilter>().mesh = weaponMesh2;
                this.weaponMesh2Location3.GetComponent<MeshRenderer>().material = weaponMaterial2;
            }
        }*/
    }

    public IEnumerator ToggleTPPistolIdle(int secondaryWeapon)
    {
        SwapGunsOnCharacter(secondaryWeapon);
        yield return new WaitForEndOfFrame();

        if (activeWeapon.GetComponent<WeaponProperties>().pistolIdle)
        {
            pController.tPersonController.anim.SetBool("Idle Pistol", true);
            pController.tPersonController.anim.SetBool("Idle Rifle", false);
        }
        else
        {
            pController.tPersonController.anim.SetBool("Idle Rifle", true);
            pController.tPersonController.anim.SetBool("Idle Pistol", false);
        }
    }
}
