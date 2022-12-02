using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class NetworkWeaponSpawnPoint : MonoBehaviour
{
    public string codeName;
    public GameObject placeHolder;
    public LootableWeapon weaponSpawned;
    public List<LootableWeapon> networkLootableWeaponPrefabs = new List<LootableWeapon>();

    float _tts;

    private void Start()
    {
        ReplaceWeaponsByGametype();

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
                        LootableWeapon lw = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", weapon.name), Vector3.zero, Quaternion.identity).GetComponent<LootableWeapon>();

                        lw.transform.position = transform.position;
                        lw.transform.rotation = transform.rotation;
                        lw.transform.parent = transform;
                        lw.gameObject.SetActive(true);
                        lw.networkWeaponSpawnPoint = this;
                        weaponSpawned = lw;
                        _tts = lw.tts;
                    }
                    catch (System.Exception e) { Debug.LogWarning(e); }
                }
            }
        }
        catch { }
    }

    private void Update()
    {
        if (weaponSpawned)
        {
            _tts -= Time.deltaTime;

            if (_tts < 0)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (!weaponSpawned.gameObject.activeSelf)
                    {
                        NetworkGameManager.instance.EnableLootableWeapon(weaponSpawned.spawnPointPosition);
                        weaponSpawned.ammo = weaponSpawned.defaultAmmo;
                        weaponSpawned.spareAmmo = weaponSpawned.defaultSpareAmmo;
                    }
                    else
                    {
                        NetworkGameManager.instance.RelocateLootableWeapon(weaponSpawned.spawnPointPosition, weaponSpawned.spawnPointRotation);
                    }
                }
                _tts = weaponSpawned.tts;
            }
        }
    }

    // Methods
    #region
    void ReplaceWeaponsByGametype()
    {
        string[] powerWeaponCodeNames = { "r700", "m1100", "rpg", "barett50cal" };
        string[] heavyWeaponCodeNames = { "m16", "m4", "ak47", "scar", "patriot", "mk14"};
        string[] lightWeaponCodeNames = { "m1911", "colt", "mp5" };

        if ((GameManager.instance.gameType.ToString().Contains("Pro")))
        {
            foreach (string weaponCode in lightWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "mk14";

            foreach (string weaponCode in heavyWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "m16";

            //foreach (string weaponCode in powerWeaponCodeNames)
            //    if (weaponCode == codeName)
            //        codeName = "mk14";
        }
        else if ((GameManager.instance.gameType.ToString().Contains("Snipers")))
        {
            foreach (string weaponCode in lightWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "barrett50cal";
            
            foreach (string weaponCode in heavyWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "r700";

            foreach (string weaponCode in powerWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "barrett50cal";
        }
        else if ((GameManager.instance.gameType.ToString().Contains("Rockets")))
        {
            foreach (string weaponCode in lightWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "rpg";

            foreach (string weaponCode in heavyWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "m32";

            foreach (string weaponCode in powerWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "rpg";


        }
        else if ((GameManager.instance.gameType.ToString().Contains("Shotguns")))
        {
            foreach (string weaponCode in lightWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "m1100";

            foreach (string weaponCode in powerWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "m1100";

            foreach (string weaponCode in heavyWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "m1100";
        }

        if ((GameManager.instance.gameType.ToString().Contains("Fiesta")))
        {
            Destroy(gameObject);
        }
    }
    #endregion
}
