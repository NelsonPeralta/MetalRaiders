using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.ObjectModel;
using TMPro;

public class PlayerWeaponSwapping : MonoBehaviourPun
{
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
    public TMP_Text pickupText;
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
            if (value)
            {
                WeaponProperties wp = player.playerInventory.GetWeaponProperties(closestLootableWeapon.codeName);
                if (wp.weaponIcon)
                {
                    pickupText.text = $"Hold E to pick up";
                    pickupText.GetComponentInChildren<Image>().sprite = wp.weaponIcon;
                    var tempColor = pickupText.GetComponentInChildren<Image>().color;
                    tempColor.a = 255;
                    pickupText.GetComponentInChildren<Image>().color = tempColor;
                }
                else
                    pickupText.text = "Hold E to pick up " + closestLootableWeapon.cleanName;
            }
            else
            {
                pickupText.text = "";
                pickupText.GetComponentInChildren<Image>().sprite = null;
                var tempColor = pickupText.GetComponentInChildren<Image>().color;
                tempColor.a = 0;
                pickupText.GetComponentInChildren<Image>().color = tempColor;
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
        pickupText.text = "";
        player.OnPlayerDeath -= OnPLayerDeath;
        player.OnPlayerDeath += OnPLayerDeath;

        weaponPool = FindObjectOfType<WeaponPool>();

        pController.OnPlayerLongInteract += OnPlayerLongInteract_Delegate;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<LootableWeapon>() && other.gameObject.activeSelf)
        {
            //if (player.isDead ||
            //weaponsInRange.Contains(other.GetComponent<LootableWeapon>()) ||
            //(pInventory.activeWeapon && other.GetComponent<LootableWeapon>().codeName == pInventory.activeWeapon.codeName) ||
            //    (pInventory.holsteredWeapon && other.GetComponent<LootableWeapon>().codeName == pInventory.holsteredWeapon.codeName))
            //    return;

            if (player.isDead || weaponsInRange.Contains(other.GetComponent<LootableWeapon>()))
                return;

            if ((pInventory.activeWeapon && other.GetComponent<LootableWeapon>().codeName != pInventory.activeWeapon.codeName) &&
                (pInventory.holsteredWeapon && other.GetComponent<LootableWeapon>().codeName != pInventory.holsteredWeapon.codeName))
            {
                other.GetComponent<LootableWeapon>().OnLooted -= OnWeaponLooted;
                other.GetComponent<LootableWeapon>().OnLooted += OnWeaponLooted;

                weaponsInRange.Add(other.GetComponent<LootableWeapon>());
                weaponsInRange = weaponsInRange;
            }
            else
            {
                other.GetComponent<LootableWeapon>().LootWeapon();
                ammoPickupAudioSource.Play();
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
        weaponsInRange.Clear();
        weaponsInRange = weaponsInRange;
    }
    void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        if (!PV.IsMine)
            return;

        Debug.Log("Om Player Long Interact Delegate");

        if (closestLootableWeapon)
        {
            int weaponCollidingWithInInventoryIndex = 0;
            for (int i = 0; i < pInventory.allWeaponsInInventory.Length; i++)
                if (weaponCollidingWithInInventory == pInventory.allWeaponsInInventory[i])
                    weaponCollidingWithInInventoryIndex = i;
            Vector3 lwPosition = closestLootableWeapon.GetComponent<LootableWeapon>().spawnPointPosition;
            if (!pInventory.holsteredWeapon) // Looks for Secondary Weapon
            {
                Debug.Log("RPC: Picking up second weapon");
                //PickupSecWeap();
                PV.RPC("PickupSecondWeapon", RpcTarget.All, lwPosition, weaponCollidingWithInInventoryIndex);
                OnWeaponPickup?.Invoke(this);

                pInventory.hasSecWeap = true;
                pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = closestLootableWeapon.ammoInThisWeapon;

                pInventory.PlayDrawSound();
            }
            else if (pInventory.holsteredWeapon)
            {
                if (player.GetComponent<PhotonView>().IsMine)
                {
                    Debug.Log("OnPlayerLongInteract_Delegate DropWeapon");
                    player.DropWeapon(pInventory.activeWeapon);
                }
                PV.RPC("ReplaceWeapon", RpcTarget.All, lwPosition, weaponCollidingWithInInventoryIndex);
                OnWeaponPickup?.Invoke(this);

                pInventory.PlayDrawSound();
            }
            Debug.Log("RPC: Calling RPC_DisableCollidingWeapon");
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


    private void Update()
    {

    }







    [PunRPC]
    public void ReplaceWeapon(Vector3 collidingWeaponPosition, int weaponCollidingWithInInventoryIndex)
    {
        LootableWeapon lw = null;
        LootableWeapon[] weapons = FindObjectsOfType<LootableWeapon>();
        foreach (LootableWeapon w in weapons)
            if (w.spawnPointPosition == collidingWeaponPosition)
                lw = w;


        WeaponProperties previousActiveWeapon = pInventory.activeWeapon;
        WeaponProperties newActiveWeapon = null;
        foreach (GameObject w in pInventory.allWeaponsInInventory)
            if (w.GetComponent<WeaponProperties>().codeName == lw.codeName)
                newActiveWeapon = w.GetComponent<WeaponProperties>();

        Debug.Log($"New Active weapon: {newActiveWeapon.name}");
        newActiveWeapon.gameObject.SetActive(true);
        previousActiveWeapon.gameObject.SetActive(false);

        //newActiveWeapon.currentAmmo = lw.ammoInThisWeapon;

        pInventory.activeWeapon = newActiveWeapon;
        pInventory.activeWeapon.currentAmmo = lw.ammoInThisWeapon;
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

        weaponCollidingWithInInventory.GetComponent<WeaponProperties>().currentAmmo = lws.ammoInThisWeapon;
        //pickupExtraAmmoFromWeapon(closestLootableWeapon);

        Debug.Log("Replace Weapon 1");

        StartCoroutine(pInventory.ToggleTPPistolIdle(0));
        pInventory.ChangeActiveAmmoCounter();
    }

    [PunRPC]
    public void RPC_DisableCollidingWeapon(Vector3 collidingWeaponPosition)
    {
        Debug.Log("RPC: Disabling lootable weapon");
        //weaponPool.DisablePooledWeapon(collidingWeaponPosition);

        LootableWeapon[] weapons = FindObjectsOfType<LootableWeapon>();
        foreach (LootableWeapon lw in weapons)
            if (lw.spawnPointPosition == collidingWeaponPosition)
                lw.LootWeapon(onlyExtraAmmo: true);
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
                //pickupExtraAmmoFromWeapon(weaponScript);
                int ammoAllowedToRemoveFromWeapon = pInventory.activeWeapon.maxAmmo - pInventory.activeWeapon.spareAmmo;

                if (weaponScript.ammoInThisWeapon <= ammoAllowedToRemoveFromWeapon)
                {
                    weaponScript.ammoInThisWeapon = 0;
                    ammoPickupAudioSource.Play();
                }
                else if (weaponScript.ammoInThisWeapon > ammoAllowedToRemoveFromWeapon)
                {
                    weaponScript.ammoInThisWeapon = weaponScript.ammoInThisWeapon - ammoAllowedToRemoveFromWeapon;
                    ammoPickupAudioSource.Play();
                }

                if (weaponScript.ammoInThisWeapon == 0)
                {
                    //Destroy(weapon.gameObject);
                    weapon.SetActive(false);
                    Debug.Log("Destroyed Small Weapon");
                }
                if (weaponScript.ammoInThisWeapon == 0)
                {
                    //Destroy(weapon.gameObject);
                    weapon.SetActive(false);
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
