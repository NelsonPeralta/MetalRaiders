using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatcherWall : MonoBehaviour
{
    public BoxCollider collider;

    private void Start()
    {
        collider = GetComponent<BoxCollider>();
        StartCoroutine(DeactivateCollider());
    }

    IEnumerator DeactivateCollider()
    {
        yield return new WaitForSeconds(2);
        Destroy(collider);
        Destroy(gameObject, 1);
    }
}
