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
            Vector3 t = _endpoint.transform.position + (_endpoint.transform.forward * 1.5f);
            Vector3 r = _endpoint.transform.rotation.eulerAngles;

            other.GetComponent<Player>().Teleport(t, r);

            //GetComponent<AudioSource>().Play();
            _endpoint.GetComponent<AudioSource>().Play();
        }else if(other.GetComponent<ExplosiveProjectile>())
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            Vector3 vel = rb.velocity;

            other.GetComponent<ExplosiveProjectile>().visualIndicator.SetActive(false); // Good

            Vector3 t = _endpoint.transform.position + (_endpoint.transform.forward * 1.5f);
            Vector3 r = _endpoint.transform.rotation.eulerAngles;
            other.transform.position = t;
            other.transform.eulerAngles = r;

            other.GetComponent<ExplosiveProjectile>().visualIndicatorDuplicate.SetActive(true);

        }else if (other.GetComponent<Bullet>())
        {
            Vector3 t = _endpoint.transform.position + (_endpoint.transform.forward * 1.5f);
            Vector3 r = _endpoint.transform.rotation.eulerAngles;
            other.transform.position = t;
            other.transform.eulerAngles = r;
        }
    }
}
