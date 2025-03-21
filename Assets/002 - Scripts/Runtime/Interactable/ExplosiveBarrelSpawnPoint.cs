using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Threading;

public class ExplosiveBarrelSpawnPoint : Hazard
{
    [SerializeField] float _defaultTts;
    [SerializeField] float _tts;

    public GameObject placeHolder;
    public ExplosiveBarrel barrel;
    public GameObject explosion;
    public GameObject prefab;

    public int index { get { return barrel.index; } }
    public float tts { get { return _tts; } private set { _tts = value; } }


    private void OnDisable()
    {
        GameTime.instance.OnGameTimeElapsedChanged -= OnGameTimeChanged;
    }
    private void OnDestroy()
    {
        GameTime.instance.OnGameTimeElapsedChanged -= OnGameTimeChanged;

    }

    private void Start()
    {
        tts = _defaultTts;
        GameTime.instance.OnGameTimeElapsedChanged -= OnGameTimeChanged;
        GameTime.instance.OnGameTimeElapsedChanged += OnGameTimeChanged;

        //if (PhotonNetwork.IsMasterClient)
        //{
        //    GameObject b = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Interactable", "ExplosiveBarrel"), transform.position, Quaternion.identity).GetComponent<ExplosiveBarrel>().gameObject;
        //    b.transform.parent = transform;
        //}

        barrel.UpdateLastPlayerWhoDamaged(-999);
        placeHolder.SetActive(false);
    }

    private void Update()
    {
        return;

        if (!barrel)
            return;
        if (index != 0)
            return;

        tts -= Time.deltaTime;

        if (tts < 0)
        {
            if (PhotonNetwork.IsMasterClient)
                NetworkGameManager.instance.ResetAllExplosiveBarrels();
            tts = _defaultTts;
        }
    }


    void OnGameTimeChanged(GameTime gameTime)
    {
        if (!CurrentRoomManager.instance.gameOver)
            if (gameTime.timeRemaining % tts == 0 && gameTime.timeRemaining > 0)
            {
                Debug.Log("ExplosiveBarrelSpawnPoint OnGameTimeChanged");
                explosion.gameObject.SetActive(false);

                barrel.UpdateLastPlayerWhoDamaged(-999);
                barrel.transform.position = barrel.spawnPointPosition;
                barrel.transform.rotation = barrel.spawnPointRotation;
                barrel.GetComponent<Rigidbody>().velocity = Vector3.zero; barrel.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

                barrel.gameObject.SetActive(true);
            }
    }

    public void TriggerExplosionCoroutine()
    {
        if (!CurrentRoomManager.instance.gameOver)
        {
            barrel.gameObject.SetActive(false);
            StartCoroutine(BarrelExplosion_Coroutine());
        }
    }

    IEnumerator BarrelExplosion_Coroutine()
    {
        //explosion.transform.position = barrel.transform.position + new Vector3(0, 1, 0);
        //explosion.GetComponent<Explosion>().damageSource = "Barrel";
        //explosion.GetComponent<Explosion>().player = GameManager.GetPlayerWithPhotonViewId(barrel.lastPid);


        //yield return new WaitForSeconds(0.1f);


        //explosion.SetActive(true);

        print($"BarrelExplosion_Coroutine {barrel.lastPid}");
        yield return new WaitForSeconds(0.05f);
        GrenadePool.SpawnExplosion(GameManager.GetPlayerWithPhotonView(barrel.lastPid), damage: 500, radius: 6, expPower: GameManager.DEFAULT_EXPLOSION_POWER, damageCleanNameSource: "Barrel",
            barrel.transform.position + new Vector3(0, 1, 0), Explosion.Color.Yellow, Explosion.Type.Barrel, GrenadePool.instance.barrelClip, WeaponProperties.KillFeedOutput.Barrel);
    }
}
