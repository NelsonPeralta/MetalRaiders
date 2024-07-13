using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnPointDisablerRay : MonoBehaviour
{
    [SerializeField] LayerMask _layerMask;
    [SerializeField] List<Transform> _hits = new List<Transform>();

    float _c;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop) return;
        _c -= Time.deltaTime;

        //if (GameManager.instance.connection == GameManager.Connection.Online)
        if (_c <= 0)
        {
            //_hits = Physics.RaycastAll(transform.position, transform.forward, 30, layerMask: _layerMask).Select(obj => obj.transform).ToList();

            //if (_hits.Count > 0)
            //    for (int i = 0; i < _hits.Count; i++)
            //        try { _hits[i].transform.GetComponent<SpawnPointDisablerRayTarget>().spawnPoint.seen = true; } catch { }




            RaycastHit[] _hits_ = Physics.RaycastAll(transform.position, transform.forward, 30, layerMask: _layerMask);

            if (_hits_.Length > 0)
                for (int i = 0; i < _hits_.Length; i++)
                    if (_hits_[i].transform.GetComponent<SpawnPointDisablerRayTarget>())
                        _hits_[i].transform.GetComponent<SpawnPointDisablerRayTarget>().spawnPoint.seen = true;

            _c = SpawnPoint.SeenResetTime * 0.7f;
        }
    }
}
