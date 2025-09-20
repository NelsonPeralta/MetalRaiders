using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollLimbCollisionDetection : MonoBehaviour
{
    public Ragdoll ragdoll;

    Vector3 _lastPos;



    private void Awake()
    {
        _lastPos = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.sqrMagnitude > 15)
        {
            Log.Print($"RagdollLimbCollisionDetection {collision.collider.name}");
            ragdoll.HandleCollision(collision);
        }
    }


    private void Update()
    {
        if (_lastPos != Vector3.zero && _lastPos != transform.position)
        {
            RaycastHit[] hits;

            hits = Physics.RaycastAll(_lastPos, (transform.position - _lastPos), (transform.position - _lastPos).magnitude);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.layer == 4)
                {
                    Debug.Log($"RagdollLimbCollisionDetection splash check: {GetComponent<Rigidbody>().linearVelocity.magnitude}");
                    if (GetComponent<Rigidbody>().mass > 3 && GetComponent<Rigidbody>().linearVelocity.magnitude > 12)
                    {
                        GameObjectPool.instance.SpawnBigWaterEffect(hits[i].point);
                    }
                    else
                    {
                        GameObjectPool.instance.SpawnSmallWaterEffect(hits[i].point);
                    }


                }
            }
        }
        _lastPos = transform.position;
    }

    private void OnDisable()
    {
        _lastPos = Vector3.zero;
    }
}
