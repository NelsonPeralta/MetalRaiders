using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnPointDisablerRay : MonoBehaviour
{
    [SerializeField] LayerMask _layerMask;
    [SerializeField] List<Transform> _hits = new List<Transform>();


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _hits = Physics.RaycastAll(transform.position, transform.forward, 40).Select(obj => obj.transform).ToList();

        for (int i = 0; i < _hits.Count; i++)
        {
            try { _hits[i].transform.GetComponent<SpawnPointDisablerRayTarget>().spawnPoint.seen = true; } catch { }
        }
    }
}
