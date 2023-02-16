using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NVAFindDestination : MonoBehaviour
{
    [SerializeField] Transform _destination;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            GetComponent<NavMeshAgent>().SetDestination(_destination.position);
        }
        catch { }
    }
}
