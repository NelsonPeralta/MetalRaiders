using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollLimbCollisionDetection : MonoBehaviour
{
    public Ragdoll ragdoll;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.sqrMagnitude > 15)
        {
            ragdoll.HandleCollision(collision);
        }
    }
}
