using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInteractableObjectHandler : MonoBehaviour
{
    public delegate void PlayerInteractableObjectHandlerEvent(PlayerInteractableObjectHandler pioh);

    public PlayerInteractableObjectHandlerEvent ClosestInteractableObjectAssigned;



    [SerializeField] InteractableObject _closestInteractableObject;

    [SerializeField] List<InteractableObject> _rawInteractableObjects = new List<InteractableObject>();
    [SerializeField] List<InteractableObject> _filteredInteractableObjects = new List<InteractableObject>();

    [SerializeField] List<InteractableObject> _weaponsThePlayerHasInInventory = new List<InteractableObject>();
    [SerializeField] List<InteractableObject> _weaponsThePlayerHasInInventoryAndAreDualWieldable = new List<InteractableObject>();



    Player _player;
    List<InteractableObject> _preRawInteractableObjects = new List<InteractableObject>();
    InteractableObject _preClosestInteractableObject;
    float _check;










    public Player player { get { return _player; } }
    PhotonView PV { get { return GetComponent<PhotonView>(); } }
    public InteractableObject closestInteractableObject
    {
        get { return _closestInteractableObject; }
        set
        {
            _closestInteractableObject = value;

            //ClosestInteractableObjectAssigned?.Invoke(this);
        }
    }

    public bool closestInteractableObjectIsDualWieldableAndPartOfPlayerInventory { get { return _weaponsThePlayerHasInInventoryAndAreDualWieldable.Count > 0; } }
    public bool closestInteractableObjectIsDualWieldableAndActiveWeaponIsDualWieldableAlso
    {
        get
        {
            if (closestInteractableObject)
                return (closestInteractableObject.GetComponent<LootableWeapon>() && closestInteractableObject.GetComponent<LootableWeapon>().isDw && player.playerInventory.activeWeapon.isDualWieldable);

            return false;
        }
    }

    List<InteractableObject> rawInteractableObjects
    {
        get { return _rawInteractableObjects; }
        set
        {
            _rawInteractableObjects = value;



            if (!player.isDead && !player.isRespawning)
                Log.Print(() => $"_preRawInteractableObjects {_preRawInteractableObjects.Count}    rawInteractableObjects {_rawInteractableObjects.Count}");



            if (_preRawInteractableObjects != _rawInteractableObjects)
            {
                if (!player.isDead && !player.isRespawning)
                    Log.Print(() => "rawInteractableObjects CHANGE");



                _weaponsThePlayerHasInInventory.Clear(); _weaponsThePlayerHasInInventoryAndAreDualWieldable.Clear();
                if (rawInteractableObjects.Count > 0) _filteredInteractableObjects = new List<InteractableObject>(rawInteractableObjects);
                else _filteredInteractableObjects.Clear();


                if (_filteredInteractableObjects.Count > 0)
                {
                    _filteredInteractableObjects = _filteredInteractableObjects.Where(item => item.gameObject.activeSelf).ToList();
                    _filteredInteractableObjects = _filteredInteractableObjects.OrderBy(x => Vector3.Distance(this.transform.position, new Vector3(x.transform.position.x, transform.position.y, x.transform.position.z))).ToList();



                    if (!player.playerInventory.isHoldingHeavy)
                    {
                        for (int i = _filteredInteractableObjects.Count - 1; i >= 0; i--)
                        {
                            // Do something
                            if (_filteredInteractableObjects[i].GetComponent<LootableWeapon>())
                            {
                                if (_player.playerInventory.activeWeapon.codeName == _filteredInteractableObjects[i].GetComponent<LootableWeapon>().codeName ||
                                    _player.playerInventory.holsteredWeapon.codeName == _filteredInteractableObjects[i].GetComponent<LootableWeapon>().codeName ||
                                  (_player.playerInventory.thirdWeapon && _player.playerInventory.thirdWeapon.codeName == _filteredInteractableObjects[i].GetComponent<LootableWeapon>().codeName))
                                {
                                    _weaponsThePlayerHasInInventory.Add(_filteredInteractableObjects[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        _filteredInteractableObjects = _filteredInteractableObjects.Where(item => !item.GetComponent<LootableWeapon>()).ToList();
                    }
                }





                _preClosestInteractableObject = _closestInteractableObject;

                if (_filteredInteractableObjects.Count > 0) _closestInteractableObject = _filteredInteractableObjects[0]; else _closestInteractableObject = null;

                if (_preClosestInteractableObject != _closestInteractableObject)
                {
                    //if(_preClosestInteractableObject.GetComponent<LootableWeapon>()) _preClosestInteractableObject.GetComponent<LootableWeapon>()
                    ClosestInteractableObjectAssigned?.Invoke(this);
                }
            }
        }
    }













    private void Awake()
    {
        _check = 0.3f;
    }

    private void Start()
    {
        _player = transform.root.GetComponent<Player>();

        player.playerController.OnPlayerShortPressInteract += OnPlayerLongInteract_Delegate;
    }

    private void Update()
    {
        if (player.isDead || player.isRespawning)
        {
            _rawInteractableObjects.Clear();
            rawInteractableObjects = rawInteractableObjects;
        }
        else
        {
            if (_rawInteractableObjects.Count > 0)
                for (int i = _rawInteractableObjects.Count - 1; i >= 0; i--)
                {
                    // check for disabled

                    if (!_rawInteractableObjects[i].gameObject.activeSelf || !_rawInteractableObjects[i].gameObject.activeInHierarchy ||
                        (_rawInteractableObjects[i].GetComponent<Collider>() && _rawInteractableObjects[i].GetComponent<Collider>().isTrigger && !_rawInteractableObjects[i].GetComponent<Collider>().enabled))
                    {
                        _preRawInteractableObjects = new List<InteractableObject>(_rawInteractableObjects);
                        _rawInteractableObjects.RemoveAt(i);
                        rawInteractableObjects = _rawInteractableObjects;
                    }
                }


            if (_check > 0)
            {

                _check -= Time.deltaTime;

                if (_check <= 0)
                {
                    _check = 0.3f;
                }
            }


            {// TRANSFER AMMO
                if (_weaponsThePlayerHasInInventory.Count > 0)
                    for (int i = _weaponsThePlayerHasInInventory.Count - 1; i >= 0; i--)
                        if (_weaponsThePlayerHasInInventory[i].GetComponent<LootableWeapon>())
                            _weaponsThePlayerHasInInventory[i].GetComponent<LootableWeapon>().LootWeapon(_player);

                if (player.playerInventory.isDualWielding)
                    if (_weaponsThePlayerHasInInventory.Count > 0)
                        for (int i = _weaponsThePlayerHasInInventory.Count - 1; i >= 0; i--)
                            if (_weaponsThePlayerHasInInventory[i].GetComponent<LootableWeapon>())
                                _weaponsThePlayerHasInInventory[i].GetComponent<LootableWeapon>().LootWeapon(_player, thirdWeapon: true);
            }




            if (_filteredInteractableObjects.Count > 0)
            {
                // order by closest closest
                if (_filteredInteractableObjects.Count > 1)
                    _filteredInteractableObjects = _filteredInteractableObjects.OrderBy(x => Vector3.Distance(this.transform.position, new Vector3(x.transform.position.x, transform.position.y, x.transform.position.z))).ToList();





                _preClosestInteractableObject = _closestInteractableObject;
                closestInteractableObject = _filteredInteractableObjects[0];

                if (_preClosestInteractableObject != _closestInteractableObject)
                    ClosestInteractableObjectAssigned?.Invoke(this);
            }
        }
    }




    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<InteractableObject>() && !_rawInteractableObjects.Contains(other.GetComponent<InteractableObject>()))
        {
            //PrintOnlyInEditor.Log($"PlayerInteractableObjectHandler OnTriggerStay addind: {other.GetComponent<InteractableObject>().name}");

            _preRawInteractableObjects = new List<InteractableObject>(_rawInteractableObjects);

            _rawInteractableObjects.Add(other.GetComponent<InteractableObject>());
            rawInteractableObjects = _rawInteractableObjects;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<InteractableObject>())
            if (_rawInteractableObjects.Contains(other.GetComponent<InteractableObject>()))
            {
                _preRawInteractableObjects = new List<InteractableObject>(_rawInteractableObjects);

                _rawInteractableObjects.Remove(other.GetComponent<InteractableObject>());
                rawInteractableObjects = _rawInteractableObjects;
            }
    }




    int weaponCollidingWithInInventoryIndex;
    Vector3 lwPosition;
    void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        if (!player.PV.IsMine || player.hasEnnemyFlag || player.playerInventory.playerOddballActive)
            return;

        Log.Print(() => $"PlayerInteractableObjectHandler OnPlayerLongInteract_Delegate");

        if (closestInteractableObject  /* && !closestInteractableObjectIsDualWieldableAndPartOfPlayerInventory*/ /*&& !closestInteractableObjectIsDualWieldableAndActiveWeaponIsDualWieldableAlso*/)
        {
            playerController.ResetLongInteractFrameCounter(PlayerController.InteractResetMode.activeweapon);

            if (closestInteractableObject.GetComponent<LootableWeapon>())
            {
                if (!closestInteractableObjectIsDualWieldableAndActiveWeaponIsDualWieldableAlso)
                {
                    if (!_player.isDualWielding)
                    {
                        if (closestInteractableObject.GetComponent<LootableWeapon>().codeName != player.playerInventory.activeWeapon.codeName
                        && closestInteractableObject.GetComponent<LootableWeapon>().codeName != player.playerInventory.holsteredWeapon.codeName)
                        {
                            weaponCollidingWithInInventoryIndex = -1;
                            for (int i = 0; i < player.playerInventory.allWeaponsInInventory.Length; i++)
                                if (closestInteractableObject.GetComponent<LootableWeapon>().codeName.Equals(player.playerInventory.allWeaponsInInventory[i].GetComponent<WeaponProperties>().codeName))
                                    weaponCollidingWithInInventoryIndex = i;
                            lwPosition = closestInteractableObject.GetComponent<LootableWeapon>().spawnPointPosition;


                            Log.Print(() => $"{player.playerInventory.allWeaponsInInventory[weaponCollidingWithInInventoryIndex].GetComponent<WeaponProperties>().weaponType.ToString()}");

                            if (player.playerInventory.allWeaponsInInventory[weaponCollidingWithInInventoryIndex].GetComponent<WeaponProperties>().weaponType == WeaponProperties.WeaponType.Heavy)
                            {
                                Log.Print(() => "Picking up a heavy weapon");

                                player.playerController.ResetLongInteractFrameCounter(PlayerController.InteractResetMode.thirdweapon);
                                PV.RPC("PickupThirdWeapon", RpcTarget.All, closestInteractableObject.GetComponent<LootableWeapon>().spawnPointPosition, false);
                                player.playerController.ResetLongInteractFrameCounter(PlayerController.InteractResetMode.thirdweapon);
                            }
                            else if (!player.playerInventory.holsteredWeapon) // Looks for Secondary Weapon
                            {
                                //Log.Print(() =>"RPC: Picking up second weapon");
                                //PickupSecWeap();
                                PV.RPC("PickupSecondWeapon", RpcTarget.All, lwPosition, weaponCollidingWithInInventoryIndex);
                                //OnWeaponPickup?.Invoke(this);

                                player.playerInventory.hasSecWeap = true;
                                player.playerInventory.activeWeapon.GetComponent<WeaponProperties>().loadedAmmo = closestInteractableObject.GetComponent<LootableWeapon>().networkAmmo;
                                player.playerInventory.activeWeapon.GetComponent<WeaponProperties>().spareAmmo = closestInteractableObject.GetComponent<LootableWeapon>().spareAmmo;
                            }
                            else if (player.playerInventory.holsteredWeapon)
                            {
                                if (player.GetComponent<PhotonView>().IsMine)
                                {
                                    //Log.Print(() =>"OnPlayerLongInteract_Delegate DropWeapon");
                                    //player.DropWeaponOnDeath(pInventory.activeWeapon);
                                    NetworkGameManager.SpawnNetworkWeapon(
                                        player.playerInventory.activeWeapon, player.weaponDropPoint.position, player.weaponDropPoint.forward);
                                }
                                PV.RPC("ReplaceWeapon", RpcTarget.All, lwPosition, weaponCollidingWithInInventoryIndex);
                                //OnWeaponPickup?.Invoke(this);
                            }

                            PV.RPC("DisableCollidingWeapon_RPC", RpcTarget.All, lwPosition);
                        }
                    }
                }
                else
                {
                    if (closestInteractableObject.GetComponent<LootableWeapon>().codeName != player.playerInventory.activeWeapon.codeName
                        && closestInteractableObject.GetComponent<LootableWeapon>().codeName != player.playerInventory.holsteredWeapon.codeName)
                    {
                        int weaponCollidingWithInInventoryIndex = 0;
                        for (int i = 0; i < player.playerInventory.allWeaponsInInventory.Length; i++)
                            if (closestInteractableObject.gameObject == player.playerInventory.allWeaponsInInventory[i])
                                weaponCollidingWithInInventoryIndex = i;
                        Vector3 lwPosition = closestInteractableObject.GetComponent<LootableWeapon>().spawnPointPosition;

                        if (player.GetComponent<PhotonView>().IsMine)
                        {
                            //Log.Print(() =>"OnPlayerLongInteract_Delegate DropWeapon");
                            //player.DropWeaponOnDeath(pInventory.activeWeapon);
                            NetworkGameManager.SpawnNetworkWeapon(
                                player.playerInventory.activeWeapon, player.weaponDropPoint.position, player.weaponDropPoint.forward);
                        }
                        PV.RPC("ReplaceWeapon", RpcTarget.All, lwPosition, weaponCollidingWithInInventoryIndex);
                        PV.RPC("DisableCollidingWeapon_RPC", RpcTarget.All, lwPosition);
                    }
                }
            }
            else if (closestInteractableObject.GetComponent<ArmorSeller>())
            {
                if (!transform.root.GetComponent<Player>().hasArmor)
                {
                    if (transform.root.GetComponent<PlayerSwarmMatchStats>().points >= closestInteractableObject.GetComponent<ArmorSeller>().cost)
                        NetworkGameManager.instance.AskHostToTriggerInteractableObject(closestInteractableObject.transform.position, playerController.player.photonId);
                }
            }

            playerController.ResetLongInteractFrameCounter(PlayerController.InteractResetMode.activeweapon);
        }
    }


    public void TriggerLongInteract()
    {
        if (closestInteractableObject) Log.Print(() => $"TriggerLongInteract {closestInteractableObjectIsDualWieldableAndActiveWeaponIsDualWieldableAlso}");

        if (!player.PV.IsMine || player.hasEnnemyFlag || player.playerInventory.playerOddballActive) return;

        if (PV.IsMine && closestInteractableObjectIsDualWieldableAndActiveWeaponIsDualWieldableAlso && !player.isDualWielding)
        {
            player.playerController.ResetLongInteractFrameCounter(PlayerController.InteractResetMode.thirdweapon);

            if (player.playerInventory.activeWeapon.isDualWieldable /*&& (closestInteractableObject.GetComponent<LootableWeapon>().codeName.Equals(player.playerInventory.activeWeapon.codeName))*/)
                player.playerInventory.PV.RPC("PickupThirdWeapon", RpcTarget.All, closestInteractableObject.GetComponent<LootableWeapon>().spawnPointPosition, true);

            player.playerController.ResetLongInteractFrameCounter(PlayerController.InteractResetMode.thirdweapon);
        }
    }




    [PunRPC]
    public void DisableCollidingWeapon_RPC(Vector3 collidingWeaponPosition)
    {
        Log.Print(() =>$"RPC: Disabling lootable weapon: {collidingWeaponPosition}");
        //weaponPool.DisablePooledWeapon(collidingWeaponPosition);

        foreach (LootableWeapon lw in WeaponPool.instance.weaponPool)
            if (lw.spawnPointPosition == collidingWeaponPosition)
            {
                lw.HideWeapon();
                return;
            }
        Log.Print(() =>$"RPC: FOUND NO WEAPON TO DISABLE");
    }
}
