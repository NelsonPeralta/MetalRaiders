using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] Portal _endpoint;

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player>() && other.GetComponent<Player>().isMine && !other.GetComponent<Player>().isDead && !other.GetComponent<Player>().isRespawning)
        {
            Vector3 t = _endpoint.transform.position + (_endpoint.transform.forward * 2);
            Vector3 r = _endpoint.transform.rotation.eulerAngles;

            other.GetComponent<Player>().Teleport(t, r);

            GetComponent<AudioSource>().Play();
        }
    }
}
