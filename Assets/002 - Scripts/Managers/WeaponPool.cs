using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Collections;

public class WeaponPool : MonoBehaviourPun
{
    public PhotonView PV;
    public static WeaponPool weaponPoolInstance;
    public static OnlineGameTime onlineGameTimeInstance;
    public int amountOfWeaponsToPool;
    public int amountOfWeaponPacksToPool;

    [Header("Timer")]
    public GameObject timerPrefab;
    public GameObject timer;

    [Header("Weapons")]
    public List<LootableWeapon> allWeapons = new List<LootableWeapon>();
    public List<GameObject> weaponPrefabs = new List<GameObject>();

    [Header("Ammo")]
    public List<OnlineAmmoPackSpawnPoint> allAmmoPackSpawnPoints = new List<OnlineAmmoPackSpawnPoint>();
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
            for (int j = 0; j < amountOfWeaponsToPool; j++)
            {
                // TO DO: Spawn them using normal instantiate instead and fix LootableWeapon script according to it (Start method)
                GameObject newWeap = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", weaponPrefabs[i].name), Vector3.zero, Quaternion.identity);
                //GameObject newWeap = Instantiate(weaponPrefabs[i], transform.position + new Vector3(0 - 100, 0), transform.rotation);
                newWeap.name = newWeap.name.Replace("(Clone)", "");
                newWeap.SetActive(false);
                allWeapons.Add(newWeap.GetComponent<LootableWeapon>());
                newWeap.transform.parent = gameObject.transform;
            }

        for (int i = 0; i < ammoPackPrefabs.Count; i++)
            for (int j = 0; j < amountOfWeaponPacksToPool; j++)
            {
                //GameObject newAmmoPack = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/AmmoPacks", ammoPackPrefabs[i].name), new Vector3(0 - 100, 0), Quaternion.identity);
                GameObject newAmmoPack = Instantiate(ammoPackPrefabs[i], transform.position + new Vector3(0 - 100, 0), transform.rotation);
                newAmmoPack.GetComponent<AmmoPack>().weaponPool = this;
                newAmmoPack.SetActive(false);
                allAmmoPacks.Add(newAmmoPack);
                newAmmoPack.transform.parent = gameObject.transform;

                //int spawnTime = newAmmoPack.GetComponent<AmmoPack>().spawnTime;
            }

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("ammo_pack_spawn_point"))
            allAmmoPackSpawnPoints.Add(go.GetComponent<OnlineAmmoPackSpawnPoint>());

        //StartCoroutine(GiveAmmoPackSpawnPointAnAmmoPack());
    }

    public GameObject GetWeaponFromList(string weaponName)
    {
        for (int i = 0; i < allWeapons.Count; i++)
        {
            if (allWeapons[i].name == weaponName && !allWeapons[i].gameObject.activeSelf)
                return allWeapons[i].gameObject;
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
        allWeapons[index].gameObject.SetActive(false);
    }

    public void DisablePooledWeapon(Vector3 position)
    {
        for (int i = 0; i < allWeapons.Count; i++)
        {
            if (allWeapons[i].GetSpawnPointPosition() == position)
                allWeapons[i].DisableWeapon();
        }
    }

    // Ammo Pack Methods

    public GameObject GetAmmoPackFromList(string ammoType)
    {
        for (int i = 0; i < allAmmoPacks.Count; i++)
        {
            if (allAmmoPacks[i].GetComponent<AmmoPack>().ammoType == ammoType && !allAmmoPacks[i].GetComponent<AmmoPack>().onlineAmmoPackSpawnPoint)
                return allAmmoPacks[i];
        }
        return null;
    }

    public GameObject GetAmmoPackWithPhotonId(int photonId)
    {
        for (int i = 0; i < allAmmoPacks.Count; i++)
        {
            if (allAmmoPacks[i].GetComponent<PhotonView>().ViewID == photonId)
                return allAmmoPacks[i];
        }
        return null;
    }

    private void OnDestroy()
    {
        weaponPoolInstance = null;
    }

    IEnumerator GiveAmmoPackSpawnPointAnAmmoPack()
    {
        yield return new WaitForSeconds(3);
        for (int i = 0; i < allAmmoPackSpawnPoints.Count; i++)
        {
            string ammoType = allAmmoPackSpawnPoints[i].ammoType;
            int ammoPackPhotonId = 0;
            int ammoPackIndex = 99;

            for (int j = 0; j < allAmmoPacks.Count; j++)
                if (allAmmoPacks[j].GetComponent<AmmoPack>().ammoType == ammoType && !allAmmoPacks[j].GetComponent<AmmoPack>().onlineAmmoPackSpawnPoint && !allAmmoPackSpawnPoints[i].ammoPack)
                {
                    allAmmoPacks[j].GetComponent<AmmoPack>().onlineAmmoPackSpawnPoint = allAmmoPackSpawnPoints[i];
                    ammoPackPhotonId = allAmmoPacks[j].GetComponent<PhotonView>().ViewID;
                    ammoPackIndex = j;
                }

            if (PhotonNetwork.IsMasterClient)
                PV.RPC("GiveAmmoPackSpawnPointAnAmmoPack_RPC", RpcTarget.All, allAmmoPackSpawnPoints[i].transform.position, ammoPackIndex);
        }
    }

    [PunRPC]
    void GiveAmmoPackSpawnPointAnAmmoPack_RPC(Vector3 spawnPointPosition, int ammoPackIndex)
    {
        AmmoPack ap = allAmmoPacks[ammoPackIndex].GetComponent<AmmoPack>();
        for (int i = 0; i < allAmmoPackSpawnPoints.Count; i++)
        {
            if (allAmmoPackSpawnPoints[i].transform.position == spawnPointPosition)
                allAmmoPackSpawnPoints[i].ammoPack = ap;

            ap.gameObject.SetActive(true);
            ap.transform.position = spawnPointPosition;
        }
    }

    public LootableWeapon GetWeaponWithSpawnPoint(Vector3 position)
    {
        for (int i = 0; i < allWeapons.Count; i++)
        {
            if (allWeapons[i].GetSpawnPointPosition() == position)
                return allWeapons[i];
        }
        return null;
    }
}
