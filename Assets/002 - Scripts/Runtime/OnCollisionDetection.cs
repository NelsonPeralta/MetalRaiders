using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDetection : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Log.Print(() =>$"OnCollisionDetection {collision.gameObject.name}");
    }
}
