using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class LootableWeapon : MonoBehaviourPun //IPunObservable*/
{
    public delegate void LootableWeaponEvent(LootableWeapon lootableWeapon);
    public LootableWeaponEvent OnLooted;

    public Transform parent { get { return transform.parent; } }
    public string cleanName;
    public string codeName;
    public int spriteId;
    public bool isDw;
    public int networkAmmo
    {
        get { return _ammo; }
        set
        {
            _ammo = value;
            Dictionary<string, string> param = new Dictionary<string, string>();

            param["ammo"] = networkAmmo.ToString();
            try
            {
                NetworkGameManager.instance.UpdateLootableWeaponData(spawnPointPosition, param);
            }
            catch (System.Exception e) { Debug.LogWarning(e); }
        }
    }

    public int localAmmo
    {
        get { return _ammo; }
        set { _ammo = value; }
    }
    public int spareAmmo { get { return _spareAmmo; } set { _spareAmmo = value; } }


    public NetworkWeaponSpawnPoint networkWeaponSpawnPoint
    {
        get { return _networkWeaponSpawnPoint; }
        set { _networkWeaponSpawnPoint = value; }
    }

    public Vector3 spawnPointPosition
    {
        get { return _spawnPointPosition; }
        set
        {
            Vector3 spp = new Vector3((float)System.Math.Round(value.x, 1), (float)System.Math.Round(value.y, 1), (float)System.Math.Round(value.z, 1));
            _spawnPointPosition = spp;
            try
            {
                MultiplayerManager.instance.lootableWeaponsDict.Add(_spawnPointPosition, this);
            }
            catch (System.Exception e) { Debug.LogWarning(e); }
        }
    }

    public Quaternion spawnPointRotation
    {
        get { return _spawnPointRotation; }
        set { _spawnPointRotation = value; }
    }

    public float tts
    {
        get { return _tts; }
        set
        {
            _tts = value;
            Dictionary<string, string> param = new Dictionary<string, string>();

            param["ttl"] = tts.ToString();
            NetworkGameManager.instance.UpdateLootableWeaponData(spawnPointPosition, param);

            //GetComponent<PhotonView>().RPC("UpdateData", RpcTarget.All, param);
        }
    }

    public float ttl { set { _ttl = value; } }
    public float defaultTtl { get { return _defaultTtl; } }
    public int defaultAmmo { get { return _defaultAmmo; } }
    public int defaultSpareAmmo { get { return _defaultSpareAmmo; } }

    [SerializeField] int _ammo;
    [SerializeField] int _spareAmmo;
    [SerializeField] int _defaultAmmo;
    [SerializeField] int _defaultSpareAmmo;
    [SerializeField] float _tts;

    [SerializeField] AudioClip _collisionAudioClip;

    [SerializeField] NetworkWeaponSpawnPoint _networkWeaponSpawnPoint;

    [SerializeField] Vector3 _spawnPointPosition;
    [SerializeField] float _ttl, _defaultTtl;

    Quaternion _spawnPointRotation;

    private void Awake()
    {
        _defaultTtl = _ttl;
        //spawnPointPosition = new Vector3((float)System.Math.Round(transform.position.x, 1), (float)System.Math.Round(transform.position.y, 1), (float)System.Math.Round(transform.position.z, 1));
        spawnPointRotation = transform.rotation;
        GameManager.instance.lootableWeapons.Add(this);
    }
    private void OnEnable()
    {

        try { GetComponent<Rigidbody>().velocity *= 0; } catch { }

        try
        {
            spriteId = -1;
            spriteId = WeaponProperties.spriteIdDic[codeName];

            if (spriteId == -1)
                spriteId = WeaponProperties.spriteIdDic[cleanName];
        }
        catch { }


        if (parent != null && WeaponPool.instance != null && parent != WeaponPool.instance.transform)
        {
            _ammo = defaultAmmo;
            _spareAmmo = defaultSpareAmmo;
        }
    }
    private void Start()
    {
        if (parent != WeaponPool.instance.transform)
            CurrentRoomManager.instance.spawnedMapAddOns++;
    }

    private void Update()
    {
        if (!networkWeaponSpawnPoint)
        {
            if (_ttl > 0 && _ttl < 999)
            {

                _ttl -= Time.deltaTime;

                if (_ttl <= 0)
                {
                    transform.position = new Vector3(0, -100, 0);
                    gameObject.SetActive(false);
                    //Destroy(gameObject);
                }
            }
        }
    }

    public void ResetAmmo()
    {
        networkAmmo = _defaultAmmo;
        spareAmmo = _defaultSpareAmmo;
    }

    public void RandomAmmo()
    {
        networkAmmo = (int)Mathf.Ceil(Random.Range(0, networkAmmo));
        spareAmmo = (int)Mathf.Ceil(Random.Range(0, spareAmmo));

        _defaultAmmo = (int)Mathf.Ceil(Random.Range(0, _defaultAmmo)); ;
        spareAmmo = (int)Mathf.Ceil(Random.Range(0, spareAmmo));
    }

    public void LootWeapon(Player player)
    {
        PlayerInventory playerInventory = player.playerInventory;
        WeaponProperties w = playerInventory.activeWeapon;
        if (playerInventory.holsteredWeapon.codeName == codeName)
            w = playerInventory.holsteredWeapon;

        int ammoNeeded = w.maxSpareAmmo - w.spareAmmo;
        int ammoAvailable = (localAmmo + spareAmmo);
        int totalAmmoAvailable = _ammo + _spareAmmo;

        if (w.injectLootedAmmo)
            ammoNeeded = w.ammoCapacity - w.loadedAmmo;

        if (ammoNeeded <= 0)
            return;


        int ammoToLoot = ammoNeeded;
        if (ammoNeeded >= ammoAvailable)
            ammoToLoot = ammoAvailable;

        if (w.injectLootedAmmo)
        {
            w.loadedAmmo += ammoAvailable;
            HideWeapon();
            NetworkGameManager.instance.DisableLootableWeapon(spawnPointPosition);
            playerInventory.player.allPlayerScripts.weaponPickUp.ammoPickupAudioSource.Play();
            playerInventory.player.GetComponent<KillFeedManager>().EnterNewFeed($"Picked up {cleanName} ammo ({ammoNeeded})");
            return;
        }
        else
        {
            w.spareAmmo += ammoToLoot;
        }
        playerInventory.player.GetComponent<KillFeedManager>().EnterNewFeed($"Picked up {cleanName} ammo ({ammoToLoot})");

        if (ammoNeeded >= ammoAvailable)
            HideWeapon();
        else
        {
            Dictionary<string, string> param = new Dictionary<string, string>();

            int newAmmo = localAmmo;
            int newSpareAmmo = spareAmmo;

            newSpareAmmo -= ammoNeeded;
            if (newSpareAmmo < 0)
            {
                newAmmo -= Mathf.Abs(newSpareAmmo);
                newSpareAmmo = 0;
            }

            param["ammo"] = newAmmo.ToString();
            param["spareAmmo"] = newSpareAmmo.ToString();

            NetworkGameManager.instance.UpdateLootableWeaponData(spawnPointPosition, param);
        }
        playerInventory.player.allPlayerScripts.weaponPickUp.ammoPickupAudioSource.Play();
    }

    public void HideWeapon()
    {
        Debug.Log("DisableWeapon");
        OnLooted?.Invoke(this);
        gameObject.SetActive(false);

        return;

        if (networkWeaponSpawnPoint)
        {
            Debug.Log("DisableWeapon 1");
            //GameManager.SetLayerRecursively(gameObject, 3);
            //gameObject.layer = 3;
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("DisableWeapon 2");
            Destroy(gameObject);
        }
    }

    public void ShowWeapon()
    {
        //GameManager.SetLayerRecursively(gameObject, 10);
        //gameObject.layer = 10;
        gameObject.SetActive(true);
    }

    public void EnableWeapon()
    {
        gameObject.SetActive(true);
        ResetAmmo();
    }

    public void AddForce(Vector3 forwardDir)
    {
        //NetworkGameManager.instance.AddForceLootableWeapon(spawnPointPosition, forwardDir);
        //GetComponent<Rigidbody>().AddForce(forwardDir * 200);
    }

    public void UpdateData(Dictionary<string, string> param)
    {
        if (param.ContainsKey("ammo"))
            _ammo = int.Parse(param["ammo"]);

        if (param.ContainsKey("spareAmmo"))
            _spareAmmo = int.Parse(param["spareAmmo"]);

        if (param.ContainsKey("ttl"))
            _tts = int.Parse(param["ttl"]);
    }

    public void UpdateSpawnPointPosition(Vector3 spp)
    {
        GetComponent<PhotonView>().RPC("UpdateSpawnPointPosition_RPC", RpcTarget.All, spp);
    }

    [PunRPC]
    public void UpdateSpawnPointPosition_RPC(Vector3 spp)
    {
        Debug.Log("UpdateSpawnPointPosition_RPC");
        _spawnPointPosition = spp;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<Rigidbody>().velocity /= 2;
        try
        {
            GetComponent<AudioSource>().clip = _collisionAudioClip;
            GetComponent<AudioSource>().Play();
            GameObjectPool.instance.SpawnWeaponSmokeCollisionObject(transform.position);
        }
        catch (System.Exception e) { Debug.LogError(e); }
    }

    private void OnDisable()
    {
        if (networkWeaponSpawnPoint)
        {
            NetworkGameManager.instance.DisableLootableWeapon(spawnPointPosition);
        }
    }
}
