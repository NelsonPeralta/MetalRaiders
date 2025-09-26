using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSurroundings : MonoBehaviour
{
    [Header("Side Surroundings")]
    public GameObject left;
    public GameObject right;
    public GameObject front;
    public GameObject back;
    public GameObject top;

    public GameObject raySpawnPoint;
    public int maxRange;
    public LayerMask layerMask;
    public bool objectOverPlayerHead;
    Vector3 raySpawn;
    RaycastHit hit;

    // Update is called once per frame
    void Update()
    {
        ShootLOSRay();
    }

    void ShootLOSRay()
    {
        raySpawn = raySpawnPoint.transform.position + new Vector3(0, 0f, 0);
        Debug.DrawRay(raySpawn, raySpawnPoint.transform.forward * maxRange, Color.green);

        if (Physics.Raycast(raySpawn, raySpawnPoint.transform.forward * maxRange, out hit, 10000, layerMask)) // Need a Raycast Range Overload to work with LayerMask
        {
            //Log.Print(() =>"GO over head = " + hit.transform.gameObject.name);
            if(hit.transform.GetComponent<MeshCollider>() || hit.transform.GetComponent<BoxCollider>())
            {
                if (hit.transform.gameObject.layer == 14)
                {
                    objectOverPlayerHead = true;
                    //Log.Print(() =>"Object layer OH = " + hit.transform.gameObject.layer);
                }
                else
                {
                    objectOverPlayerHead = false;
                }
            }
            else
            {
                objectOverPlayerHead = false;
            }
        }
    }
}
