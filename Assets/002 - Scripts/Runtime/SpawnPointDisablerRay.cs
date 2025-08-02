using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnPointDisablerRay : MonoBehaviour
{
    public Player player;

    [SerializeField] LayerMask _layerMask;
    [SerializeField] List<Transform> _hits = new List<Transform>();
    [SerializeField] int _range, _blockingLevel, _id;

    float _c;
    List<RaycastHit> _hits_ = new List<RaycastHit>();

    private void Awake()
    {
        _id = Random.Range(0, 9999);
    }

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



            if (player.isAlive)
            {
                // THIS RAY DOES NOT WORK IF PLAYER IS INSIDE THE SPHERE. USE THE CONTESTED VAR IN SPAWNPOINT SCRIPT FOR BETTER CONTROL
                _hits_ = Physics.RaycastAll(transform.position, transform.forward, _range, layerMask: _layerMask, queryTriggerInteraction: QueryTriggerInteraction.Collide).ToList();

                if (_hits_.Count > 0)
                {
                    if (_hits_.Count > 1)
                    {
                        _hits_ = _hits_.Where(x => x.transform.gameObject.layer == 0 || x.transform.GetComponent<SpawnPointDisablerRayTarget>() != null).ToList();
                        _hits_.Sort((hit1, hit2) => hit1.distance.CompareTo(hit2.distance));


                        // closest to farthest
                        for (int i = 0; i < _hits_.Count; i++)
                        {
                            //Debug.Log($"Hit {_id} {i}:  {_hits_[i].collider.name}, Distance: {_hits_[i].distance}" +
                            //    $" {_hits_[i].transform.root.GetComponent<ThisRootBelongsToAPlayer>() == null}" +
                            //    $"{_hits_[i].transform.root.GetComponent<Player>() == null}" +
                            //    $" {_hits_[i].transform.GetComponent<SpawnPointDisablerRayTarget>() == true}");



                            if (_hits_[i].transform.GetComponent<SpawnPointDisablerRayTarget>())
                            {
                                _hits_[i].transform.GetComponent<SpawnPointDisablerRayTarget>().spawnPoint.AddBlockingLevelEntry(_id, _blockingLevel, SpawnPoint.SeenResetTime);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        //print($"Hit SINGLE {_id} {_hits_[0].transform.name}");
                        if (_hits_[0].transform.GetComponent<SpawnPointDisablerRayTarget>())
                        {
                            //_hits_[0].transform.GetComponent<SpawnPointDisablerRayTarget>().spawnPoint.seen = true;
                            _hits_[0].transform.GetComponent<SpawnPointDisablerRayTarget>().spawnPoint.AddBlockingLevelEntry(_id, _blockingLevel, SpawnPoint.SeenResetTime);
                        }
                    }
                }


                //for (int i = 0; i < _hits_.Length; i++)
                //    if (_hits_[i].transform.GetComponent<SpawnPointDisablerRayTarget>())
                //    {
                //        _hits_[i].transform.GetComponent<SpawnPointDisablerRayTarget>().spawnPoint.seen = true;
                //        _hits_[i].transform.GetComponent<SpawnPointDisablerRayTarget>().spawnPoint.AddDanger(_id, _dangerLevel, SpawnPoint.SeenResetTime);
                //    }


            }

            _c = SpawnPoint.SeenResetTime * 0.6f;
        }
    }
}
