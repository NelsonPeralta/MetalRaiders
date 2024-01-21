using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDetection : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"OnCollisionDetection {collision.gameObject.name}");
    }
}
