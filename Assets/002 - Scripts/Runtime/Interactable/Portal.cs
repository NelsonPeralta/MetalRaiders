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

            GetComponent<AudioSource>().Play();
        }else if(other.GetComponent<ExplosiveProjectile>())
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            Vector3 vel = rb.velocity;

            Destroy(other.GetComponent<ExplosiveProjectile>().visualIndicator.gameObject);

            //rb.velocity = Vector3.zero;
            //other.GetComponent<Rigidbody>().isKinematic = true;
            //other.gameObject.SetActive(false);

            Vector3 t = _endpoint.transform.position + (_endpoint.transform.forward * 1.5f);
            Vector3 r = _endpoint.transform.rotation.eulerAngles;
            other.transform.position = t;
            other.transform.eulerAngles = r;

            Instantiate(other.GetComponent<ExplosiveProjectile>().visualIndicatorPrefab, parent: other.transform, instantiateInWorldSpace: false);

            //other.GetComponent<Rigidbody>().isKinematic = false;
            //other.gameObject.SetActive(true);
            //rb.AddForce(vel, ForceMode.Impulse);

            //other.GetComponent<ExplosiveProjectile>().model.SetActive(false);


            //StartCoroutine(Delay(other.GetComponent<ExplosiveProjectile>()));
            //other.GetComponent<ExplosiveProjectile>().model.SetActive(true);
            //GetComponent<Rigidbody>().for = true;
        }
    }

    IEnumerator Delay(ExplosiveProjectile ep)
    {
        // Tried WaitForEndOfFrame, doesnt work, still sliding
        yield return new WaitForSeconds(0.1f);
        ep.model.SetActive(true);
    }
}
