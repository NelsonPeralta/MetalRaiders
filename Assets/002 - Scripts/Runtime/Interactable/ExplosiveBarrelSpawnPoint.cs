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
    public float tts { get { return _tts; } private set { _tts = value; } }

    private void Start()
    {
        tts = _defaultTts;

        if (PhotonNetwork.IsMasterClient)
            barrel = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Interactable", "ExplosiveBarrel"), transform.position, Quaternion.identity).GetComponent<ExplosiveBarrel>();

        placeHolder.SetActive(false);
    }

    private void Update()
    {
        tts -= Time.deltaTime;

        if (tts < 0)
        {
            if (PhotonNetwork.IsMasterClient && !barrel)
                barrel = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Interactable", "ExplosiveBarrel"), transform.position, Quaternion.identity).GetComponent<ExplosiveBarrel>();
            tts = _defaultTts;
        }
    }
}
