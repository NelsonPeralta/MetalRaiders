using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.ObjectModel;
using TMPro;
using Rewired;
using System.Linq;

public class PlayerWeaponSwapping : MonoBehaviourPun
{
    int _originalLayer;



    // Events
    public delegate void WeaponSwappingEvent(PlayerWeaponSwapping weaponPickUp);
    public WeaponSwappingEvent OnWeaponPickup;

    [Header("Singletons")]
    WeaponPool weaponPool;

    [Header("Other Scripts")]
    public Player player;
    public PlayerInventory pInventory;
    public DualWielding dWielding;
    public PlayerController pController;
    public PlayerSFXs sfxManager;
    public AudioSource ammoPickupAudioSource;
    public PhotonView PV;
    //public ControllerScript cScript;

    public GameObject weaponCollidingWithInInventory; // Stores weapon in order to use Update void without "other"
    public string weaponName;
    public int puWeapStoredNumber;
    public int equippedWeapStoredNum;

    [Header("Dual Wielding")]
    public GameObject rightArmWeaponInInventory;
    public GameObject leftArmWeaponInInventory;

    KeyCode pickup = KeyCode.E;

    bool InSameAmmoType;
    bool weaponHasMoreAmmoThanCurrent;

    public LootableWeapon _closestLootableWeapon;

    public LootableWeapon closestLootableWeapon
    {
        get { return _closestLootableWeapon; }
        set
        {
            _closestLootableWeapon = value;

            if (GameManager.instance.gameType == GameManager.GameType.Oddball && player.playerInventory.playerOddballActive) return;

            if (value)
            {
                WeaponProperties wp = player.playerInventory.GetWeaponProperties(closestLootableWeapon.codeName);

                if (wp.isDualWieldable && pInventory.activeWeapon.isDualWieldable)
                {
                    //pickupText.text = "Hold E to DUAL WIELD " + closestLootableWeapon.cleanName;
                }
                else
                {
                    if (wp.weaponIcon)
                    {
                        player.GetComponent<PlayerUI>().ShowInformer($"Hold E to pick up", wp.weaponIcon);


                        //player.GetComponent<PlayerUI>()._informerHolder.gameObject.SetActive(true);
                        //pickupText.text = $"Hold E to pick up";

                        //pickupWeaponImage.sprite = wp.weaponIcon;
                        //var tempColor = pickupWeaponImage.color;
                        //tempColor.a = 255;
                        //pickupWeaponImage.color = tempColor;
                    }
                    else
                    {
                        //pickupText.text = "Hold E to pick up " + closestLootableWeapon.cleanName;
                        player.GetComponent<PlayerUI>().ShowInformer("Hold E to pick up " + closestLootableWeapon.cleanName);

                        if (GameManager.instance.activeControllerType == ControllerType.Joystick)
                            player.GetComponent<PlayerUI>().ShowInformer("Hold RELOAD to pick up " + closestLootableWeapon.cleanName);
                    }
                }
            }
            else
            {
                //pickupText.text = "";
                //pickupWeaponImage.sprite = null;
                //var tempColor = pickupWeaponImage.color;
                //tempColor.a = 0;
                //pickupWeaponImage.color = tempColor;
                //player.GetComponent<PlayerUI>()._informerHolder.gameObject.SetActive(false);
                player.GetComponent<PlayerUI>().HideInformer();
            }
        }
    }

    class CustomList<T>
    {
        // Declare an array to store the data elements.
        private T[] arr = new T[100];
        int nextIndex = 0;

        // Define the indexer to allow client code to use [] notation.
        public T this[int i] => arr[i];

        public void Add(T value)
        {
            if (nextIndex >= arr.Length)
                throw new System.IndexOutOfRangeException($"The collection can hold only {arr.Length} elements.");
            arr[nextIndex++] = value;
        }
    }

    [SerializeField] List<LootableWeapon> _weaponsInRange = new List<LootableWeapon>();
    public List<LootableWeapon> weaponsInRange
    {
        get { return _weaponsInRange; }
        set
        {
            _weaponsInRange = value;

            if (_weaponsInRange.Count == 0) { closestLootableWeapon = null; return; }

            float smallestDistance = 100;
            for (int i = 0; i < weaponsInRange.Count; i++)
                if (Vector3.Distance(weaponsInRange[i].transform.position, transform.position) < smallestDistance)
                {
                    smallestDistance = Vector3.Distance(weaponsInRange[i].transform.position, transform.position);
                    closestLootableWeapon = weaponsInRange[i];
                }
        }
    }

    //[SerializeField] List<LootableWeapon> _weaponsInRange = new List<LootableWeapon>();

    //public List<LootableWeapon> weaponsInRange
    //{
    //    get { return _weaponsInRange; }
    //    set
    //    {
    //        _weaponsInRange = value;

    //        Debug.Log($"esrse123424");
    //        Debug.Log(_weaponsInRange.Count);
    //        if (_weaponsInRange.Count == 0) { closestLootableWeapon = null; return; }

    //        Debug.Log($"esrse");
    //        float smallestDistance = 100;
    //        for (int i = 0; i < weaponsInRange.Count; i++)
    //            if (Vector3.Distance(weaponsInRange[i].transform.position, transform.position) < smallestDistance)
    //            {
    //                Debug.Log($"esasdasqwerse");
    //                smallestDistance = Vector3.Distance(weaponsInRange[i].transform.position, transform.position);
    //                closestLootableWeapon = weaponsInRange[i];
    //            }
    //    }
    //}
    private void Start()
    {
        _originalLayer = gameObject.layer;

        player.OnPlayerDeath -= OnPLayerDeath;
        player.OnPlayerDeath += OnPLayerDeath;

        player.OnPlayerRespawned -= OnPlayerRespawn;
        player.OnPlayerRespawned += OnPlayerRespawn;

        weaponPool = FindObjectOfType<WeaponPool>();

        pController.OnPlayerLongInteract += OnPlayerLongInteract_Delegate;
    }

    private void OnTriggerStay(Collider other)
    {
        return;
        if (other.GetComponent<LootableWeapon>() && other.gameObject.activeSelf)
        {
            //if (player.isDead ||
            //weaponsInRange.Contains(other.GetComponent<LootableWeapon>()) ||
            //(pInventory.activeWeapon && other.GetComponent<LootableWeapon>().codeName == pInventory.activeWeapon.codeName) ||
            //    (pInventory.holsteredWeapon && other.GetComponent<LootableWeapon>().codeName == pInventory.holsteredWeapon.codeName))
            //    return;

            if (player.isDead || weaponsInRange.Contains(other.GetComponent<LootableWeapon>()) || GameManager.instance.gameType == GameManager.GameType.GunGame)
                return;

            try
            {
                if ((pInventory.activeWeapon && other.GetComponent<LootableWeapon>().codeName != pInventory.activeWeapon.codeName) &&
                    (pInventory.holsteredWeapon && other.GetComponent<LootableWeapon>().codeName != pInventory.holsteredWeapon.codeName))
                {
                    //Debug.Log("other.GetComponent<LootableWeapon>().codeName");
                    Debug.Log(other.GetComponent<LootableWeapon>().codeName);
                    other.GetComponent<LootableWeapon>().OnLooted -= OnWeaponLooted;
                    other.GetComponent<LootableWeapon>().OnLooted += OnWeaponLooted;

                    //other.GetComponent<LootableWeapon>(). += OnWeaponLooted;

                    weaponsInRange.Add(other.GetComponent<LootableWeapon>());
                    weaponsInRange = weaponsInRange;
                }
                else
                {
                    LootableWeapon lw = other.GetComponent<LootableWeapon>();
                    lw.LootWeapon(player);

                    if (lw.isDw)
                    {
                        lw.OnLooted -= OnWeaponLooted;
                        lw.OnLooted += OnWeaponLooted;

                        weaponsInRange.Add(lw);
                        weaponsInRange = weaponsInRange;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(ex);
                //closestLootableWeapon = null;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<LootableWeapon>() && weaponsInRange.Contains(other.GetComponent<LootableWeapon>()))
        {
            other.GetComponent<LootableWeapon>().OnLooted -= OnWeaponLooted;

            weaponsInRange.Remove(other.GetComponent<LootableWeapon>());
            weaponsInRange = weaponsInRange;

            if (weaponsInRange.Count <= 0)
                closestLootableWeapon = null;
        }
    }

    void OnPLayerDeath(Player p)
    {
        Debug.Log("OnPLayerDeath");
        gameObject.layer = 3;
        weaponsInRange.Clear();
        weaponsInRange = weaponsInRange;
    }

    void OnPlayerRespawn(Player p)
    {
        gameObject.layer = _originalLayer;
    }

    // DEPRECATED. Look at PlayerInteractableObjectHandler instead
    void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        //if (!PV.IsMine)
        return;

        //Debug.Log("Om Player Long Interact Delegate");

        if (closestLootableWeapon)
        {
            int weaponCollidingWithInInventoryIndex = 0;
            for (int i = 0; i < pInventory.allWeaponsInInventory.Length; i++)
                if (weaponCollidingWithInInventory == pInventory.allWeaponsInInventory[i])
                    weaponCollidingWithInInventoryIndex = i;
            Vector3 lwPosition = closestLootableWeapon.GetComponent<LootableWeapon>().spawnPointPosition;

            if (closestLootableWeapon.isDw && pInventory.activeWeapon.isDualWieldable)
            {
                foreach (GameObject w in pInventory.allWeaponsInInventory)
                    if (w.GetComponent<WeaponProperties>().codeName == closestLootableWeapon.GetComponent<LootableWeapon>().codeName)
                    {
                        pInventory.leftWeapon = w.GetComponent<WeaponProperties>().leftWeapon;
                        pInventory.leftWeapon.loadedAmmo = closestLootableWeapon.networkAmmo;
                        pInventory.leftWeapon.spareAmmo = closestLootableWeapon.spareAmmo;
                    }
            }
            else
            {



                if (player.playerInteractableObjectHandler.closestInteractableObjectIsDualWieldableAndPartOfPlayerInventory)
                {
                    if (GetComponent<Player>().playerInventory.activeWeapon.isDualWieldable && (player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName.Equals(GetComponent<Player>().playerInventory.activeWeapon.codeName)))
                        PV.RPC("PickupThirdWeapon", RpcTarget.All, lwPosition, true);
                    else
                        ;
                }
                else if (!pInventory.holsteredWeapon) // Looks for Secondary Weapon
                {
                    //Debug.Log("RPC: Picking up second weapon");
                    //PickupSecWeap();
                    PV.RPC("PickupSecondWeapon", RpcTarget.All, lwPosition, weaponCollidingWithInInventoryIndex);
                    OnWeaponPickup?.Invoke(this);

                    pInventory.hasSecWeap = true;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().loadedAmmo = closestLootableWeapon.networkAmmo;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().spareAmmo = closestLootableWeapon.spareAmmo;
                }
                else if (pInventory.holsteredWeapon)
                {
                    if (player.GetComponent<PhotonView>().IsMine)
                    {
                        //Debug.Log("OnPlayerLongInteract_Delegate DropWeapon");
                        //player.DropWeaponOnDeath(pInventory.activeWeapon);
                        NetworkGameManager.SpawnNetworkWeapon(
                            pInventory.activeWeapon, player.weaponDropPoint.position, player.weaponDropPoint.forward);
                    }
                    PV.RPC("ReplaceWeapon", RpcTarget.All, lwPosition, weaponCollidingWithInInventoryIndex);
                    OnWeaponPickup?.Invoke(this);
                }
            }

            PV.RPC("RPC_DisableCollidingWeapon", RpcTarget.All, lwPosition);
        }
    }

    void OnWeaponLooted(LootableWeapon lw)
    {
        for (int i = 0; i < weaponsInRange.Count; i++)
            if (weaponsInRange[i] == lw)
            {
                weaponsInRange[i].GetComponent<LootableWeapon>().OnLooted -= OnWeaponLooted;
                weaponsInRange.Remove(weaponsInRange[i]);
                weaponsInRange = weaponsInRange;
            }
    }

    void OnWeaponDespawned(LootableWeapon lw)
    {

    }
    private void Update()
    {
        if (weaponsInRange.Count > 0)
        {
            for (int i = 0; i < weaponsInRange.Count; i++)
                if (weaponsInRange[i] == null || !weaponsInRange[i].gameObject.activeSelf)
                {
                    List<LootableWeapon> nl = weaponsInRange;
                    nl.RemoveAt(i);

                    weaponsInRange = nl;
                }
        }
    }







    [PunRPC]
    public void ReplaceWeapon(Vector3 collidingWeaponPosition, int weaponCollidingWithInInventoryIndex)
    {
        LootableWeapon weaponToLoot = null;
        foreach (LootableWeapon _allLootableWeapons in WeaponPool.instance.weaponPool)
            if (_allLootableWeapons.spawnPointPosition == collidingWeaponPosition)
                weaponToLoot = _allLootableWeapons;

        //Debug.Log($"ReplaceWeapon: weaponToLoot: {weaponToLoot.name}");

        WeaponProperties previousActiveWeapon = pInventory.activeWeapon;
        WeaponProperties newActiveWeapon = null;
        foreach (GameObject w in pInventory.allWeaponsInInventory)
            if (w.GetComponent<WeaponProperties>().codeName == weaponToLoot.codeName)
                newActiveWeapon = w.GetComponent<WeaponProperties>();

        //Debug.Log($"New Active weapon: {newActiveWeapon.name}");
        newActiveWeapon.gameObject.SetActive(true);
        previousActiveWeapon.gameObject.SetActive(false);

        //newActiveWeapon.currentAmmo = lw.ammoInThisWeapon;

        pInventory.activeWeapon = newActiveWeapon;
        pInventory.activeWeapon.loadedAmmo = weaponToLoot.networkAmmo;
        pInventory.activeWeapon.spareAmmo = weaponToLoot.spareAmmo;
        pInventory.player.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>Picked up a {pInventory.activeWeapon.cleanName}");
        //pInventory.holsteredWeapon = previousActiveWeapon;

        //StartCoroutine(pInventory.ToggleTPPistolIdle(1));
        //pInventory.changeAmmoCounter();
    }

    [PunRPC]
    public void PickupSecondWeapon(Vector3 collidingWeaponPosition, int weaponCollidingWithInInventoryIndex)
    {


        LootableWeapon lws = null;
        LootableWeapon[] weapons = FindObjectsOfType<LootableWeapon>();
        foreach (LootableWeapon lw in weapons)
            if (lw.spawnPointPosition == collidingWeaponPosition)
                lws = lw;

        WeaponProperties previousActiveWeapon = pInventory.activeWeapon;
        WeaponProperties newActiveWeapon = pInventory.allWeaponsInInventory[weaponCollidingWithInInventoryIndex].GetComponent<WeaponProperties>();
        newActiveWeapon.gameObject.SetActive(true);
        previousActiveWeapon.gameObject.SetActive(false);


        pInventory.holsteredWeapon = previousActiveWeapon;
        pInventory.activeWeapon = newActiveWeapon;

        pInventory.activeWeapon = weaponCollidingWithInInventory.GetComponent<WeaponProperties>();
        pInventory.activeWeapIs = 1;
        pInventory.hasSecWeap = true;

        weaponCollidingWithInInventory.GetComponent<WeaponProperties>().loadedAmmo = lws.networkAmmo;
        //pickupExtraAmmoFromWeapon(closestLootableWeapon);

        //Debug.Log("Replace Weapon 1");

        StartCoroutine(pInventory.ToggleTPPistolIdle(0));
        pInventory.ChangeActiveAmmoCounter();
    }



    [PunRPC]
    void PickupThirdWeapon(Vector3 collidingWeaponPosition, bool dw)
    {
        if (dw)
        {
            LootableWeapon weaponToLoot = WeaponPool.instance.weaponPool.Where(item => item.spawnPointPosition == collidingWeaponPosition).FirstOrDefault();

            foreach (GameObject w in pInventory.allWeaponsInInventory)
                if (w.GetComponent<WeaponProperties>().codeName == weaponToLoot.codeName)
                    player.playerInventory.thirdWeapon = w.GetComponent<WeaponProperties>();

            weaponToLoot.gameObject.SetActive(false);
        }
        else // its a heavy weapon
        {
            LootableWeapon weaponToLoot = WeaponPool.instance.weaponPool.Where(item => item.spawnPointPosition == collidingWeaponPosition).FirstOrDefault();

            foreach (GameObject w in pInventory.allWeaponsInInventory)
                if (w.GetComponent<WeaponProperties>().weaponType == WeaponProperties.WeaponType.Heavy && w.GetComponent<WeaponProperties>().codeName == weaponToLoot.codeName)
                {
                    player.playerInventory.thirdWeapon = w.GetComponent<WeaponProperties>();

                    pInventory.activeWeapon.loadedAmmo = weaponToLoot.networkAmmo;
                    pInventory.activeWeapon.spareAmmo = weaponToLoot.spareAmmo;
                    pInventory.player.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>Picked up a {pInventory.activeWeapon.cleanName}");


                    weaponToLoot.gameObject.SetActive(false);
                }
        }
    }





    [PunRPC]
    public void RPC_DisableCollidingWeapon(Vector3 collidingWeaponPosition)
    {
        Debug.Log($"RPC: Disabling lootable weapon: {collidingWeaponPosition}");
        //weaponPool.DisablePooledWeapon(collidingWeaponPosition);

        foreach (LootableWeapon lw in WeaponPool.instance.weaponPool)
            if (lw.spawnPointPosition == collidingWeaponPosition)
            {
                lw.HideWeapon();
                return;
            }
        Debug.Log($"RPC: FOUND NO WEAPON TO DISABLE");
    }

    // TO DO: make it across all the network
    public void PickupAmmoFromWeapon(GameObject weapon)
    {
        //Debug.Log("In Ammo Pickuo From Weapon: " + weapon);
        if (weapon.GetComponent<LootableWeapon>() != null)
        {
            LootableWeapon weaponScript = weapon.GetComponent<LootableWeapon>();

            //pickupExtraAmmoFromWeapon(weaponScript);
            int ammoAllowedToRemoveFromWeapon = pInventory.activeWeapon.maxSpareAmmo - pInventory.activeWeapon.spareAmmo;

            if (weaponScript.networkAmmo <= ammoAllowedToRemoveFromWeapon)
            {
                weaponScript.networkAmmo = 0;
                ammoPickupAudioSource.Play();
            }
            else if (weaponScript.networkAmmo > ammoAllowedToRemoveFromWeapon)
            {
                weaponScript.networkAmmo = weaponScript.networkAmmo - ammoAllowedToRemoveFromWeapon;
                ammoPickupAudioSource.Play();
            }

            if (weaponScript.networkAmmo == 0)
            {
                //Destroy(weapon.gameObject);
                weapon.SetActive(false);
                //Debug.Log("Destroyed Small Weapon");
            }
            if (weaponScript.networkAmmo == 0)
            {
                //Destroy(weapon.gameObject);
                weapon.SetActive(false);
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
    public void DisableAmmoPackWithRPC(int index)
    {
        if (!PV.IsMine)
            return;
        //Debug.Log("Disabling Ammo Pack");
        PV.RPC("DespawnAmmoPack_RPC", RpcTarget.All, index);
    }

    [PunRPC]
    void DespawnAmmoPack_RPC(int index)
    {
        //weaponPool.allAmmoPacks[index].GetComponent<NetworkGrenadeSpawnPoint>().onlineAmmoPackSpawnPoint.StartRespawn();
        //weaponPool.allAmmoPacks[index].SetActive(false);
    }
}
