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
    public GameObject shieldImpactPrefab;
    public GameObject shieldElectricityThirdPersonModel;
    public GameObject shieldRechargeThirdPersonModel;

    [Header("Shield Sounds")]
    public AudioSource shieldAudioSource;
    public AudioSource shieldAlarmAudioSource;
    public AudioClip shieldStartClip;
    public AudioClip shieldDownClip;
    public AudioClip healthStartClip;
    public AudioClip shieldHitClip;
    public AudioClip shieldAlarmClip;


    float _maxShield;
    float _shield;
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
        //shield = (float)GetComponent<Player>().maxShield;

        GetComponent<PlayerController>().OnPlayerTestButton += OnPlayerTestButton_Delegate;
    }

    private void Start()
    {
        GetComponent<Player>().OnPlayerShieldDamaged += OnPlayerShieldDamaged_Delegate;
        GetComponent<Player>().OnPlayerShieldDamaged += PlayShieldHitSound;
        GetComponent<Player>().OnPlayerShieldRechargeStarted += PlayShieldStartSound;
        GetComponent<Player>().OnPlayerShieldBroken += PlayShieldDownSound;

        GetComponent<Player>().OnPlayerDeath += OnPlayerDeath_Delegate;
        GetComponent<Player>().OnPlayerRespawned += OnPlayerRespawned_Delegate;
    }

    void OnPlayerShieldDamaged_Delegate(Player player)
    {
        try
        {
            var a = Instantiate(shieldImpactPrefab, (Vector3)player.impactPos, Quaternion.identity);
            Destroy(a, 0.5f);
        }catch { }
    }


    void OnPlayerTestButton_Delegate(PlayerController playerController)
    {

    }

    // Sounds

    void OnPlayerDeath_Delegate(Player player)
    {
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

    void PlayShieldStartSound(Player player)
    {
        shieldAudioSource.clip = shieldStartClip;
        shieldAudioSource.Play();

        StopShieldAlarmSound();
        HideThirdPersionShieldElectricityModel();
    }

    public void PlayShieldDownSound(Player player)
    {
        shieldAudioSource.clip = shieldDownClip;
        shieldAudioSource.Play();

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
            shieldElectricityThirdPersonModel.SetActive(true);
        shieldRechargeThirdPersonModel.SetActive(false);
    }

    void HideThirdPersionShieldElectricityModel()
    {
        if (shieldElectricityThirdPersonModel.activeSelf)
            shieldElectricityThirdPersonModel.SetActive(false);
        shieldRechargeThirdPersonModel.SetActive(true);
    }
}
