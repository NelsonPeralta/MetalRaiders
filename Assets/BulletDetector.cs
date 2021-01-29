using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDetector : MonoBehaviour
{

    public float bSpeed;
    public BulletScript bs;
    public LayerMask layerMask;

    public GameObject collision;

    //public AutomaticWeaponScript ws;

    Vector3 prePos;

    void Start()
    {

        //Debug.Log(ws.bulletForce);
        prePos = transform.position;

    }

    void FixedUpdate()
    {
        prePos = transform.position;

        //Debug.Log(prePos);

        transform.Translate(Vector3.forward * Time.deltaTime * bSpeed);

        RaycastHit[] hits = Physics.RaycastAll(new Ray(prePos, (transform.position - prePos).normalized), (transform.position - prePos).magnitude, layerMask);



        for (int i = 0; i < hits.Length; i++)
        {
            Debug.Log(hits[i].collider.gameObject.name);
            //collision = hits[i].collider.gameObject;



            /*
            //If bullet collides with "Blood" tag
            if (collision.transform.tag == "Blood")
            {
                //Instantiate random impact prefab from array
                Instantiate(bs.bloodImpactPrefabs[Random.Range
                    (0, bs.bloodImpactPrefabs.Length)], transform.position,
                    Quaternion.LookRotation(hits[0].normal));
                //Destroy bullet object
                Destroy(gameObject);
            }
            */
            Debug.DrawLine(transform.position, prePos);

        }
    }
}
