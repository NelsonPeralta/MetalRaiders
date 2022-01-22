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

    void OnPlayerTestButton_Delegate(PlayerController playerController)
    {

    }

    // Sounds
    void PlayShieldHitSound()
    {
        shieldAudioSource.clip = shieldHitClip;
        shieldAudioSource.Play();
    }

    void PlayShieldStartSound()
    {
        shieldAudioSource.clip = shieldStartClip;
        shieldAudioSource.Play();
    }

    void PlayShieldDownSound()
    {
        shieldAudioSource.clip = shieldDownClip;
        shieldAudioSource.Play();
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
    }

    void HideThirdPersionShieldElectricityModel()
    {
        if (shieldElectricityThirdPersonModel.activeSelf)
            shieldElectricityThirdPersonModel.SetActive(false);
    }
    void ShowThirdPersonShieldModel()
    {
        StartCoroutine(ShowThirdPersonShieldModel_Coroutine());
    }

    IEnumerator ShowThirdPersonShieldModel_Coroutine()
    {
        shieldThirdPersonModel.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        shieldThirdPersonModel.SetActive(false);
    }

    void ShowThirdPersonShieldRechargeModel()
    {
        StartCoroutine(ShowThirdPersonShieldRechargeModel_Coroutine());
    }

    IEnumerator ShowThirdPersonShieldRechargeModel_Coroutine()
    {
        shieldRechargeThirdPersonModel.SetActive(true);
        yield return new WaitForSeconds(2f);
        shieldRechargeThirdPersonModel.SetActive(false);
    }

}
