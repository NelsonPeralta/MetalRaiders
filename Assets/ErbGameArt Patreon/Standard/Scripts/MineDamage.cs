using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineDamage : MonoBehaviour
{
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }
    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);
        int i = 0;
        while (i < numCollisionEvents)
        {
            Visible();
            i++;
        }
    }

    public GameObject Object;
    private void Visible()
    {
        Object.SetActive(true);
    }
}
