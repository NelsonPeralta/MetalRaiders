using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LootableWeapon : MonoBehaviourPun //IPunObservable*/
{
    public delegate void LootableWeaponEvent(LootableWeapon lootableWeapon);
    public LootableWeaponEvent OnLooted;

    Vector3 _spawnPointPosition;
    public string cleanName;
    public string codeName;
    public int spriteId;
    public bool isWallGun;

    [SerializeField] int _ammoInThisWeapon;

    public int ammoInThisWeapon
    {
        get { return _ammoInThisWeapon; }
        set
        {
            _ammoInThisWeapon = value;
            Dictionary<string, string> param = new Dictionary<string, string>();

            param["ammo"] = ammoInThisWeapon.ToString();
            GetComponent<PhotonView>().RPC("UpdateData", RpcTarget.All, param);
        }
    }
    public int extraAmmo;
    public bool isDualWieldable;

    [SerializeField] int defaultAmmo;
    [SerializeField] int defaultExtraAmmo;

    public bool smallAmmo;
    public bool heavyAmmo;
    public bool powerAmmo;

    [SerializeField] OnlineWeaponSpawnPoint _onlineWeaponSpawnPoint;
    public OnlineWeaponSpawnPoint onlineWeaponSpawnPoint
    {
        get { return _onlineWeaponSpawnPoint; }
        set { _onlineWeaponSpawnPoint = value; }
    }

    public Vector3 spawnPointPosition
    {
        get { return _spawnPointPosition; }
        set { _spawnPointPosition = value; }
    }


    [SerializeField] float _ttl;
    public float ttl
    {
        get { return _ttl; }
        set
        {
            _ttl = value;
            Dictionary<string, string> param = new Dictionary<string, string>();

            param["ttl"] = ttl.ToString();
            GetComponent<PhotonView>().RPC("UpdateData", RpcTarget.All, param);
        }
    }

    private void Awake()
    {
        _ttl = 0;
    }

    private void OnEnable()
    {
        try
        {
            spriteId = -1;
            spriteId = WeaponProperties.spriteIdDic[codeName];

            if (spriteId == -1)
                spriteId = WeaponProperties.spriteIdDic[cleanName];
        }
        catch { }
    }
    private void Start()
    {
        defaultAmmo = ammoInThisWeapon;
        defaultExtraAmmo = extraAmmo;
        spawnPointPosition = new Vector3((float)System.Math.Round(transform.position.x, 1), (float)System.Math.Round(transform.position.y, 1), (float)System.Math.Round(transform.position.z, 1));
    }

    private void Update()
    {
        if (onlineWeaponSpawnPoint)
            return;
        _ttl -= Time.deltaTime;

        if (!onlineWeaponSpawnPoint && _ttl <= 0)
            Destroy(gameObject);
    }

    public void ResetAmmo()
    {
        ammoInThisWeapon = defaultAmmo;
        extraAmmo = defaultExtraAmmo;
    }

    public void RandomAmmo()
    {
        ammoInThisWeapon = (int)Mathf.Ceil(Random.Range(0, ammoInThisWeapon));
        extraAmmo = (int)Mathf.Ceil(Random.Range(0, extraAmmo));

        defaultAmmo = (int)Mathf.Ceil(Random.Range(0, defaultAmmo)); ;
        extraAmmo = (int)Mathf.Ceil(Random.Range(0, extraAmmo));
    }

    public void LootWeapon(bool onlyExtraAmmo = false)
    {
        int ammoToLoot = extraAmmo;
        PlayerInventory playerInventory = GameManager.GetMyPlayer().playerInventory;
        if (!onlyExtraAmmo)
            ammoToLoot += ammoInThisWeapon;

        foreach (GameObject wp in playerInventory.allWeaponsInInventory)
            if (wp.GetComponent<WeaponProperties>().codeName == codeName)
                wp.GetComponent<WeaponProperties>().spareAmmo += ammoToLoot;

        OnLooted?.Invoke(this);
        if (onlineWeaponSpawnPoint)
        {
            onlineWeaponSpawnPoint.StartRespawn();
            gameObject.SetActive(false);
        }
        else
            Destroy(gameObject);
    }

    public void EnableWeapon()
    {
        gameObject.SetActive(true);
        ResetAmmo();
    }

    [PunRPC]
    void UpdateData(Dictionary<string, string> param)
    {
        if (param.ContainsKey("ammo"))
            _ammoInThisWeapon = int.Parse(param["ammo"]);

        if (param.ContainsKey("ttl"))
            _ttl = int.Parse(param["ttl"]);
    }
}
