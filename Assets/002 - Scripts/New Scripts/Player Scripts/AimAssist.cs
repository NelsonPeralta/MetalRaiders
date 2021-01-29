using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAssist : MonoBehaviour
{
    public List<GameObject> targets = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Object entered aim assist: {other.name}");
        if (other.GetComponent<PlayerHitbox>() || other.GetComponent<AIHitbox>())
            targets.Add(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Object entered aim assist {collision.gameObject.name}");
        if (collision.gameObject.GetComponent<PlayerHitbox>() || collision.gameObject.GetComponent<AIHitbox>())
            targets.Add(collision.gameObject);
    }
}
