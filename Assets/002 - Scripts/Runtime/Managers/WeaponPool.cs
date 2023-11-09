using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;


// Used to spawn weapons at runtime
public class WeaponPool : MonoBehaviourPun
{
    public static WeaponPool instance;

    public GameObject weaponCollisionSmoke;

    int amountOfWeaponsToPool;

    [SerializeField] List<LootableWeapon> _weaponPrefabs = new List<LootableWeapon>();
    [SerializeField] List<LootableWeapon> _spawnedWeapons = new List<LootableWeapon>();


    private void Awake()
    {
        amountOfWeaponsToPool = 100;
        instance = this;
    }

    private void Start()
    {
        Debug.Log("WeaponPool");
        foreach (var weapon in _weaponPrefabs)
            for (int j = 0; j < amountOfWeaponsToPool; j++)
            {
                LootableWeapon newWeap = Instantiate(weapon, transform);

                newWeap.gameObject.SetActive(false);
                newWeap.name = newWeap.name.Replace("(Clone)", "");
                newWeap.transform.parent = gameObject.transform;

                _spawnedWeapons.Add(newWeap);
            }
    }



    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 0)
        {
            foreach (LootableWeapon weapon in _spawnedWeapons) weapon.gameObject.SetActive(false);
        }
        else
        {

        }
    }


    public LootableWeapon GetLootableWeapon(string n)
    {
        for (int i = 0; i < _spawnedWeapons.Count; i++)
        {
            if (_spawnedWeapons[i].codeName.Equals(n) && !_spawnedWeapons[i].gameObject.activeInHierarchy)
            {
                Debug.Log($"GetLootableWeapon FOUND!");
                //_spawnedWeapons.Remove(weapon);
                return _spawnedWeapons[i];
            }
        }
        //foreach (LootableWeapon weapon in _spawnedWeapons)
        //{
        //    if (weapon.codeName.Equals(n) && !weapon.gameObject.activeInHierarchy)
        //    {
        //        Debug.Log($"GetLootableWeapon FOUND!");
        //        //_spawnedWeapons.Remove(weapon);
        //        return weapon;
        //    }
        //}

        return null;
    }
}
