using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class PlayerInteractableObjectHandler : MonoBehaviour
{
    [SerializeField] InteractableObject _closestInteractableObject;

    [SerializeField] List<InteractableObject> _rawInteractableObjects = new List<InteractableObject>();
    [SerializeField] List<InteractableObject> _filteredInteractableObjects = new List<InteractableObject>();

    [SerializeField] List<InteractableObject> _weaponsThePlayerHasInInventory = new List<InteractableObject>();




    Player _player;
    List<InteractableObject> _preRawInteractableObjects = new List<InteractableObject>();
    InteractableObject _preClosestInteractableObject;
    float _check;










    public Player player { get { return _player; } }
    PhotonView PV { get { return GetComponent<PhotonView>(); } }
    public InteractableObject closestInteractableObject { get { return _closestInteractableObject; } set { _closestInteractableObject = value; } }

    List<InteractableObject> rawInteractableObjects
    {
        get { return _rawInteractableObjects; }
        set
        {
            _rawInteractableObjects = value;

            print($"_preRawInteractableObjects {_preRawInteractableObjects.Count}    rawInteractableObjects {_rawInteractableObjects.Count}");
            if (_preRawInteractableObjects.Count != _rawInteractableObjects.Count)
            {
                print("rawInteractableObjects CHANGE");



                _weaponsThePlayerHasInInventory.Clear();
                _filteredInteractableObjects = new List<InteractableObject>(rawInteractableObjects);

                _filteredInteractableObjects = _filteredInteractableObjects.Where(item => item.gameObject.activeSelf).ToList();
                _filteredInteractableObjects.OrderBy(x => Vector3.Distance(this.transform.position, new Vector3(x.transform.position.x, transform.position.y, x.transform.position.z))).ToList();


                for (int i = _filteredInteractableObjects.Count - 1; i >= 0; i--)
                {
                    // Do something
                    if (_filteredInteractableObjects[i].GetComponent<LootableWeapon>())
                    {
                        if (_player.playerInventory.activeWeapon.codeName == _filteredInteractableObjects[i].GetComponent<LootableWeapon>().codeName ||
                            _player.playerInventory.holsteredWeapon.codeName == _filteredInteractableObjects[i].GetComponent<LootableWeapon>().codeName)
                        {
                            _weaponsThePlayerHasInInventory.Add(_filteredInteractableObjects[i]);
                            _filteredInteractableObjects.RemoveAt(i);
                        }
                    }
                }





                _preClosestInteractableObject = _closestInteractableObject;

                if (_filteredInteractableObjects.Count > 0) _closestInteractableObject = _filteredInteractableObjects[0]; else _closestInteractableObject = null;

                if (_preClosestInteractableObject != _closestInteractableObject)
                {
                    //if(_preClosestInteractableObject.GetComponent<LootableWeapon>()) _preClosestInteractableObject.GetComponent<LootableWeapon>()
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

        player.playerController.OnPlayerLongInteract += OnPlayerLongInteract_Delegate;
    }

    private void Update()
    {

        for (int i = _rawInteractableObjects.Count - 1; i >= 0; i--)
        {
            // check for disabled

            if (!_rawInteractableObjects[i].gameObject.activeSelf || !_rawInteractableObjects[i].gameObject.activeInHierarchy)
            {
                _preRawInteractableObjects = new List<InteractableObject>(_rawInteractableObjects);
                _rawInteractableObjects.RemoveAt(i);
                rawInteractableObjects = _rawInteractableObjects;
            }
        }





        if (player.isDead || player.isRespawning)
        {
            _rawInteractableObjects.Clear();
            rawInteractableObjects = rawInteractableObjects;
        }
        else
        {
            if (_check > 0)
            {

                _check -= Time.deltaTime;

                if (_check <= 0)
                {
                    _check = 0.3f;
                }
            }


            if (_weaponsThePlayerHasInInventory.Count > 0)
            {
                for (int i = _weaponsThePlayerHasInInventory.Count - 1; i >= 0; i--)
                {
                    // Do something
                    if (_weaponsThePlayerHasInInventory[i].GetComponent<LootableWeapon>())
                    {
                        _weaponsThePlayerHasInInventory[i].GetComponent<LootableWeapon>().LootWeapon(_player);
                    }
                }
            }

            if (_filteredInteractableObjects.Count > 0)
            {
                if (_filteredInteractableObjects[0].GetComponent<LootableWeapon>())
                {
                    //_filteredInteractableObjects[0].GetComponent<LootableWeapon>().OnLooted -= OnWeaponLooted;
                    //_filteredInteractableObjects[0].GetComponent<LootableWeapon>().OnLooted += OnWeaponLooted;
                }


                _closestInteractableObject = _filteredInteractableObjects[0];
            }
        }
    }




    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<InteractableObject>() && !_rawInteractableObjects.Contains(other.GetComponent<InteractableObject>()))
        {
            print($"PlayerInteractableObjectHandler OnTriggerStay addind: {other.GetComponent<InteractableObject>().name}");


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



    void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        if (!player.PV.IsMine)
            return;

        print($"PlayerInteractableObjectHandler OnPlayerLongInteract_Delegate");

        if (closestInteractableObject)
        {
            if (closestInteractableObject.GetComponent<LootableWeapon>())
            {
                int weaponCollidingWithInInventoryIndex = 0;
                for (int i = 0; i < player.playerInventory.allWeaponsInInventory.Length; i++)
                    if (closestInteractableObject.gameObject == player.playerInventory.allWeaponsInInventory[i])
                        weaponCollidingWithInInventoryIndex = i;
                Vector3 lwPosition = closestInteractableObject.GetComponent<LootableWeapon>().spawnPointPosition;



                if (!player.playerInventory.holsteredWeapon) // Looks for Secondary Weapon
                {
                    //Debug.Log("RPC: Picking up second weapon");
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
                        //Debug.Log("OnPlayerLongInteract_Delegate DropWeapon");
                        //player.DropWeaponOnDeath(pInventory.activeWeapon);
                        NetworkGameManager.SpawnNetworkWeapon(
                            player.playerInventory.activeWeapon, player.weaponDropPoint.position, player.weaponDropPoint.forward);
                    }
                    PV.RPC("ReplaceWeapon", RpcTarget.All, lwPosition, weaponCollidingWithInInventoryIndex);
                    //OnWeaponPickup?.Invoke(this);
                }

                PV.RPC("RPC_DisableCollidingWeapon", RpcTarget.All, lwPosition);
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
}
