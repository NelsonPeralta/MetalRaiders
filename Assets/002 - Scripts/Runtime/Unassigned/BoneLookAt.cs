using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneLookAt : MonoBehaviour
{
    public Transform target;

    [Header("Offsets")]
    public Vector3 offset;

    Player _p;

    private void LateUpdate()
    {
        if (transform.root.GetComponent<Player>())
        {
            if (_p == null) _p = transform.root.GetComponent<Player>();

            if (_p.isAlive && !_p.playerController.cameraIsFloating)
            {
                transform.LookAt(target.position);
                transform.rotation *= Quaternion.Euler(offset);
            }
        }
        else
        {
            try
            {
                transform.LookAt(target.position);
                transform.rotation *= Quaternion.Euler(offset);
            }
            catch { }
        }
    }
}
