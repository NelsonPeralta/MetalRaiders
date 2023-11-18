using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollLimbCollisionDetection : MonoBehaviour
{
    public Ragdoll ragdoll;
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"RagdollLimbCollisionDetection {collision.relativeVelocity.sqrMagnitude}");
        if (collision.relativeVelocity.sqrMagnitude > 10)
        {
            ragdoll.HandleCollision(collision);
        }
    }
}
