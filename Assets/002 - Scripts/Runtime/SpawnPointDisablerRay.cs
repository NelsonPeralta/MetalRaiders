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


    private RaycastHit[] _raycastHitsBuffer = new RaycastHit[32]; // large enough for max hits
    private int _hitsCount;

    private void Awake()
    {
        _id = Random.Range(-1, -99);
    }

    // Start is called before the first frame update
    void Start()
    {

    }


    int _tempBlockLevel;
    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop) return;
        if (!player.isAlive)
        {
            // Player dead: reset temp variables
            _hitsCount = 0;
            _tempBlockLevel = 0;
            _c = SpawnPoint.SeenResetTime * 0.6f;
            return;
        }
        _c -= Time.deltaTime;

        // Step 1: RaycastAll into preallocated buffer
        _hitsCount = Physics.RaycastNonAlloc(transform.position, transform.forward, _raycastHitsBuffer, _range, _layerMask, QueryTriggerInteraction.Collide);

        if (_hitsCount == 0)
        {
            _c = SpawnPoint.SeenResetTime * 0.6f;
            return;
        }

        // Step 2: Filter out irrelevant hits (keep only layer 0 or SpawnPointDisablerRayTarget)
        int validHits = 0;
        for (int i = 0; i < _hitsCount; i++)
        {
            var t = _raycastHitsBuffer[i].transform;
            if (t.gameObject.layer == 0 || t.GetComponent<SpawnPointDisablerRayTarget>() != null)
            {
                _raycastHitsBuffer[validHits] = _raycastHitsBuffer[i];
                validHits++;
            }
        }
        _hitsCount = validHits;

        if (_hitsCount == 0)
        {
            _c = SpawnPoint.SeenResetTime * 0.6f;
            return;
        }

        // Step 3: Sort by distance (simple in-place Bubble Sort for small arrays, <=15 items)
        for (int i = 0; i < _hitsCount - 1; i++)
        {
            for (int j = 0; j < _hitsCount - 1 - i; j++)
            {
                if (_raycastHitsBuffer[j].distance > _raycastHitsBuffer[j + 1].distance)
                {
                    var temp = _raycastHitsBuffer[j];
                    _raycastHitsBuffer[j] = _raycastHitsBuffer[j + 1];
                    _raycastHitsBuffer[j + 1] = temp;
                }
            }
        }

        // Step 4 & 5: Apply _blockingLevel to each hit
        for (int i = 0; i < _hitsCount; i++)
        {
            var targetTransform = _raycastHitsBuffer[i].transform;
            var target = targetTransform.GetComponent<SpawnPointDisablerRayTarget>();
            if (target == null)
                continue;

            _tempBlockLevel = _blockingLevel;

            float distance = Vector3.Distance(targetTransform.position, transform.position);
            int blocksOfThree = Mathf.FloorToInt(distance / 3f);

            _tempBlockLevel = Mathf.Max(0, _tempBlockLevel - blocksOfThree);

            target.spawnPoint.AddBlockingLevelEntry(_id, _tempBlockLevel, 0.5f);
        }

        // Step 6: Reset timer
        _c = SpawnPoint.SeenResetTime * 0.3f;


        ////if (GameManager.instance.connection == GameManager.Connection.Online)
        //if (_c <= 0)
        //{
        //    if (player.isAlive)
        //    {
        //        // THIS RAY DOES NOT WORK IF PLAYER IS INSIDE THE SPHERE. USE THE CONTESTED VAR IN SPAWNPOINT SCRIPT FOR BETTER CONTROL
        //        _hits_ = Physics.RaycastAll(transform.position, transform.forward, _range, layerMask: _layerMask, queryTriggerInteraction: QueryTriggerInteraction.Collide).ToList();

        //        if (_hits_.Count > 0)
        //        {
        //            if (_hits_.Count > 1)
        //            {
        //                _hits_ = _hits_.Where(x => x.transform.gameObject.layer == 0 || x.transform.GetComponent<SpawnPointDisablerRayTarget>() != null).ToList();
        //                _hits_.Sort((hit1, hit2) => hit1.distance.CompareTo(hit2.distance));


        //                // closest to farthest
        //                for (int i = 0; i < _hits_.Count; i++)
        //                {
        //                    if (_hits_[i].transform.GetComponent<SpawnPointDisablerRayTarget>())
        //                    {
        //                        _tempBlockLevel = _blockingLevel;
        //                        if (_blockingLevel < 0)
        //                        {

        //                            float distance = Vector3.Distance(_hits_[i].transform.position, transform.position);

        //                            // Calculate how many "blocks of 5 units" fit into distance
        //                            int blocksOfThree = Mathf.FloorToInt(distance / 3f);

        //                            // Decrement _tempBlockLevel by that amount
        //                            _tempBlockLevel -= blocksOfThree; // since _blockingLevel is negative, adding moves toward 0

        //                        }
        //                        _hits_[i].transform.GetComponent<SpawnPointDisablerRayTarget>().spawnPoint.AddBlockingLevelEntry(_id, _tempBlockLevel, SpawnPoint.SeenResetTime);
        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (_hits_[0].transform.GetComponent<SpawnPointDisablerRayTarget>())
        //                {
        //                    _tempBlockLevel = _blockingLevel;
        //                    if (_blockingLevel < 0)
        //                    {

        //                        float distance = Vector3.Distance(_hits_[0].transform.position, transform.position);

        //                        // Calculate how many "blocks of 5 units" fit into distance
        //                        int blocksOfThree = Mathf.FloorToInt(distance / 3f);

        //                        // Decrement _tempBlockLevel by that amount
        //                        _tempBlockLevel -= blocksOfThree; // since _blockingLevel is negative, adding moves toward 0

        //                    }
        //                    _hits_[0].transform.GetComponent<SpawnPointDisablerRayTarget>().spawnPoint.AddBlockingLevelEntry(_id, _blockingLevel, SpawnPoint.SeenResetTime);
        //                }
        //            }
        //        }
        //    }

        //    _c = SpawnPoint.SeenResetTime * 0.6f;
        //}


    }
}
