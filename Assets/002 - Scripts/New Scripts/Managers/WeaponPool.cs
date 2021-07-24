using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class WeaponPool : MonoBehaviourPun
{
    public static WeaponPool weaponPoolInstance;
    public static OnlineGameTime onlineGameTimeInstance;
    public int amountToPool;

    [Header("Timer")]
    public GameObject timerPrefab;
    public GameObject timer;

    [Header("Weapons")]
    public List<GameObject> allWeapons = new List<GameObject>();
    public List<GameObject> weaponPrefabs = new List<GameObject>();

    [Header("Ammo")]
    public List<GameObject> allAmmoPacks = new List<GameObject>();
    public List<GameObject> ammoPackPrefabs = new List<GameObject>();

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
        onlineGameTimeInstance = OnlineGameTime.onlineGameTimeInstance;

        for (int i = 0; i < weaponPrefabs.Count; i++)
            for (int j = 0; j < amountToPool; j++)
            {
                // TO DO: Spawn them using normal instantiate instead and fix LootableWeapon script according to it (Start method)
                GameObject newWeap = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", weaponPrefabs[i].name), Vector3.zero, Quaternion.identity);
                //GameObject newWeap = Instantiate(weaponPrefabs[i], transform.position + new Vector3(0 - 100, 0), transform.rotation);
                newWeap.name = newWeap.name.Replace("(Clone)", "");
                newWeap.SetActive(false);
                allWeapons.Add(newWeap);
                newWeap.transform.parent = gameObject.transform;
            }

        for (int i = 0; i < ammoPackPrefabs.Count; i++)
            for (int j = 0; j < amountToPool; j++)
            {
                GameObject newAmmoPack = Instantiate(ammoPackPrefabs[i], transform.position + new Vector3(0 - 100, 0), transform.rotation);
                newAmmoPack.GetComponent<AmmoPack>().weaponPool = this;
                newAmmoPack.SetActive(false);
                allAmmoPacks.Add(newAmmoPack);
                newAmmoPack.transform.parent = gameObject.transform;

                //int spawnTime = newAmmoPack.GetComponent<AmmoPack>().spawnTime;
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

    // Ammo Pack Methods

    public GameObject GetAmmoPackFromList(string ammoType)
    {
        for (int i = 0; i < allAmmoPacks.Count; i++)
        {
            if (allAmmoPacks[i].GetComponent<AmmoPack>().ammoType == ammoType && !allAmmoPacks[i].GetComponent<AmmoPack>().spawnPoint)
                return allAmmoPacks[i];
        }
        return null;
    }
}
