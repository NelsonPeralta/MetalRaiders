using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] List<Renderer> _shieldRenderers = new List<Renderer>();




    public float shield
    {
        get { return _shield; }
        set
        {
            if (value < shield)
                OnPlayerShieldDamaged?.Invoke(this);
            if (value != shield)
            {
                OnPlayerShieldChanged?.Invoke(this);

            }

            _shield = value;
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

    float shieldDamagePercentage
    {
        get
        {
            return 1 - (_player.shieldPoints / _player.maxShieldPoints);
        }
    }


    float _shieldDamagePercentage;






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



        _shieldRenderers = _player.playerController.playerThirdPersonModelManager.spartanModel.GetComponentsInChildren<Renderer>(includeInactive: true).Where(item => item.GetComponent<PlayerShieldShaderHere>()).ToList();

        foreach (Renderer mr in _shieldRenderers)
        {
            mr.sharedMaterials[1].SetFloat("_Alpha", 0); // normal shield
            mr.sharedMaterials[2].SetFloat("_Alpha", 0); // overshield
            //mr.materials[1].SetFloat("_Alpha", 0);
        }
    }


    private void Update()
    {
        if (_player)
        {
            foreach (Renderer mr in _shieldRenderers)
            {
                if (mr.gameObject.activeInHierarchy)
                {
                    if (_player.isHealing)
                    {
                        if (shieldDamagePercentage > 0.5f)
                            mr.materials[1].SetFloat("_Alpha", (1 - shieldDamagePercentage) * 2);
                        else
                            mr.materials[1].SetFloat("_Alpha", shieldDamagePercentage * 2);
                    }
                    else if (shieldDamagePercentage == 1)
                        mr.materials[1].SetFloat("_Alpha", 0);
                    else
                        mr.materials[1].SetFloat("_Alpha", shieldDamagePercentage);

                    if (_player.overshieldPoints > 0)
                    {
                        mr.materials[2].SetFloat("_Alpha", Mathf.Clamp(_player.overshieldPoints, 0, _player.maxOvershieldPoints - 1) / _player.maxOvershieldPoints);
                    }
                    else
                    {
                        if (mr.materials[2].GetFloat("_Alpha") != 0)
                        {
                            mr.materials[2].SetFloat("_Alpha", 0);
                        }
                    }
                }
            }
        }
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
                    GameManager.SetLayerRecursively(shieldElectricityThirdPersonModel, 25);
                else if (_player.playerController.rid == 1)
                    GameManager.SetLayerRecursively(shieldElectricityThirdPersonModel, 27);
                else if (_player.playerController.rid == 2)
                    GameManager.SetLayerRecursively(shieldElectricityThirdPersonModel, 29);
                else if (_player.playerController.rid == 3)
                    GameManager.SetLayerRecursively(shieldElectricityThirdPersonModel, 31);
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
