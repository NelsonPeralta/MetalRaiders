using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] Portal _endpoint;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"PORTAL: {other.name}");

        if (other.GetComponent<PlayerCapsule>() && other.GetComponent<PlayerCapsule>().player.isMine && !other.GetComponent<PlayerCapsule>().player.isDead && !other.GetComponent<PlayerCapsule>().player.isRespawning)
        {
            Vector3 t = _endpoint.transform.position + (_endpoint.transform.forward * 1.5f);
            Vector3 r = _endpoint.transform.rotation.eulerAngles;

            other.GetComponent<PlayerCapsule>().player.Teleport(t, _endpoint.transform.forward);

            //GetComponent<AudioSource>().Play();
            _endpoint.GetComponent<AudioSource>().Play();
        }
        else if (other.GetComponent<ExplosiveProjectile>())
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
            other.GetComponent<ExplosiveProjectile>().visualIndicator.SetActive(false); // Good

            other.transform.position = _endpoint.transform.position + (_endpoint.transform.forward * 1.5f);
            other.transform.eulerAngles = _endpoint.transform.rotation.eulerAngles;

            //other.GetComponent<ExplosiveProjectile>().visualIndicatorDuplicate.SetActive(true);
            other.GetComponent<Rigidbody>().AddForce(_endpoint.transform.forward * 300);
        }
        else if (other.GetComponent<Bullet>())
        {
            Vector3 t = _endpoint.transform.position + (_endpoint.transform.forward * 1.5f);
            Vector3 r = _endpoint.transform.rotation.eulerAngles;
            other.transform.position = t;
            other.transform.eulerAngles = r;
        }
    }
}
