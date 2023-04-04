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

    private void OnDestroy()
    {
        GameTime.instance.OnGameTimeChanged -= OnGameTimeChanged;
    }
    private void Start()
    {
        _respawnListenerDelay = 1;

        FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom -= OnAllPlayersJoinedRoom_Delegate;
        FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom += OnAllPlayersJoinedRoom_Delegate;


        GameTime.instance.OnGameTimeChanged -= OnGameTimeChanged;
        GameTime.instance.OnGameTimeChanged += OnGameTimeChanged;
        ReplaceWeaponsByGametype();

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
                        //LootableWeapon lw = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", weapon.name), transform.position, transform.rotation).GetComponent<LootableWeapon>();
                        LootableWeapon lw = Instantiate(networkLootableWeaponPrefabs.Where(x => x.name == weapon.name).SingleOrDefault(), transform.position, transform.rotation);
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

    void OnWaveStart(SwarmManager swarmManager)
    {
        Debug.Log($"NetworkWeaponSpawnPoint OnWaveStart: {swarmManager.currentWave}");
    }

    // Methods
    #region
    void ReplaceWeaponsByGametype()
    {
        string[] powerWeaponCodeNames = { "r700", "m1100", "rpg", "barett50cal", "m32" };
        string[] heavyWeaponCodeNames = { "m16", "m4", "ak47", "scar", "patriot", "mk14", "m249c" };
        string[] lightWeaponCodeNames = { "m1911", "colt", "mp5", "p90", "desert_eagle" };

        if (codeName == "mk14")
            codeName = "c7";

        if (codeName == "colt")
            codeName = "deagle";

        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            if ((GameManager.instance.gameType.ToString().Contains("Slayer")))
            {
                if (codeName == "scar")
                    codeName = "m4";

                if (codeName == "m16")
                    codeName = "m1911";

                if (codeName == "c7" || codeName == "mk14")
                    //codeName = "mp5";
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
