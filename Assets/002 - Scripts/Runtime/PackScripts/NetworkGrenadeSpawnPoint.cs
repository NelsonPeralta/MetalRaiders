using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Threading;

public class NetworkGrenadeSpawnPoint : MonoBehaviour
{
    public enum Type { frag, plasma }
    public Type type;
    public bool enable
    {
        set
        {
            print($"oneobjmode - NetworkGrenadeSpawnPoint {value}");
            ammoInThisPack = defaultAmmo;
            UpdateAmmoText();
            if (value) _tts = _defaultTts;
            _model.SetActive(value);
            GetComponent<SphereCollider>().enabled = value;
        }
    }
    public Vector3 spawnPoint { get { return _spawnPoint; } }
    public int index { get { return _index; } }

    [SerializeField] GameObject _model;
    [SerializeField] int _index;
    [SerializeField] int _defaultTts;
    [SerializeField] float _tts;

    Vector3 _spawnPoint;







    public PhotonView PV;

    [Header("Single")]
    public WeaponPool weaponPool;

    [Header("Ammo")]
    public bool randomAmmo;
    public string ammoType;
    public int defaultAmmo;
    [SerializeField] int ammoInThisPack;


    private void Awake()
    {
        if (!GameManager.instance) Destroy(gameObject);
    }
    private void Start()
    {
        _tts = _defaultTts;

        int i = 0;
        foreach (NetworkGrenadeSpawnPoint eb in FindObjectsOfType<NetworkGrenadeSpawnPoint>())
        {
            if (eb == this)
                _index = i;
            i++;
        }


        _spawnPoint = transform.position;
        weaponPool = FindObjectOfType<WeaponPool>();
        ammoInThisPack = GetNewAmmo();
        UpdateAmmoText();

        CurrentRoomManager.instance.AddSpawnedMappAddOn(transform);
    }

    private void Update()
    {
        if (!_model)
            return;
        if (index != 0)
            return;

        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            return;


        if (CurrentRoomManager.instance.gameStarted)
        {
            if (GameManager.instance.oneObjMode == GameManager.OneObjMode.Off)
                _tts -= Time.deltaTime;
            else
            {
                if (!GameManager.instance.OneObjModeRoundOver && GameTime.instance.roundTimeRemaining > 0)
                {
                    _tts -= Time.deltaTime;
                }
            }
        }

        if (_tts < 0)
        {
            if (PhotonNetwork.IsMasterClient)
                NetworkGameManager.instance.EnableGrenadePacks();
            _tts = _defaultTts;
        }
    }

    int GetNewAmmo()
    {
        int newAmmo = 0;
        if (!randomAmmo)
            newAmmo = defaultAmmo;
        else
            newAmmo = (int)Mathf.Floor(Random.Range(1, (defaultAmmo * 0.7f)));
        if (newAmmo <= 0)
            newAmmo = 1;
        return newAmmo;
    }

    void UpdateAmmoText()
    {
        //ammoText.text = ammoInThisPack.ToString();
    }

    public void SetRandomAmmoAsDefault()
    {
        randomAmmo = true;
        ammoInThisPack = GetNewAmmo();
        UpdateAmmoText();
    }

    public int GetAmmo()
    {
        return ammoInThisPack;
    }
}
