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

    public List<LootableWeapon> weaponPool { get { return _spawnedWeapons; } }
    int amountOfWeaponsToPool, _c;

    [SerializeField] List<LootableWeapon> _weaponPrefabs = new List<LootableWeapon>();
    [SerializeField] List<LootableWeapon> _spawnedWeapons = new List<LootableWeapon>();


    private void Awake()
    {
        amountOfWeaponsToPool = 50;
        instance = this;
    }

    private void Start()
    {
        foreach (var weapon in _weaponPrefabs)
        {

            for (int j = 0; j < amountOfWeaponsToPool; j++)
            {
                LootableWeapon newWeap = Instantiate(weapon, transform);

                newWeap.gameObject.SetActive(false);
                newWeap.name = newWeap.name.Replace("(Clone)", "");
                newWeap.transform.parent = gameObject.transform;
                newWeap.SetSpawnPositionIdentity(Vector3.up * -(_c + 1));

                _spawnedWeapons.Add(newWeap);
                _c++;
            }
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


    public LootableWeapon GetLootableWeapon(string weaponCodeName)
    {
        for (int i = 0; i < _spawnedWeapons.Count; i++)
        {
            if (_spawnedWeapons[i].codeName.Equals(weaponCodeName) && !_spawnedWeapons[i].gameObject.activeInHierarchy
                && _spawnedWeapons[i].transform.parent == transform)
            {
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
