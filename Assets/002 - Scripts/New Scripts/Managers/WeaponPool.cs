using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class WeaponPool : MonoBehaviour
{
    public static WeaponPool weaponPoolInstance;
    public int amountToPool;

    public List<GameObject> allWeapons = new List<GameObject>();
    public List<GameObject> weaponPrefabs = new List<GameObject>();

    private void Awake()
    {
        if (weaponPoolInstance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        weaponPoolInstance = this;
    }

    private void Start()
    {
        for (int i = 0; i < weaponPrefabs.Count; i++)
        {
            for (int j = 0; j < amountToPool; j++)
            {
                // TO DO: Spawn them using normal instantiate instead and fix LootableWeapon script according to it (Start method)
                GameObject newWeap = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", weaponPrefabs[i].name), Vector3.zero, Quaternion.identity);
                newWeap.name = newWeap.name.Replace("(Clone)", "");
                newWeap.SetActive(false);
                allWeapons.Add(newWeap);
                newWeap.transform.parent = gameObject.transform;
            }
        }
    }

    public GameObject GetWeaponFromList(string weaponName)
    {
        for (int i = 0; i < allWeapons.Count; i++)
        {
            if (allWeapons[i].name == weaponName && !allWeapons[i].activeSelf)
                return allWeapons[i];
        }
        return null;
    }

    public int GetWeaponIndex(GameObject weapon)
    {
        for (int i = 0; i < allWeapons.Count; i++)
        {
            if (allWeapons[i] == weapon)
                return i;
        }
        return 0;
    }

    public LootableWeapon GetLootableWeaponScript(int index)
    {
        return allWeapons[index].GetComponent<LootableWeapon>();
    }

    public void DisablePooledWeapon(int index)
    {
        allWeapons[index].SetActive(false);
    }
}
