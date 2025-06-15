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
        if (GameTime.instance) GameTime.instance.OnGameTimeElapsedChanged -= OnGameTimeChanged;
    }

    private void Awake()
    {
        if (!GameManager.instance) Destroy(gameObject);


        if (GameManager.instance.gameType == GameManager.GameType.Fiesta ||
            GameManager.instance.gameType == GameManager.GameType.GunGame
            || GameManager.instance.gameType == GameManager.GameType.Rockets
            || GameManager.instance.gameType == GameManager.GameType.Snipers
            || GameManager.instance.gameType == GameManager.GameType.Shotguns)
        {
            gameObject.SetActive(false);
        }
    }


    private void Start()
    {
        _respawnListenerDelay = 1;

        if (GameTime.instance)
        {
            GameTime.instance.OnGameTimeElapsedChanged -= OnGameTimeChanged;
            GameTime.instance.OnGameTimeElapsedChanged += OnGameTimeChanged;
        }
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


        if (CurrentRoomManager.instance) StartCoroutine(SpawnWeaponCoroutine());
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
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop) return;

        try
        {

            if (GameManager.instance.oneObjMode == GameManager.OneObjMode.Off)
            {
                if (weaponSpawned && gameTime.timeElapsed > 0 && (gameTime.timeElapsed % (weaponSpawned.tts - 0) == 0) && gameTime.timeRemaining > 0)
                {
                    //EnableWeapon();

                    if (!CurrentRoomManager.instance.gameOver)
                    {
                        ResetWeaponPositionIfTooFar();
                        StartCoroutine(EnableWeapon_Coroutine());
                    }
                }
            }
            else
            {
                if (weaponSpawned && gameTime.timeElapsed > 0 && (gameTime.timeElapsed % (weaponSpawned.tts - 0) == 0) && gameTime.timeRemaining > 0)
                {
                    //EnableWeapon();

                    if (!CurrentRoomManager.instance.gameOver)
                    {
                        ResetWeaponPositionIfTooFar();
                        StartCoroutine(EnableWeapon_Coroutine());
                    }
                }
            }
        }
        catch (System.Exception e) { Debug.LogWarning(e); }
    }

    public void EnableWeapon()
    {
        if (!CurrentRoomManager.instance.gameOver)
        {
            weaponSpawned.transform.localPosition = Vector3.zero;
            weaponSpawned.transform.localRotation = Quaternion.identity;
            weaponSpawned.localAmmo = weaponSpawned.defaultAmmo;
            weaponSpawned.spareAmmo = weaponSpawned.defaultSpareAmmo;
            weaponSpawned.gameObject.SetActive(true);
            weaponSpawned.ttl = weaponSpawned.defaultTtl;
        }
    }

    void SpawnWeapon()
    {
        try
        {
            //LootableWeapon lw = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", weapon.name), transform.position, transform.rotation).GetComponent<LootableWeapon>();
            //LootableWeapon lw = Instantiate(networkLootableWeaponPrefabs.Where(x => x.name == weapon.name).SingleOrDefault(), transform.position, transform.rotation);

            LootableWeapon lw = WeaponPool.instance.GetLootableWeapon(codeName);

            if (_inGunRack)
            {
                lw.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
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

            CurrentRoomManager.instance.AddSpawnedMappAddOn(transform);
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
        if (GameManager.instance)
        {
            if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
                if (GameManager.instance.gameType == GameManager.GameType.Slayer)
                {
                    if (codeName == "br")
                        codeName = "ar";

                    if (codeName == "pb")
                        codeName = "pr";
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
                else if (GameManager.instance.gameType == GameManager.GameType.PurpleRain)
                {
                    codeName = "cl";
                }
                else if (GameManager.instance.gameType == GameManager.GameType.Swords)
                {
                    codeName = "sword";
                }

            if (GameManager.instance.gameMode == GameManager.GameMode.Versus && GameManager.instance.gameType == GameManager.GameType.Duals)
            {
                if (!codeName.Equals("pp") && !codeName.Equals("rvv") && !codeName.Equals("smg"))
                {
                    if (codeName == "pistol" || codeName == "br")
                        codeName = "rvv";
                    else if (codeName == "sniper" || codeName == "rpg" || codeName == "shotgun" || codeName == "gl" || codeName == "cl")
                        codeName = "pp";
                    else
                        codeName = "smg";
                }
            }





            if (GameManager.instance.gameMode == GameManager.GameMode.Versus && GameManager.instance.gameType == GameManager.GameType.Martian)
            {
                if (!codeName.Equals("pp") && !codeName.Equals("pr") && !codeName.Equals("pb"))
                {
                    if (codeName == "pistol" || codeName == "rvv")
                        codeName = "pb";
                    else if (codeName == "rvv")
                        codeName = "pp";
                    else if (codeName == "sniper" || codeName == "rpg" || codeName == "shotgun" || codeName == "gl" || codeName == "cl")
                        codeName = "cl";
                    else
                        codeName = "pr";
                }
            }





            if ((GameManager.instance.gameType.ToString().Contains("Fiesta")) || GameManager.instance.gameType == GameManager.GameType.GunGame)
            {
                gameObject.SetActive(false);
                //Destroy(gameObject);
            }
        }
    }


    void ResetWeaponPositionIfTooFar()
    {
        if (Vector3.Distance(weaponSpawned.transform.position, transform.position) > 2 || !weaponSpawned.gameObject.activeSelf)
        {
            print("ResetWeaponPosition_Coroutine");
            ReturnWeaponToSpawnPosition();
        }
    }



    public void ReturnWeaponToSpawnPosition()
    {
        weaponSpawned.transform.position = transform.position;
        weaponSpawned.transform.rotation = transform.rotation;
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
