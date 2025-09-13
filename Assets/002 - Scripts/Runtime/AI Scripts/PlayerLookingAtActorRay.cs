using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookingAtActorRay : MonoBehaviour
{
    [SerializeField] Player p;
    [SerializeField] LayerMask _layerMask;
    [SerializeField] GameObject _hitObj;

    RaycastHit hit;
    float _c;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        _c -= Time.deltaTime;

        //if (GameManager.instance.connection == GameManager.Connection.Online)
        if (_c <= 0)
        {
            // Does the ray intersect any objects excluding the player layer
            //if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 40, _layerMask, QueryTriggerInteraction.Collide))
            //{
            //    _hitObj = hit.transform.gameObject;
            //    //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

            //    PrintOnlyInEditor.Log(_hitObj.GetComponent<PlayerLookingAtActorRayTarget>());
            //    if (_hitObj.GetComponent<PlayerLookingAtActorRayTarget>())
            //        _hitObj.GetComponent<PlayerLookingAtActorRayTarget>().Actor.ResetSeenCooldown(p);

            //}
            //else _hitObj = null;





            RaycastHit[] _hits_ = Physics.RaycastAll(transform.position, transform.forward, 30, layerMask: _layerMask);

            if (_hits_.Length > 0)
                for (int i = 0; i < _hits_.Length; i++)
                {
                    if (_hits_[i].transform.GetComponent<PlayerLookingAtActorRayTarget>())
                        _hits_[i].transform.GetComponent<PlayerLookingAtActorRayTarget>().Actor.ResetSeenCooldown(p);
                }


            _c = SpawnPoint.SeenResetTime * 0.4f;
        }
    }
}
