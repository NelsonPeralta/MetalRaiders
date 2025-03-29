using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraCenterPointCheck : MonoBehaviour
{
    public Transform target { get { return _witness; } }

    [SerializeField] Transform _hitTransform, _witness;


    RaycastHit _hit;





    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On
            && CurrentRoomManager.instance.gameStarted)
        {
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.forward, out _hit, 100, GameManager.instance.bulletLayerMask))
            {
                _hitTransform = _hit.transform;
                _witness.transform.position = _hit.point;
            }
            else
            {
                _witness.transform.localPosition = Vector3.forward * 100;
                _hitTransform = _witness;
            }
        }
    }
}
