using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class ExplosiveBarrelSpawnPoint : MonoBehaviour
{
    [SerializeField] float _defaultTts;
    [SerializeField] float _tts;

    public GameObject placeHolder;
    public ExplosiveBarrel barrel;
    public GameObject prefab;

    public int index { get { return barrel.index; } }
    public float tts { get { return _tts; } private set { _tts = value; } }

    private void Start()
    {
        tts = _defaultTts;
        GameTime.instance.OnGameTimeChanged += OnGameTimeChanged;

        if (PhotonNetwork.IsMasterClient)
            barrel = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Interactable", "ExplosiveBarrel"), transform.position, Quaternion.identity).GetComponent<ExplosiveBarrel>();

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
        if (gameTime.totalTime % tts == 0)
        {
            barrel.gameObject.SetActive(true);

            barrel.transform.position = barrel.spawnPointPosition;
            barrel.transform.rotation = barrel.spawnPointRotation;
        }
    }
}
