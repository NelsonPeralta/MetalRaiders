using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MysteryBox : InteractableObject
{
    [SerializeField] int _cost;
    [SerializeField] Transform _spawnFor;
    [SerializeField] Animator _animator;


    [SerializeField] List<Player> playersInRange = new List<Player>();



    int _weapIndd;
    float _lockedTime;






    public int cost { get { return _cost; } }




    private void Awake()
    {
        _animator.enabled = false;
    }


    private void Update()
    {
        if (_lockedTime > 0)
        {
            _lockedTime -= Time.deltaTime;

            if (_lockedTime <= 0)
            {
                _animator.enabled = false;
            }
        }
    }



    private void OnTriggerStay(Collider other)
    {
        if (_lockedTime > 0) return;

        if (other.GetComponent<PlayerCapsule>() && !other.transform.root.GetComponent<Player>().isDead && !playersInRange.Contains(other.transform.root.GetComponent<Player>()))
        {
            playersInRange.Add(other.transform.root.GetComponent<Player>());

            if (other.transform.root.GetComponent<PlayerSwarmMatchStats>().points >= cost)
            {
                other.transform.root.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
                other.transform.root.GetComponent<PlayerController>().OnPlayerLongInteract += OnPlayerLongInteract_Delegate;

                other.transform.root.GetComponent<PlayerUI>().ShowInformer($"Mystery Box [Cost: {cost}]");
            }
            else
            {
                other.transform.root.GetComponent<PlayerUI>().ShowInformer($"Not enough points [Cost: {cost}]");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        try
        {
            other.transform.root.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
            playersInRange.Remove(other.transform.root.GetComponent<Player>());

            other.transform.root.GetComponent<PlayerUI>().HideInformer();
        }
        catch { }
    }

    void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        if (_lockedTime > 0) return;

        if (playerController.GetComponent<PlayerSwarmMatchStats>().points >= _cost)
        {
            print("MysteryBox OnPlayerLongInteract_Delegate");
            NetworkGameManager.instance.AskHostToTriggerInteractableObject(transform.position, WeaponPool.instance.GetMysteryBoxWeaponInd());
        }
    }


    public override void Trigger(int? pid)
    {
        print("MysteryBox Trigger");
        _weapIndd = (int)pid;
        _lockedTime = 5;
        _animator.enabled = true;
        _animator.Play("Entry");
    }


    public void SpitWeapon()
    {
        if (_weapIndd > 0)
        {
            LootableWeapon weapon = WeaponPool.instance.GetWeaponAtInd(_weapIndd);
            print($"SpitWeapon: {weapon.name}");

            weapon.transform.position = _spawnFor.position;
            weapon.transform.rotation = _spawnFor.rotation;
            weapon.gameObject.SetActive(true);
            weapon.GetComponent<Rigidbody>().velocity = Vector3.zero;
            weapon.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            weapon.GetComponent<Rigidbody>().AddForce(_spawnFor.forward * GameManager.WEAPON_DROP_FORCE * 2);

            _weapIndd = -1;
        }
    }
}
