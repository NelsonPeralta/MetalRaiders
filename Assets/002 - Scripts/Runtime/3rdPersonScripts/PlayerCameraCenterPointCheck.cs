using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraCenterPointCheck : MonoBehaviour
{
    public Transform target { get { return _witness; } }
    public bool theObjectIntheMiddleIsBehindTheCamera { get { return _theObjectIntheMiddleIsBehindTheCamera; } }

    [SerializeField] Player _player;
    [SerializeField] Transform _hitTransform, _witness;
    [SerializeField] bool _theObjectIntheMiddleIsBehindTheCamera;

    RaycastHit _hit;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if ((GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On 
            || (_player.playerInventory.activeWeapon && _player.playerInventory.activeWeapon.weaponType == WeaponProperties.WeaponType.Heavy))
            && CurrentRoomManager.instance.gameStarted)
        {
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.forward, out _hit, 100, GameManager.instance.bulletLayerMask))
            {
                // we want to find out if the hit is between the camera and the player model. In other words, is my camera behind a wall?
                if (Vector3.Dot((_hit.point - transform.position).normalized, (_hit.point - _player.transform.position).normalized) <= 0)
                {
                    _witness.transform.localPosition = Vector3.forward * 100;
                    _hitTransform = _witness;
                }
                else
                {
                    _hitTransform = _hit.transform;
                    _witness.transform.position = _hit.point;
                }
            }
            else
            {
                _witness.transform.localPosition = Vector3.forward * 100;
                _hitTransform = _witness;
            }
        }
    }
}
