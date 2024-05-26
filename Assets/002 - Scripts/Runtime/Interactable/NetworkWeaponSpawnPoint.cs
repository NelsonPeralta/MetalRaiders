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
    [SerializeField] bool _inGunRack;
    bool _auth;

    float _respawnListenerDelay = 1;

    private void OnDisable()
    {
        GameTime.instance.OnGameTimeElapsedChanged -= OnGameTimeChanged;
    }
    private void OnDestroy()
    {
        GameTime.instance.OnGameTimeElapsedChanged -= OnGameTimeChanged;
    }

    private void Awake()
    {
        if (GameManager.instance.gameType == GameManager.GameType.Fiesta || GameManager.instance.gameType == GameManager.GameType.GunGame)
        {
            gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        _respawnListenerDelay = 1;

        GameTime.instance.OnGameTimeElapsedChanged -= OnGameTimeChanged;
        GameTime.instance.OnGameTimeElapsedChanged += OnGameTimeChanged;
        ReplaceWeaponsByGametype();
        //SpawnWeapon();

        if (placeHolder)
            placeHolder.gameObject.SetActive(false);


        try
        {
            FindObjectOfType<SwarmManager>().OnWaveStart -= OnWaveStart;
            FindObjectOfType<SwarmManager>().OnWaveStart += OnWaveStart;
        }
        catch { }
        StartCoroutine(SpawnWeaponCoroutine());
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
            if (weaponSpawned && (gameTime.timeElapsed % (weaponSpawned.tts - 0) == 0) && gameTime.timeRemaining > 0)
            {
                //EnableWeapon();

                ResetWeaponPositionIfTooFar();
                StartCoroutine(EnableWeapon_Coroutine());
            }
        }
        catch (System.Exception e) { Debug.LogWarning(e); }
    }

    void EnableWeapon()
    {
        print("EnableWeapon");


        weaponSpawned.transform.localPosition = Vector3.zero;
        weaponSpawned.transform.localRotation = Quaternion.identity;
        weaponSpawned.localAmmo = weaponSpawned.defaultAmmo;
        weaponSpawned.spareAmmo = weaponSpawned.defaultSpareAmmo;
        weaponSpawned.gameObject.SetActive(true);
    }

    void SpawnWeapon()
    {
        try
        {
            //LootableWeapon lw = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", weapon.name), transform.position, transform.rotation).GetComponent<LootableWeapon>();
            //LootableWeapon lw = Instantiate(networkLootableWeaponPrefabs.Where(x => x.name == weapon.name).SingleOrDefault(), transform.position, transform.rotation);

            print($"SpawnWeapon {codeName} {WeaponPool.instance.weaponPool.Count}");

            LootableWeapon lw = WeaponPool.instance.GetLootableWeapon(codeName);

            if (_inGunRack)
            {
                lw.GetComponent<Rigidbody>().velocity = Vector3.zero;
                lw.GetComponent<Rigidbody>().isKinematic = true;
                lw.GetComponent<Rigidbody>().useGravity = false;
                //Destroy(lw.GetComponent<Rigidbody>());
            }

            lw.transform.parent = transform;
            lw.transform.localPosition = Vector3.zero;
            lw.transform.localRotation = Quaternion.identity;
            //lw.spawnPointPosition = this.transform.position;
            lw.gameObject.SetActive(true);
            lw.networkWeaponSpawnPoint = this;
            weaponSpawned = lw;
            _tts = lw.tts;
        }
        catch (System.Exception e) { Debug.LogWarning(e); }
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
            if (GameManager.instance.gameType == GameManager.GameType.Slayer)
            {
                if (codeName == "br")
                    codeName = "ar";

                if (codeName == "pb")
                    codeName = "pr";
            }
            else if (GameManager.instance.gameType == GameManager.GameType.Pro)
            {
                //if (codeName == "cl")
                //    codeName = "ar";

                //if (codeName == "rpg")
                //    codeName = "ar";
            }
            else if (GameManager.instance.gameType == GameManager.GameType.Swat)
            {
                codeName = "br";
            }
            else if (GameManager.instance.gameType == GameManager.GameType.Snipers)
            {
                codeName = "sniper";
            }
            else if (GameManager.instance.gameType == GameManager.GameType.Rockets)
            {
                codeName = "rpg";
            }
            else if (GameManager.instance.gameType == GameManager.GameType.Shotguns)
            {
                codeName = "shotgun";
            }

        if ((GameManager.instance.gameType.ToString().Contains("Fiesta")) || GameManager.instance.gameType == GameManager.GameType.GunGame)
        {
            gameObject.SetActive(false);
            //Destroy(gameObject);
        }
    }


    void ResetWeaponPositionIfTooFar()
    {
        if (Vector3.Distance(weaponSpawned.transform.position, transform.position) > 2 || !weaponSpawned.gameObject.activeSelf)
        {
            print("ResetWeaponPosition_Coroutine");
            weaponSpawned.transform.position = transform.position;
            weaponSpawned.transform.rotation = transform.rotation;
        }
    }
    #endregion

    IEnumerator EnableWeapon_Coroutine()
    {
        yield return new WaitForSeconds(0.5f);

        EnableWeapon();
    }

    IEnumerator SpawnWeaponCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnWeapon();
    }
}
