using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public GameObject playerWhoThrewGrenade;

    [Header("Settings")]
    public float damage; // Determined in Weapon Properties Script
    public float force;
    public float radius;
    public float power;
    public int deSpawnTime;

    [Header("Other")]
    public bool useConstantForce;

    [Header("Prefabs")]
    public Transform explosionPrefab;

    GameObject[] AIsHit = new GameObject[20];

    private void Start()
    {
        //If not using constant force (grenade launcher projectile)
        if (!useConstantForce)
        {
            //Launch the projectile forward by adding force to it at start
            GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * force);
        }
        else
        {
            GetComponent<Rigidbody>().useGravity = false;
            //GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        if (useConstantForce == true)
        {
            //Launch the projectile forward with a constant force (used for rockets)
            GetComponent<Rigidbody>().AddForce
                (gameObject.transform.forward * force);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Fireball collided with: " + collision.gameObject.name);
        StartCoroutine(Despawn());
        Explosion();
    }



    void Explosion()
    {
        var explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
        Destroy(explosion.gameObject, 5);

        //Explosion force
        Vector3 explosionPos = transform.position;
        //Use overlapshere to check for nearby colliders
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            //Add force to nearby rigidbodies
            if (rb != null)
                rb.AddExplosionForce(power * 5, explosionPos, radius, 3.0F);

            if (hit.GetComponent<PlayerHitbox>() != null)
            {
                //Debug.Log("Fireball Hit Player");
                GameObject player = hit.GetComponent<PlayerHitbox>().player.gameObject;
                float playerDistance = Vector3.Distance(hit.transform.position, transform.position);

                int playerHitID = player.GetComponent<PlayerProperties>().playerRewiredID;


                if (!player.GetComponent<PlayerProperties>().isDead)
                {
                    if (playerDistance < radius)
                    {
                        float calculatedDamage = damage * (1 - (playerDistance / radius));
                        //Debug.Log("Damage= " + damage + " playerDistance= " + playerDistance + " radius= " + radius);
                        player.GetComponent<PlayerProperties>().Damage((int)calculatedDamage, false, 99);

                    }
                }
            }
            if (hit.GetComponent<AIHitbox>() != null)
            {
                //Debug.Log("Hit AI");
                GameObject ai;
                ai = hit.GetComponent<AIHitbox>().aiGO;
                float aiDistance = Vector3.Distance(hit.transform.position, transform.position);

                bool aiAlreadyHit = false;

                for (int i = 0; i < AIsHit.Length; i++)
                {
                    if (AIsHit[i] != null)
                    {
                        if (ai == AIsHit[i])
                        {
                            aiAlreadyHit = true;
                        }
                    }
                }

                bool assignedAIInArray = false;

                if (!aiAlreadyHit)
                {
                    for (int i = 0; i < AIsHit.Length; i++)
                    {
                        if (AIsHit[i] == null && !assignedAIInArray)
                        {
                            AIsHit[i] = ai;
                            assignedAIInArray = true;
                        }
                    }
                }

                if (!aiAlreadyHit)
                {
                    if (hit.GetComponent<AIHitbox>().aiHealth > 0)
                    {
                        float calculatedDamage = damage * (1 - (aiDistance / radius));
                        hit.GetComponent<AIHitbox>().DamageAI(false, calculatedDamage, playerWhoThrewGrenade);
                    }

                }
            }
        }

        //Destroy the grenade object on explosion
        Destroy(gameObject);
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(deSpawnTime);

        if (gameObject)
            Destroy(gameObject);
    }
}
