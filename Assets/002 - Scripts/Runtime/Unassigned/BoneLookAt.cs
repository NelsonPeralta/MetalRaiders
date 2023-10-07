using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneLookAt : MonoBehaviour
{
    public Transform target;

    [Header("Offsets")]
    public Vector3 offset;

    private void LateUpdate()
    {
        try
        {
            transform.LookAt(target.position);
            transform.rotation *= Quaternion.Euler(offset);
        }
        catch { }
    }
}
