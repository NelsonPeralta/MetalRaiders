using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class WeaponPickUp : MonoBehaviourPun
{
    [Header("Singletons")]
    WeaponPool weaponPool;

    [Header("Other Scripts")]
    public PlayerProperties pProperties;
    public PlayerInventory pInventory;
    public DualWielding dWielding;
    public PlayerController pController;
    public PlayerSFXs sfxManager;
    public Text pickupText;
    public AudioSource ammoPickupAudioSource;
    public PhotonView PV;
    //public ControllerScript cScript;

    public GameObject weaponCollidingWithInInventory; // Stores weapon in order to use Update void without "other"
    public GameObject weaponCollidingWith;
    public string weaponName;
    public int puWeapStoredNumber;
    public int equippedWeapStoredNum;

    [Header("Dual Wielding")]
    public GameObject rightArmWeaponInInventory;
    public GameObject leftArmWeaponInInventory;
    public bool canPickupDW;

    [Header("Items to Drop")]
    public GameObject weaponEquippedToDrop1;
    public GameObject weaponEquippedToDrop2;

    KeyCode pickup = KeyCode.E;

    private bool isOnTrigger = false;
    private bool canPickup = false;
    bool InSameAmmoType;
    bool weaponHasMoreAmmoThanCurrent;

    private void Start()
    {
        weaponPool = WeaponPool.weaponPoolInstance;
        //pInventory = GameObject.FindGameObjectWithTag("Player Inventory").GetComponent<PlayerInventoryManager>();
        //pController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        //pickupText = GameObject.FindGameObjectWithTag("Player Informer").GetComponent<Text>();

        //cScript = GameObject.FindGameObjectWithTag("Player").GetComponent<ControllerScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        isOnTrigger = true;

        if (isOnTrigger == true)
        {

            if (other.gameObject.GetComponent<LootableWeapon>() != null) //Check if weapon on maps have the Pickable Tag
            {
                if (!WeaponAlreadyInInventory(other.gameObject))
                {
                    weaponCollidingWith = other.gameObject;

                    for (int i = 0; i < pInventory.allWeaponsInInventory.Length; i++) //Looks for a weapon in the Unequipped Array for the Gameobject with the same name
                    {
                        if (pInventory.allWeaponsInInventory[i] != null)
                        {
                            if (other.gameObject.name == pInventory.allWeaponsInInventory[i].gameObject.name)
                            {
                                if (pInventory.weaponsEquiped[1] == null)
                                {
                                    if (other.gameObject.name != pInventory.weaponsEquiped[0].gameObject.name)
                                    {
                                        //Debug.Log("Here");
                                        weaponCollidingWithInInventory = pInventory.allWeaponsInInventory[i].gameObject; // Adds the weapon in "pickupWeap"
                                        pickupText.text = "Hold E to pick up " + weaponCollidingWithInInventory.name;
                                        canPickup = true;
                                    }
                                    else
                                    {
                                        PickupAmmoFromWeapon(other.gameObject);
                                    }
                                }
                                else
                                {
                                    /*
                                    if (other.gameObject.GetComponent<LootableWeapon>().weaponType == pInventory.weaponsEquiped[0].GetComponent<WeaponProperties>().weaponType ||
                                        other.gameObject.GetComponent<LootableWeapon>().weaponType == pInventory.weaponsEquiped[1].GetComponent<WeaponProperties>().weaponType)
                                    {
                                        PickupAmmoFromWeapon(other.gameObject);
                                    }
                                    Debug.Log("Small Weapon has more Ammo");
                                    Debug.Log("Weapon type is = " + other.gameObject.GetComponent<LootableWeapon>().ammoInThisWeapon);
                                    weaponCollidingWithInInventory = pInventory.Unequipped[i].gameObject; // Adds the weapon in "pickupWeap"
                                    pickupText.text = "Pick up: " + weaponCollidingWithInInventory.name;
                                    canPickup = true;
                                    weaponHasMoreAmmoThanCurrent = true;
                                    */
                                    if (pInventory.activeWeapIs == 0)
                                    {
                                        if (other.gameObject.name != pInventory.weaponsEquiped[0].gameObject.name)
                                        {
                                            //Debug.Log("Here");
                                            weaponCollidingWithInInventory = pInventory.allWeaponsInInventory[i].gameObject; // Adds the weapon in "pickupWeap"
                                            pickupText.text = "Hold E to pick up " + weaponCollidingWithInInventory.name;
                                            canPickup = true;
                                        }
                                        else
                                        {
                                            //PickupAmmoFromWeapon(other.gameObject);
                                        }
                                    }
                                    else
                                    {
                                        if (other.gameObject.name != pInventory.weaponsEquiped[1].gameObject.name)
                                        {
                                            //Debug.Log("Here");
                                            weaponCollidingWithInInventory = pInventory.allWeaponsInInventory[i].gameObject; // Adds the weapon in "pickupWeap"
                                            pickupText.text = "Hold E to pick up " + weaponCollidingWithInInventory.name;
                                            canPickup = true;
                                        }
                                        else
                                        {
                                            //PickupAmmoFromWeapon(other.gameObject);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //PickupAmmoFromWeapon(other.gameObject);
                }
            }


            if (other.gameObject.GetComponent<LootableWeapon>() != null && other.gameObject.GetComponent<LootableWeapon>().isDualWieldable &&
                        pInventory.activeWeapon.GetComponent<WeaponProperties>().isDualWieldable)
            {
                Debug.Log("In script");
                weaponCollidingWith = other.gameObject;

                for (int i = 0; i < pInventory.allWeaponsInInventory.Length; i++) //Looks for a weapon in the Unequipped Array for the Gameobject with the same name
                {
                    if (pInventory.allWeaponsInInventory[i] != null)
                    {
                        if (other.gameObject.name == pInventory.allWeaponsInInventory[i].gameObject.name)
                        {
                            if (weaponHasMoreAmmoThanCurrent)
                            {
                                weaponCollidingWithInInventory = pInventory.allWeaponsInInventory[i].gameObject; // Adds the weapon in "pickupWeap"

                                canPickup = true;
                            }

                        }

                    }
                }

                for (int i = 0; i < dWielding.dualWiledableWeaponsRighArm.Length; i++) // Looks for the Righ Arm Only version of the weapon
                {
                    if (pInventory.activeWeapon.gameObject.name == dWielding.dualWiledableWeaponsRighArm[i].gameObject.name)
                    {
                        rightArmWeaponInInventory = dWielding.dualWiledableWeaponsRighArm[i].gameObject;
                        canPickupDW = true;

                    }



                }

                for (int i = 0; i < dWielding.dualWiledableWeaponsLeftArm.Length; i++) // Looks for the Righ Arm Only version of the weapon
                {
                    if (other.gameObject.name == dWielding.dualWiledableWeaponsLeftArm[i].gameObject.name)
                    {
                        leftArmWeaponInInventory = dWielding.dualWiledableWeaponsLeftArm[i].gameObject;
                        canPickupDW = true;

                    }



                }

                if (canPickupDW && weaponCollidingWith != null)
                {
                    pickupText.text = "Hold E to pick up " + other.gameObject.name + " or press Reload to Dual Wield";
                }
            }


        }
    }

    private void OnTriggerExit(Collider other)
    {
        ResetCollider();
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        if (isOnTrigger == true && canPickup == true)
        {
            if (pController.player.GetButtonShortPressDown("Interact") /*|| cScript.InteractButtonPressed*/)
            {
                int weaponCollidingWithInInventoryIndex = 0;
                for (int i = 0; i < pInventory.allWeaponsInInventory.Length; i++)
                    if (weaponCollidingWithInInventory == pInventory.allWeaponsInInventory[i])
                        weaponCollidingWithInInventoryIndex = i;
                Vector3 lwPosition = weaponCollidingWith.GetComponent<LootableWeapon>().GetSpawnPointPosition();
                if (!pInventory.holsteredWeapon) // Looks for Secondary Weapon
                {
                    Debug.Log("RPC: Picking up second weapon");
                    //PickupSecWeap();
                    PV.RPC("PickupSecondWeapon", RpcTarget.All, lwPosition, weaponCollidingWithInInventoryIndex);

                    pInventory.hasSecWeap = true;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = weaponCollidingWith.gameObject.GetComponent<LootableWeapon>().ammoInThisWeapon;

                    ResetCollider();
                    pInventory.playDrawSound();
                }
                else if (pInventory.weaponsEquiped[1] != null && weaponCollidingWith.gameObject.GetComponent<LootableWeapon>() != null) // Replace Equipped weapon
                {
                    PV.RPC("ReplaceWeapon", RpcTarget.All, lwPosition, weaponCollidingWithInInventoryIndex);
                    ResetCollider();
                    pInventory.playDrawSound();
                }
                Debug.Log("RPC: Calling RPC_DisableCollidingWeapon");
                PV.RPC("RPC_DisableCollidingWeapon", RpcTarget.All, lwPosition);
            }
        }


        if (isOnTrigger == true && canPickupDW == true)
        {
            if (pController.player.GetButtonShortPressDown("Switch Grenades") /*|| cScript.InteractButtonPressed*/)
            {
                pInventory.leftWeaponCurrentAmmo = weaponCollidingWith.GetComponent<LootableWeapon>().ammoInThisWeapon;

                pInventory.activeWeapon.gameObject.SetActive(false);

                pInventory.rightWeapon = rightArmWeaponInInventory;
                pInventory.leftWeapon = leftArmWeaponInInventory;

                pInventory.rightWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.currentAmmo;

                pInventory.rightWeapon.SetActive(true);
                pInventory.leftWeapon.SetActive(true);

                pController.isDualWielding = true;


                if (!weaponCollidingWith.gameObject.GetComponent<LootableWeapon>().isWallGun)
                {
                    Destroy(weaponCollidingWith);
                    ResetCollider();
                }

            }

        }
    }







    [PunRPC]
    public void ReplaceWeapon(Vector3 collidingWeaponPosition, int weaponCollidingWithInInventoryIndex)
    {
        LootableWeapon lws = weaponPool.GetWeaponWithSpawnPoint(collidingWeaponPosition);
        weaponCollidingWithInInventory = pInventory.allWeaponsInInventory[weaponCollidingWithInInventoryIndex];
        //LootableWeapon lws = PhotonView.Find(collidingWeaponPhotonId).gameObject.GetComponent<LootableWeapon>();
        Debug.Log("Replace Weapon");
        if (pInventory.activeWeapIs == 1)
        {
            weaponEquippedToDrop1 = pInventory.activeWeapon.gameObject;

            pInventory.allWeaponsInInventory[weaponCollidingWithInInventoryIndex].SetActive(true);
            pInventory.weaponsEquiped[1].gameObject.SetActive(false);
            pInventory.weaponsEquiped[1] = weaponCollidingWithInInventory;
            pInventory.activeWeapon = weaponCollidingWithInInventory.GetComponent<WeaponProperties>();

            pInventory.activeWeapon = weaponCollidingWithInInventory.GetComponent<WeaponProperties>();

            weaponCollidingWithInInventory.GetComponent<WeaponProperties>().currentAmmo = lws.ammoInThisWeapon;
            //pickupExtraAmmoFromWeapon(weaponCollidingWith.GetComponent<LootableWeapon>());

            Debug.Log("Replace Weapon 1");

            StartCoroutine(pInventory.ToggleTPPistolIdle(0));
        }

        if (pInventory.activeWeapIs == 0)
        {
            weaponEquippedToDrop1 = pInventory.activeWeapon.gameObject;

            weaponCollidingWithInInventory.SetActive(true);
            pInventory.weaponsEquiped[0].gameObject.SetActive(false);
            pInventory.weaponsEquiped[0] = weaponCollidingWithInInventory;
            pInventory.activeWeapon = weaponCollidingWithInInventory.GetComponent<WeaponProperties>();

            pInventory.activeWeapon = weaponCollidingWithInInventory.GetComponent<WeaponProperties>();

            weaponCollidingWithInInventory.GetComponent<WeaponProperties>().currentAmmo = lws.ammoInThisWeapon;
            //pickupExtraAmmoFromWeapon(weaponCollidingWith.GetComponent<LootableWeapon>());

            Debug.Log("Replace Weapon 1");

            StartCoroutine(pInventory.ToggleTPPistolIdle(1));
        }
        pInventory.changeAmmoCounter();
    }

    [PunRPC]
    public void PickupSecondWeapon(Vector3 collidingWeaponPosition, int weaponCollidingWithInInventoryIndex)
    {
        LootableWeapon lws = weaponPool.GetWeaponWithSpawnPoint(collidingWeaponPosition);
        weaponEquippedToDrop1 = pInventory.activeWeapon.gameObject;

        weaponCollidingWithInInventory = pInventory.allWeaponsInInventory[weaponCollidingWithInInventoryIndex];
        weaponCollidingWithInInventory.SetActive(true);
        pInventory.weaponsEquiped[1] = weaponCollidingWithInInventory;
        pInventory.holsteredWeapon = pInventory.activeWeapon;
        pInventory.activeWeapon.gameObject.SetActive(false);
        pInventory.activeWeapon = weaponCollidingWithInInventory.GetComponent<WeaponProperties>();
        pInventory.activeWeapIs = 1;
        pInventory.hasSecWeap = true;

        weaponCollidingWithInInventory.GetComponent<WeaponProperties>().currentAmmo = lws.ammoInThisWeapon;
        //pickupExtraAmmoFromWeapon(weaponCollidingWith.GetComponent<LootableWeapon>());

        Debug.Log("Replace Weapon 1");

        StartCoroutine(pInventory.ToggleTPPistolIdle(0));
        pInventory.changeAmmoCounter();
    }

    [PunRPC]
    public void RPC_DisableCollidingWeapon(Vector3 collidingWeaponPosition)
    {
        Debug.Log("RPC: Disabling lootable weapon");
        weaponPool.DisablePooledWeapon(collidingWeaponPosition);
    }

    // TO DO: make it across all the network
    public void PickupAmmoFromWeapon(GameObject weapon)
    {
        Debug.Log("In Ammo Pickuo From Weapon: " + weapon);
        if (weapon.GetComponent<LootableWeapon>() != null)
        {
            LootableWeapon weaponScript = weapon.GetComponent<LootableWeapon>();

            if (!weaponScript.isWallGun)
            {
                if (weaponScript.smallAmmo)
                {
                    //pickupExtraAmmoFromWeapon(weaponScript);
                    int ammoAllowedToRemoveFromWeapon = pInventory.maxSmallAmmo - pInventory.smallAmmo;

                    if (weaponScript.ammoInThisWeapon <= ammoAllowedToRemoveFromWeapon)
                    {
                        pInventory.smallAmmo = pInventory.smallAmmo + weaponScript.ammoInThisWeapon;
                        weaponScript.ammoInThisWeapon = 0;
                        ammoPickupAudioSource.Play();
                    }
                    else if (weaponScript.ammoInThisWeapon > ammoAllowedToRemoveFromWeapon)
                    {
                        pInventory.smallAmmo = pInventory.smallAmmo + ammoAllowedToRemoveFromWeapon;
                        weaponScript.ammoInThisWeapon = weaponScript.ammoInThisWeapon - ammoAllowedToRemoveFromWeapon;
                        ammoPickupAudioSource.Play();
                    }

                    if (weaponScript.ammoInThisWeapon == 0)
                    {
                        //Destroy(weapon.gameObject);
                        weapon.SetActive(false);
                        Debug.Log("Destroyed Small Weapon");
                        ResetCollider();
                        StartCoroutine(ResetColliderLate());
                    }
                }
                if (weaponScript.heavyAmmo)
                {
                    //pickupExtraAmmoFromWeapon(weaponScript);
                    int ammoAllowedToRemoveFromWeapon = pInventory.maxHeavyAmmo - pInventory.heavyAmmo;

                    if (weaponScript.ammoInThisWeapon <= ammoAllowedToRemoveFromWeapon)
                    {
                        pInventory.heavyAmmo = pInventory.heavyAmmo + weaponScript.ammoInThisWeapon;
                        weaponScript.ammoInThisWeapon = 0;
                        ammoPickupAudioSource.Play();
                    }
                    else if (weaponScript.ammoInThisWeapon > ammoAllowedToRemoveFromWeapon)
                    {
                        pInventory.heavyAmmo = pInventory.heavyAmmo + ammoAllowedToRemoveFromWeapon;
                        weaponScript.ammoInThisWeapon = weaponScript.ammoInThisWeapon - ammoAllowedToRemoveFromWeapon;
                        ammoPickupAudioSource.Play();
                    }

                    if (weaponScript.ammoInThisWeapon == 0)
                    {
                        //Destroy(weapon.gameObject);
                        weapon.SetActive(false);
                        ResetCollider();
                        StartCoroutine(ResetColliderLate());
                    }
                }
                if (weaponScript.powerAmmo)
                {
                    //pickupExtraAmmoFromWeapon(weaponScript);
                    int ammoAllowedToRemoveFromWeapon = pInventory.maxPowerAmmo - pInventory.powerAmmo;

                    if (weaponScript.ammoInThisWeapon <= ammoAllowedToRemoveFromWeapon)
                    {
                        pInventory.powerAmmo = pInventory.powerAmmo + weaponScript.ammoInThisWeapon;
                        weaponScript.ammoInThisWeapon = 0;
                        ammoPickupAudioSource.Play();
                    }
                    else if (weaponScript.ammoInThisWeapon > ammoAllowedToRemoveFromWeapon)
                    {
                        pInventory.powerAmmo = pInventory.powerAmmo + ammoAllowedToRemoveFromWeapon;
                        weaponScript.ammoInThisWeapon = weaponScript.ammoInThisWeapon - ammoAllowedToRemoveFromWeapon;
                        ammoPickupAudioSource.Play();
                    }

                    if (weaponScript.ammoInThisWeapon == 0)
                    {
                        //Destroy(weapon.gameObject);
                        weapon.SetActive(false);
                        ResetCollider();
                        StartCoroutine(ResetColliderLate());

                    }
                }
                if (weaponScript.ammoInThisWeapon == 0)
                {
                    //Destroy(weapon.gameObject);
                    weapon.SetActive(false);
                    ResetCollider();
                    StartCoroutine(ResetColliderLate());
                }
            }
        }
    }

    bool WeaponAlreadyInInventory(GameObject weaponCollidingWith)
    {
        foreach (GameObject weap in pInventory.weaponsEquiped)
        {
            if (weap)
                if (weap.name == weaponCollidingWith.name)
                    return true;
        }
        return false;
    }

    void ResetCollider()
    {
        //Debug.Log("Reset Collider");
        weaponCollidingWith = null;
        weaponCollidingWithInInventory = null;

        pickupText.text = "";
        isOnTrigger = false;
        canPickup = false;
        canPickupDW = false;

        InSameAmmoType = false;
        weaponHasMoreAmmoThanCurrent = false;
    }

    IEnumerator ResetColliderLate()
    {
        yield return new WaitForEndOfFrame();

        Debug.Log("Reset Collider Late");
        weaponCollidingWith = null;
        weaponCollidingWithInInventory = null;

        pickupText.text = "";
        isOnTrigger = false;
        canPickup = false;
        canPickupDW = false;

        InSameAmmoType = false;
        weaponHasMoreAmmoThanCurrent = false;

    }

    //void pickupExtraAmmoFromWeapon(LootableWeapon weapon)
    //{
    //    Debug.Log("Extra ammo: " + weapon.extraAmmo);
    //    if (weapon.smallAmmo)
    //    {
    //        int availableExtraAmmo = weapon.extraAmmo;
    //        int ammoMissing = pInventory.maxSmallAmmo - pInventory.smallAmmo;

    //        if (ammoMissing >= availableExtraAmmo)
    //        {
    //            pInventory.smallAmmo = pInventory.smallAmmo + availableExtraAmmo;
    //        }
    //        else
    //        {
    //            pInventory.smallAmmo = pInventory.maxSmallAmmo;
    //        }
    //    }

    //    if (weapon.heavyAmmo)
    //    {
    //        int availableExtraAmmo = weapon.extraAmmo;
    //        int ammoMissing = pInventory.maxHeavyAmmo - pInventory.heavyAmmo;

    //        Debug.Log("Ammo Missing = " + ammoMissing + " available Extra Ammo = " + availableExtraAmmo + " heavy ammo in stock = " + pInventory.heavyAmmo);

    //        if (ammoMissing >= availableExtraAmmo)
    //        {
    //            //Debug.Log("Grabbed extra ammo");
    //            pInventory.heavyAmmo = pInventory.heavyAmmo + availableExtraAmmo;
    //        }
    //        else
    //        {
    //            Debug.Log("Maxxed Ammo");
    //            pInventory.heavyAmmo = pInventory.maxHeavyAmmo;
    //        }
    //    }

    //    if (weapon.powerAmmo)
    //    {
    //        int availableExtraAmmo = weapon.extraAmmo;
    //        int ammoMissing = pInventory.maxPowerAmmo - pInventory.powerAmmo;

    //        if (ammoMissing >= availableExtraAmmo)
    //        {
    //            pInventory.powerAmmo = pInventory.powerAmmo + availableExtraAmmo;
    //        }
    //        else
    //        {
    //            pInventory.powerAmmo = pInventory.maxPowerAmmo;
    //        }
    //    }
    //}

    public void DisableAmmoPackWithRPC(int index)
    {
        if (!PV.IsMine)
            return;
        Debug.Log("Disabling Ammo Pack");
        PV.RPC("DespawnAmmoPack_RPC", RpcTarget.All, index);
    }

    [PunRPC]
    void DespawnAmmoPack_RPC(int index)
    {
        weaponPool.allAmmoPacks[index].GetComponent<AmmoPack>().onlineAmmoPackSpawnPoint.StartRespawn();
        weaponPool.allAmmoPacks[index].SetActive(false);
    }
}
