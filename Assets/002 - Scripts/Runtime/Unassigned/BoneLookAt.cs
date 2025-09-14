using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneLookAt : MonoBehaviour
{
    public Transform target;

    [Header("Offsets")]
    public Vector3 offset;

    Player _player;
    private Transform _root;
    private Quaternion _offsetQuat;


    private void Start()
    {
        _root = transform.root;
        _player = _root.GetComponent<Player>();
        _offsetQuat = Quaternion.Euler(offset); // compute once
    }

    private void LateUpdate()
    {
        if (_player != null)
        {
            if (_player.isAlive && !_player.playerController.cameraIsFloating)
            {
                transform.LookAt(target.position);
                transform.rotation *= _offsetQuat;
            }
        }
        else
        {
            // fallback if no Player component
            if (target != null)
            {
                transform.LookAt(target.position);
                transform.rotation *= _offsetQuat;
            }
        }
    }
}
