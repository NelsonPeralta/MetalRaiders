using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
    public float Health;

    [Header("Settings")]
    public bool canBeDestroyed;
    public bool canExplode;
    public bool objectHasBeenDestroyed;
    public bool dropsLoot;
    public MeshRenderer objectBeforeDestruction;
    public MeshRenderer objectAfterDestruction;
    public GameObject explosionPrefab;
    public GameObject fireEffect;
    public GameObject playerWhoShotLast;
    public GameObject lootPrefab;


    private void Update()
    {
        if(canExplode)
        {
            if(Health <= 0 && !objectHasBeenDestroyed)
            {
                explodeDestroyableObject();
            }
        }
    }

    public void explodeDestroyableObject()
    {
        var explosion = Instantiate(explosionPrefab, transform.position + new Vector3(0, 1, 0), transform.rotation);
        explosion.GetComponent<AIGrenade>().playerWhoThrewGrenade = playerWhoShotLast;
        Destroy(explosion, 2);
        var fire = Instantiate(fireEffect, transform.position, transform.rotation);

        if (objectAfterDestruction != null)
        {
            objectAfterDestruction.enabled = true;

            objectBeforeDestruction.enabled = false;
        }
        else if(dropsLoot)
        {
            var loot = Instantiate(lootPrefab, transform.position + new Vector3(0, 0, 0), transform.rotation);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
}
