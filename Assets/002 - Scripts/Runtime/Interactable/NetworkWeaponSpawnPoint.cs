using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Linq;

public class NetworkWeaponSpawnPoint : MonoBehaviour
{
    public bool auth { get { return _auth; } set { _auth = value; } }
    public string codeName;
    public GameObject placeHolder;
    public LootableWeapon weaponSpawned { get { return _weaponSpawned; } set { _weaponSpawned = value; _tts = _weaponSpawned.tts; } }
    public List<LootableWeapon> networkLootableWeaponPrefabs = new List<LootableWeapon>();

    [SerializeField] float _tts;
    [SerializeField] LootableWeapon _weaponSpawned;
    bool _auth;

    float _respawnListenerDelay = 1;

    private void OnDisable()
    {
        GameTime.instance.OnGameTimeChanged -= OnGameTimeChanged;
    }
    private void OnDestroy()
    {
        GameTime.instance.OnGameTimeChanged -= OnGameTimeChanged;
    }
    private void Start()
    {
        _respawnListenerDelay = 1;

        GameTime.instance.OnGameTimeChanged -= OnGameTimeChanged;
        GameTime.instance.OnGameTimeChanged += OnGameTimeChanged;
        ReplaceWeaponsByGametype();
        SpawnWeapon();

        if (placeHolder)
            placeHolder.gameObject.SetActive(false);


        try
        {
            FindObjectOfType<SwarmManager>().OnWaveStart -= OnWaveStart;
            FindObjectOfType<SwarmManager>().OnWaveStart += OnWaveStart;
        }
        catch { }

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
                        weaponSpawned.networkAmmo = weaponSpawned.defaultAmmo;
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
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            //EnableWeapon();
            return;
        }

        try
        {
            if (weaponSpawned && (gameTime.totalTime % (weaponSpawned.tts - 5) == 0) && gameTime.totalTime > 0)
            {
                //EnableWeapon();
                StartCoroutine(ResetWeaponPosition_Coroutine());
                StartCoroutine(EnableWeapon_Coroutine());
            }
        }
        catch (System.Exception e) { Debug.LogWarning(e); }
    }

    void EnableWeapon()
    {

        weaponSpawned.localAmmo = weaponSpawned.defaultAmmo;
        weaponSpawned.spareAmmo = weaponSpawned.defaultSpareAmmo;

        //Vector3 wpp = weaponSpawned.transform.position;
        //float d = Vector3.Distance(wpp, weaponSpawned.spawnPointPosition);

        //if (d > 2 || !weaponSpawned.gameObject.activeSelf)
        //{
        //    weaponSpawned.transform.position = weaponSpawned.spawnPointPosition;
        //    weaponSpawned.transform.rotation = weaponSpawned.spawnPointRotation;
        //}

        weaponSpawned.gameObject.SetActive(true);
        //if (weaponSpawned.gameObject.layer != 10)
        //    weaponSpawned.ShowWeapon();
    }

    void OnAllPlayersJoinedRoom_Delegate(CurrentRoomManager gme)
    {
        SpawnWeapon();

    }

    void SpawnWeapon()
    {
        try
        {
            foreach (LootableWeapon weapon in networkLootableWeaponPrefabs)
            {
                if (weapon.codeName == codeName)
                {
                    try
                    {
                        //LootableWeapon lw = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", weapon.name), transform.position, transform.rotation).GetComponent<LootableWeapon>();
                        LootableWeapon lw = Instantiate(networkLootableWeaponPrefabs.Where(x => x.name == weapon.name).SingleOrDefault(), transform.position, transform.rotation);
                        lw.transform.parent = transform;
                        lw.spawnPointPosition = this.transform.position;
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

    void OnWaveStart(SwarmManager swarmManager)
    {
        Debug.Log($"NetworkWeaponSpawnPoint OnWaveStart: {swarmManager.currentWave}");
    }

    // Methods
    #region
    void ReplaceWeaponsByGametype()
    {
        string[] powerWeaponCodeNames = { "r700", "m1100", "rpg", "barett50cal", "m32", "sniper", "shotgun" };
        string[] heavyWeaponCodeNames = { "ar", "br", "m16", "c7", "m4", "ak47", "scar", "patriot", "mk14", "m249c" };
        string[] lightWeaponCodeNames = { "pistol", "smg", "m1911", "colt", "mp5", "p90", "deagle" };

        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            if ((GameManager.instance.gameType.ToString().Contains("Slayer"))
                || GameManager.instance.gameType == GameManager.GameType.Retro)
            {
                if (codeName == "br")
                    codeName = "pistol";
            }
            else if ((GameManager.instance.gameType.ToString().Contains("Snipers")))
            {
                codeName = "sniper";
            }
            else if ((GameManager.instance.gameType.ToString().Contains("Rockets")))
            {
                codeName = "rpg";
            }
            else if ((GameManager.instance.gameType.ToString().Contains("Shotguns")))
            {
                codeName = "shotgun";
            }

        if ((GameManager.instance.gameType.ToString().Contains("Fiesta")) || GameManager.instance.gameType == GameManager.GameType.GunGame)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    IEnumerator EnableWeapon_Coroutine()
    {
        yield return new WaitForSeconds(5);

        EnableWeapon();
    }

    IEnumerator ResetWeaponPosition_Coroutine()
    {
        yield return new WaitForSeconds(2);

        Vector3 wpp = weaponSpawned.transform.position;
        float d = Vector3.Distance(wpp, weaponSpawned.spawnPointPosition);

        if (d > 2 || !weaponSpawned.gameObject.activeSelf)
        {
            weaponSpawned.transform.position = weaponSpawned.spawnPointPosition;
            weaponSpawned.transform.rotation = weaponSpawned.spawnPointRotation;
        }
    }
}
