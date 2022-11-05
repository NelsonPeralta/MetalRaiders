using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class NetworkWeaponSpawnPoint : MonoBehaviour
{
    public string codeName;
    public int timeToSpawn;
    public GameObject placeHolder;
    public LootableWeapon weaponSpawned;
    public List<LootableWeapon> networkLootableWeaponPrefabs = new List<LootableWeapon>();

    private void Start()
    {
        //ReplaceWeaponsByGametype();
        if (placeHolder)
            placeHolder.gameObject.SetActive(false);

        try
        {
            foreach (LootableWeapon weapon in networkLootableWeaponPrefabs)
            {
                if (weapon.codeName == codeName)
                {
                    try
                    {

                    LootableWeapon lw = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", weapon.cleanName), Vector3.zero, Quaternion.identity).GetComponent<LootableWeapon>();

                    lw.transform.position = transform.position;
                    lw.transform.rotation = transform.rotation;
                    lw.gameObject.SetActive(true);
                    lw.networkWeaponSpawnPoint = this;
                    weaponSpawned = lw;
                    }
                    catch(System.Exception e) { Debug.LogWarning(e); }
                }
            }
        }
        catch { }
    }
}
