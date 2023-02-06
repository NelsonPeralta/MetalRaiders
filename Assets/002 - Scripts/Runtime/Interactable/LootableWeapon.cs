using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class LootableWeapon : MonoBehaviourPun //IPunObservable*/
{
    public delegate void LootableWeaponEvent(LootableWeapon lootableWeapon);
    public LootableWeaponEvent OnLooted;

    public string cleanName;
    public string codeName;
    public int spriteId;
    public int ammo
    {
        get { return _ammo; }
        set
        {
            _ammo = value;
            Dictionary<string, string> param = new Dictionary<string, string>();

            param["ammo"] = ammo.ToString();
            try
            {
                NetworkGameManager.instance.UpdateLootableWeaponData(spawnPointPosition, param);
            }
            catch (System.Exception e) { Debug.LogWarning(e); }
        }
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
        set { _spawnPointPosition = value; }
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
    [SerializeField] float _ttl;

    Quaternion _spawnPointRotation;
    private void OnEnable()
    {
        GetComponent<Rigidbody>().velocity *= 0;

        try
        {
            spriteId = -1;
            spriteId = WeaponProperties.spriteIdDic[codeName];

            if (spriteId == -1)
                spriteId = WeaponProperties.spriteIdDic[cleanName];
        }
        catch { }

        _ammo = defaultAmmo;
        _spareAmmo = defaultSpareAmmo;
    }
    private void Start()
    {
        spawnPointPosition = new Vector3((float)System.Math.Round(transform.position.x, 2), (float)System.Math.Round(transform.position.y, 2), (float)System.Math.Round(transform.position.z, 2));
        spawnPointRotation = transform.rotation;

        try
        {
            MultiplayerManager.instance.lootableWeaponsDict.Add(spawnPointPosition, this);
        }
        catch (System.Exception e) { Debug.LogWarning(e); }
    }

    private void Update()
    {
        if (!networkWeaponSpawnPoint)
        {
            _ttl -= Time.deltaTime;

            if (_ttl <= 0)
                Destroy(gameObject);
        }
    }

    public void ResetAmmo()
    {
        ammo = _defaultAmmo;
        spareAmmo = _defaultSpareAmmo;
    }

    public void RandomAmmo()
    {
        ammo = (int)Mathf.Ceil(Random.Range(0, ammo));
        spareAmmo = (int)Mathf.Ceil(Random.Range(0, spareAmmo));

        _defaultAmmo = (int)Mathf.Ceil(Random.Range(0, _defaultAmmo)); ;
        spareAmmo = (int)Mathf.Ceil(Random.Range(0, spareAmmo));
    }

    public void LootWeapon(int controllerId = 0, bool onlyExtraAmmo = false)
    {
        Debug.Log("OnLooted");
        int ammoToLoot = spareAmmo;
        PlayerInventory playerInventory = GameManager.GetMyPlayer(controllerId).playerInventory;
        if (!onlyExtraAmmo)
            ammoToLoot += ammo;

        foreach (GameObject wp in playerInventory.allWeaponsInInventory)
            if (wp.GetComponent<WeaponProperties>().codeName == codeName)
                wp.GetComponent<WeaponProperties>().spareAmmo += ammoToLoot;

        DisableWeapon();
    }

    public void DisableWeapon()
    {
        Debug.Log("DisableWeapon");
        OnLooted?.Invoke(this);

        if (networkWeaponSpawnPoint)
        {
            Debug.Log("DisableWeapon 1");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("DisableWeapon 2");
            Destroy(gameObject);
        }
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
        _spawnPointPosition= spp;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<Rigidbody>().velocity /= 2;
        try
        {
            GetComponent<AudioSource>().clip = _collisionAudioClip;
            GetComponent<AudioSource>().Play();
        }
        catch { }
    }

    private void OnDisable()
    {
        if (networkWeaponSpawnPoint)
        {
            NetworkGameManager.instance.DisableLootableWeapon(spawnPointPosition);
        }
    }
}
