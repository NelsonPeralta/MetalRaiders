using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    public delegate void PlayerShieldEvent(PlayerShield playerShield);
    public PlayerShieldEvent OnPlayerShieldDamaged, OnPlayerShieldChanged;

    [Header("Models")]
    public GameObject shieldModel;
    public GameObject shieldThirdPersonModel;
    public GameObject shieldElectricityThirdPersonModel;
    public GameObject shieldRechargeThirdPersonModel;

    [Header("Shield Sounds")]
    public AudioSource shieldAudioSource, shieldBorkenAudioSource;
    public AudioSource shieldAlarmAudioSource;
    public AudioClip shieldStartClip;
    public AudioClip shieldDownClip;
    public AudioClip healthStartClip;
    public AudioClip shieldHitClip;
    public AudioClip shieldAlarmClip;


    float _maxShield;
    float _shield;

    Player _player;



    List<GameObject> _shieldHits = new List<GameObject>();




    public float shield
    {
        get { return _shield; }
        set
        {
            if (value < shield)
                OnPlayerShieldDamaged?.Invoke(this);
            if (value != shield)
                OnPlayerShieldChanged?.Invoke(this);

            shield = value;
        }
    }

    bool _shieldIsRecharging;
    bool shieldIsRecharging
    {
        get { return _shieldIsRecharging; }
        set
        {
            _shieldIsRecharging = value;
        }
    }









    private void Awake()
    {
        //shield = (float)_player.maxShield;
        _player = GetComponent<Player>();
        GetComponent<PlayerController>().OnPlayerTestButton += OnPlayerTestButton_Delegate;
    }

    private void Start()
    {
        _player.OnPlayerShieldDamaged += OnPlayerShieldDamaged_Delegate;
        _player.OnPlayerShieldDamaged += PlayShieldHitSound;
        _player.OnPlayerShieldRechargeStarted += PlayShieldStartSound;
        _player.OnPlayerShieldBroken += PlayShieldDownSound;

        _player.OnPlayerDeath += OnPlayerDeath_Delegate;
        _player.OnPlayerRespawned += OnPlayerRespawned_Delegate;

    }




    public void SpawnShieldHit()
    {
        if (_player.impactPos != null)
        {
            GameObject genericHit = GameObjectPool.instance.SpawnPooledShieldHit();
            genericHit.transform.position = (Vector3)_player.impactPos;
            genericHit.SetActive(true);
        }
    }



    void OnPlayerShieldDamaged_Delegate(Player player)
    {
        SpawnShieldHit();
    }


    void OnPlayerTestButton_Delegate(PlayerController playerController)
    {

    }

    // Sounds

    void OnPlayerDeath_Delegate(Player player)
    {
        Debug.Log("PlayerShield OnPlayerDeath_Delegate");
        player.GetComponent<PlayerUI>().shieldBar.SetActive(false);
        StopShieldAlarmSound();
        HideThirdPersionShieldElectricityModel();
    }

    void OnPlayerRespawned_Delegate(Player player)
    {
        player.GetComponent<PlayerUI>().shieldBar.SetActive(true);
    }

    void PlayShieldHitSound(Player player)
    {
        shieldAudioSource.clip = shieldHitClip;
        shieldAudioSource.Play();
    }

    public void PlayShieldStartSound(Player player)
    {
        player.ultraMergeCount = 0;
        shieldAudioSource.clip = shieldStartClip;
        shieldAudioSource.Play();

        StopShieldAlarmSound();
        HideThirdPersionShieldElectricityModel();
    }

    public void PlayShieldDownSound(Player player)
    {
        shieldBorkenAudioSource.Play();
        //shieldAudioSource.clip = shieldDownClip;
        //shieldAudioSource.Play();

        PlayShieldAlarmSound();
        ShowThirdPersionShieldElectricityModel();
    }
    void PlayShieldAlarmSound()
    {
        if (!shieldAlarmAudioSource.isPlaying && shieldAlarmAudioSource.gameObject.activeSelf)
        {
            shieldAlarmAudioSource.clip = shieldAlarmClip;
            shieldAlarmAudioSource.Play();
        }
    }

    void StopShieldAlarmSound()
    {
        shieldAlarmAudioSource.Stop();
    }

    void ShowThirdPersionShieldElectricityModel()
    {
        if (!shieldElectricityThirdPersonModel.activeSelf)
        {
            if (_player.PV.IsMine)
            {
                if (_player.playerController.rid == 0)
                    GameManager.SetLayerRecursively(shieldElectricityThirdPersonModel, 24);
                else if (_player.playerController.rid == 1)
                    GameManager.SetLayerRecursively(shieldElectricityThirdPersonModel, 26);
                else if (_player.playerController.rid == 2)
                    GameManager.SetLayerRecursively(shieldElectricityThirdPersonModel, 28);
                else if (_player.playerController.rid == 3)
                    GameManager.SetLayerRecursively(shieldElectricityThirdPersonModel, 30);
            }



            shieldElectricityThirdPersonModel.SetActive(true);
        }
        shieldRechargeThirdPersonModel.SetActive(false);
    }

    void HideThirdPersionShieldElectricityModel()
    {
        if (shieldElectricityThirdPersonModel.activeSelf)
            shieldElectricityThirdPersonModel.SetActive(false);
        ShowShieldRechargeEffect();
    }

    public void ShowShieldRechargeEffect()
    {
        shieldRechargeThirdPersonModel.SetActive(false);
        shieldRechargeThirdPersonModel.SetActive(true);

    }
}
