using System.Collections;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class OnlineWeaponSpawnPoint : MonoBehaviour
{
    public string codeName;
    public GameObject weaponPlaceHolder;
    public LootableWeapon weaponSpawned;
    public int timeToSpawn;

    private void Start()
    {
        ReplaceWeaponsByGametype();
        if (weaponPlaceHolder)
            weaponPlaceHolder.gameObject.SetActive(false);

        StartCoroutine(SpawnNewWeaponFromWeaponPool(0.1f));
    }

    IEnumerator SpawnNewWeaponFromWeaponPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (FindObjectOfType<WeaponPool>().allWeapons.Count <= 0)
            StartCoroutine(SpawnNewWeaponFromWeaponPool(0.1f));
        if (!weaponSpawned)
        {
            //Debug.Log("Spawning New Weapon");
            var newWeap = FindObjectOfType<WeaponPool>().GetWeaponFromList(codeName).GetComponent<LootableWeapon>();
            newWeap.transform.position = transform.position;
            newWeap.transform.rotation = transform.rotation;
            newWeap.gameObject.SetActive(true);
            weaponSpawned = newWeap;
        }
        else
            weaponSpawned.EnableWeapon();
    }

    public void StartRespawn()
    {
        GameTime ogt = FindObjectOfType<GameTime>();
        int timeWeaponWasGrabbed = ogt.totalTime;
        int newSpawnTime = timeToSpawn - (timeWeaponWasGrabbed % timeToSpawn);
        //Debug.Log($"Time weapon grabbed: {ogt.totalTime}. New Spawn Time: {newSpawnTime}");
        StartCoroutine(SpawnNewWeaponFromWeaponPool(newSpawnTime));
    }

    void ReplaceWeaponsByGametype()
    {
        string[] powerWeaponCodeNames = { "r700", "m1100", "rpg", "barett50cal" };
        string[] heavyWeaponCodeNames = { "m16", "m4", "ak47", "scar", "patriot", "mk14" };
        string[] lightWeaponCodeNames = { "m1911", "colt", "mp5" };

        if ((GameManager.instance.gameType == GameManager.GameType.Pro))
        {

            foreach (string weaponCode in powerWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "mk14";

            foreach (string weaponCode in heavyWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "m16";

            foreach (string weaponCode in lightWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "mk14";
        }

        if ((GameManager.instance.gameType == GameManager.GameType.Snipers))
        {
            foreach (string weaponCode in lightWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "barrett50cal";

            foreach (string weaponCode in powerWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "r700";

            foreach (string weaponCode in heavyWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "barrett50cal";

        }else if ((GameManager.instance.gameType == GameManager.GameType.Rockets))
        {
            foreach (string weaponCode in lightWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "rpg";

            foreach (string weaponCode in powerWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "rpg";

            foreach (string weaponCode in heavyWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "m32";

        }

        if ((GameManager.instance.gameType == GameManager.GameType.Fiesta))
        {
            Destroy(gameObject);
        }
    }
}
