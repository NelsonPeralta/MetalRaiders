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
            if (allWeapons[i].name == weaponName)
                return allWeapons[i];
        }
        return null;
    }
}
