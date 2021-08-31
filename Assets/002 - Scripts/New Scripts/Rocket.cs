using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour
{
    public PlayerProperties playerWhoThrewGrenade;

    [Header("Settings")]
    public float damage; // Determined in Weapon Properties Script
    public float force;
    public float radius;
    public float power;

    [Header("Rocket OR Grenade")]
    public bool useConstantForce;

    [Header("Prefabs")]
    public Transform explosionPrefab;    

    GameObject[] playersHit = new GameObject[4];
    GameObject[] AIsHit = new GameObject[20];

    private void Start()
    {
        //If not using constant force (grenade launcher projectile)
        if (!useConstantForce)
        {
            //Launch the projectile forward by adding force to it at start
            GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * force);
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

        //BulletBehaviour();

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with rocket: " + collision.gameObject.name);
        if(collision.gameObject.layer != 9)
            Explosion();
    }



    void Explosion()
    {
        if(!useConstantForce)
            Instantiate(explosionPrefab, transform.position + new Vector3(0, 0.5f, 0), transform.rotation);
        else
            Instantiate(explosionPrefab, transform.position, transform.rotation);

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
                GameObject player = hit.GetComponent<PlayerHitbox>().player;
                float playerDistance = Vector3.Distance(hit.transform.position, transform.position);

                int playerHitID = player.GetComponent<PlayerProperties>().playerRewiredID;

                bool playerAlreadyHit = false;

                for (int i = 0; i < playersHit.Length; i++)
                {
                    if (playersHit[i] != null)
                    {
                        if (playersHit[i] == player)
                        {
                            //Debug.Log("Here");
                            playerAlreadyHit = true;
                        }
                        else
                        {
                            Debug.Log("Not yet hit");
                        }
                    }
                }

                bool assignedPlayerInArray = false;

                if (!playerAlreadyHit)
                {
                    for (int i = 0; i < playersHit.Length; i++)
                    {
                        if (playersHit[i] == null && !assignedPlayerInArray)
                        {
                            playersHit[i] = player;
                            assignedPlayerInArray = true;
                        }
                    }
                }

                if (!playerAlreadyHit)
                {
                    if (!player.GetComponent<PlayerProperties>().isDead)
                    {
                        float calculatedDamage = damage * (1 - (playerDistance / radius));
                        Debug.Log("Damage= " + damage + " playerDistance= " + playerDistance + " radius= " + radius);
                        //player.GetComponent<PlayerProperties>().BleedthroughDamage(calculatedDamage, false, 99);
                        if (playerWhoThrewGrenade.PV.IsMine)
                            player.GetComponent<PlayerProperties>().Damage((int)calculatedDamage, false, playerWhoThrewGrenade.PV.ViewID);
                    }
                }
            }
            if (hit.GetComponent<AIHitbox>() != null)
            {
                Debug.Log("Hit AI");
                GameObject ai;
                ai = hit.GetComponent<AIHitbox>().aiGO;
                float aiDistance = Vector3.Distance(hit.transform.position, transform.position);
                    if (hit.GetComponent<AIHitbox>().aiHealth > 0)
                    {
                        float calculatedDamage = damage * (1 - (aiDistance / radius));
                        Debug.Log($"Rocket Damage on AI: {calculatedDamage}");
                        if (playerWhoThrewGrenade.PV.IsMine)
                            hit.GetComponent<AIHitbox>().DamageAI(true, calculatedDamage, playerWhoThrewGrenade.gameObject);
                    }
            }
        }

        //Destroy the grenade object on explosion
        Destroy(gameObject);
    }

    
}
