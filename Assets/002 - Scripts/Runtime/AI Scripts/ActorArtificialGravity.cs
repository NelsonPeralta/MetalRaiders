using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorArtificialGravity : MonoBehaviour
{
    [SerializeField] Transform _feet;
    [SerializeField] LayerMask _layerMask;
    [SerializeField] float _distanceToGround;
    [SerializeField] GameObject _raycastHitGo;

    RaycastHit hit;

    private void Update()
    {
        if (Physics.Raycast(_feet.position, Vector3.down, out hit, 1, layerMask: _layerMask))
        {
            _raycastHitGo = hit.collider.gameObject;
            _distanceToGround = Vector3.Distance(_feet.position, hit.point);
        }
        else
        {
            _raycastHitGo = null; _distanceToGround = 0;
        }


    }


    private void FixedUpdate()
    {
        if (_distanceToGround > 0.2f) transform.position -= new Vector3(0, 0.1f, 0);
    }
}
