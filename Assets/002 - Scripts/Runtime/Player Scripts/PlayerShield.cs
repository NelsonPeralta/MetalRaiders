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
    [SerializeField] float shieldAlpha;



    private MaterialPropertyBlock[] _mpbSlots;
    private float[] _shieldAlphaCache;
    private float[] _overshieldAlphaCache;

    private void Awake()
    {
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

        _shieldRenderers = _player.playerController.playerThirdPersonModelManager.spartanModel
            .GetComponentsInChildren<Renderer>(includeInactive: true)
            .Where(r => r.GetComponent<PlayerShieldShaderHere>())
            .ToList();

        foreach (Renderer mr in _shieldRenderers)
        {
            mr.sharedMaterials[1].SetFloat("_Alpha", 0); // normal shield
            mr.sharedMaterials[2].SetFloat("_Alpha", 0); // overshield
        }

        int count = _shieldRenderers.Count;
        _shieldAlphaCache = new float[count];
        _overshieldAlphaCache = new float[count];

        // Initialize separate MaterialPropertyBlocks for each slot
        _mpbSlots = new MaterialPropertyBlock[2];
        _mpbSlots[0] = new MaterialPropertyBlock(); // slot 1
        _mpbSlots[1] = new MaterialPropertyBlock(); // slot 2

        // Initialize all alphas to 0
        for (int i = 0; i < count; i++)
        {
            var mr = _shieldRenderers[i];

            // Slot 1
            mr.GetPropertyBlock(_mpbSlots[0]);
            _mpbSlots[0].SetFloat("_Alpha", 0f);
            mr.SetPropertyBlock(_mpbSlots[0], 1); // Apply to material slot 1
            _shieldAlphaCache[i] = 0f;

            // Slot 2
            mr.GetPropertyBlock(_mpbSlots[1]);
            _mpbSlots[1].SetFloat("_Alpha", 0f);
            mr.SetPropertyBlock(_mpbSlots[1], 2); // Apply to material slot 2
            _overshieldAlphaCache[i] = 0f;
        }
    }

    private void Update()
    {
        if (!_player) return;

        shieldAlpha = CalculateShieldAlpha();
        float overshieldAlpha = _player.overshieldPoints > 0
            ? Mathf.Clamp(_player.overshieldPoints, 0f, _player.maxOvershieldPoints - 1f) / _player.maxOvershieldPoints
            : 0f;

        for (int i = 0; i < _shieldRenderers.Count; i++)
        {
            var mr = _shieldRenderers[i];
            if (!mr.gameObject.activeInHierarchy) continue;

            // Slot 1: normal shield
            mr.GetPropertyBlock(_mpbSlots[0]);
            if (_shieldAlphaCache[i] != shieldAlpha)
            {
                _mpbSlots[0].SetFloat("_Alpha", shieldAlpha);
                _shieldAlphaCache[i] = shieldAlpha;
                mr.SetPropertyBlock(_mpbSlots[0], 1); // apply to material slot 1
            }

            // Slot 2: overshield
            mr.GetPropertyBlock(_mpbSlots[1]);
            if (_overshieldAlphaCache[i] != overshieldAlpha)
            {
                _mpbSlots[1].SetFloat("_Alpha", overshieldAlpha);
                _overshieldAlphaCache[i] = overshieldAlpha;
                mr.SetPropertyBlock(_mpbSlots[1], 2); // apply to material slot 2
            }
        }
    }

    private float CalculateShieldAlpha()
    {
        float sdp = shieldDamagePercentage;
        if (_player.isHealing)
            return sdp > 0.5f ? (1f - sdp) * 2f : sdp * 2f;
        if (sdp == 0f || sdp == 1f)
            return 0f;
        return sdp;
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
        player.splinterShardCount = 0;
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
