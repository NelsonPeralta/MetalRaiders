using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class NetworkWeaponSpawnPoint : MonoBehaviour
{
    public bool auth { get { return _auth; } set { _auth = value; } }
    public string codeName;
    public GameObject placeHolder;
    public LootableWeapon weaponSpawned;
    public List<LootableWeapon> networkLootableWeaponPrefabs = new List<LootableWeapon>();

    [SerializeField] float _tts;
    bool _auth;

    private void OnDestroy()
    {
        GameTime.instance.OnGameTimeChanged -= OnGameTimeChanged;
    }
    private void Start()
    {
        FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom -= OnAllPlayersJoinedRoom_Delegate;
        FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom += OnAllPlayersJoinedRoom_Delegate;


        GameTime.instance.OnGameTimeChanged -= OnGameTimeChanged;
        GameTime.instance.OnGameTimeChanged += OnGameTimeChanged;
        ReplaceWeaponsByGametype();

        if (placeHolder)
            placeHolder.gameObject.SetActive(false);

    }

    private void Update()
    {

        return;

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

    void OnGameTimeChanged(GameTime gameTime)
    {
        try
        {

            if (gameTime.totalTime % weaponSpawned.tts == 0)
            {
                weaponSpawned.gameObject.SetActive(true);

                weaponSpawned.ammo = weaponSpawned.defaultAmmo;
                weaponSpawned.spareAmmo = weaponSpawned.defaultSpareAmmo;

                weaponSpawned.transform.position = weaponSpawned.spawnPointPosition;
                weaponSpawned.transform.rotation = weaponSpawned.spawnPointRotation;
            }
        }
        catch (System.Exception e) { Debug.LogWarning(e); }
    }

    void OnAllPlayersJoinedRoom_Delegate(GameManagerEvents gme)
    {
        Debug.Log("OnAllPlayersJoinedRoom_Delegate");

        try
        {
            foreach (LootableWeapon weapon in networkLootableWeaponPrefabs)
            {
                if (weapon.codeName == codeName)
                {
                    try
                    {
                        LootableWeapon lw = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", weapon.name), transform.position, transform.rotation).GetComponent<LootableWeapon>();

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

    // Methods
    #region
    void ReplaceWeaponsByGametype()
    {
        string[] powerWeaponCodeNames = { "r700", "m1100", "rpg", "barett50cal", "m32" };
        string[] heavyWeaponCodeNames = { "m16", "m4", "ak47", "scar", "patriot", "mk14", "m249c" };
        string[] lightWeaponCodeNames = { "m1911", "colt", "mp5", "p90", "desert_eagle" };

        if ((GameManager.instance.gameType.ToString().Contains("Slayer")))
        {
            if (codeName == "scar")
                codeName = "m249c";

            if (codeName == "m16")
                codeName = "deagle";

            if (codeName == "mk14")
                codeName = "p90";
        }
        else if ((GameManager.instance.gameType.ToString().Contains("Snipers")))
        {
            foreach (string weaponCode in powerWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "barrett50cal";

            foreach (string weaponCode in lightWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "r700";

            foreach (string weaponCode in heavyWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "r700";
        }
        else if ((GameManager.instance.gameType.ToString().Contains("Rockets")))
        {
            foreach (string weaponCode in powerWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "rpg";

            foreach (string weaponCode in lightWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "rpg";

            foreach (string weaponCode in heavyWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "m32";
        }
        else if ((GameManager.instance.gameType.ToString().Contains("Shotguns")))
        {
            foreach (string weaponCode in lightWeaponCodeNames)
                if (weaponCode == codeName)
                    codeName = "nailgun";

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
