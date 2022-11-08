using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rocket : MonoBehaviour
{
    public Player player;

    [Header("Settings")]
    public float damage; // Determined in Weapon Properties Script
    public float force;
    public float radius;
    public float power;

    [Header("Rocket OR Grenade")]
    public bool useConstantForce;

    [Header("Prefabs")]
    public Transform explosionPrefab;
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
        if (collision.gameObject.layer != 9)
            Explosion();
    }



    void Explosion()
    {
        if (!useConstantForce)
            Instantiate(explosionPrefab, transform.position + new Vector3(0, 0.5f, 0), transform.rotation);
        else
            Instantiate(explosionPrefab, transform.position, transform.rotation);

        //Explosion force
        Vector3 explosionPos = transform.position;
        //Use overlapshere to check for nearby colliders
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        string hitsDebugMessage = "Rocket Hits: ";

        List<GameObject> objectsHit = new List<GameObject>();
        for (int i = 0; i < colliders.Length; i++) // foreach(Collider hit in colliders)
        {
            Collider hit = colliders[i];
            hitsDebugMessage += $"({i}) {hit.name}, ";
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            CharacterController cc = hit.GetComponent<CharacterController>();

            float hitDistance = Vector3.Distance(transform.position, hit.transform.position);
            float calculatedPower = (power * (1 - (hitDistance / radius)));

            //Add force to nearby rigidbodies
            if (rb != null)
                rb.AddExplosionForce(calculatedPower, explosionPos, radius, 3.0F);

            if (cc)
            {
                Vector3 exDir = (cc.transform.position - this.transform.position).normalized;
                cc.GetComponent<PlayerImpactReceiver>().AddImpact(player, exDir, calculatedPower);
            }

            if (hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<PlayerHitbox>().player.isDead && !hit.GetComponent<PlayerHitbox>().player.isRespawning)
            {
                GameObject playerHit = hit.GetComponent<PlayerHitbox>().player.gameObject;
                if (!objectsHit.Contains(playerHit))
                {
                    objectsHit.Add(playerHit);
                    float playerDistance = Vector3.Distance(hit.transform.position, transform.position);
                    float calculatedDamage = damage * (1 - (playerDistance / radius));
                    Debug.Log("Damage= " + calculatedDamage + " playerDistance= " + playerDistance + " radius= " + radius);
                    //player.GetComponent<PlayerProperties>().BleedthroughDamage(calculatedDamage, false, 99);
                    if (player.PV.IsMine && calculatedDamage > 0)
                        playerHit.GetComponent<Player>().Damage((int)calculatedDamage, false, player.PV.ViewID, damageSource: "rpg");
                }
            }
            if (hit.GetComponent<AIHitbox>() && !hit.GetComponent<AIHitbox>().aiAbstractClass.isDead)
            {
                GameObject aiHit = hit.GetComponent<AIHitbox>().aiAbstractClass.gameObject;
                if (!objectsHit.Contains(aiHit))
                {
                    objectsHit.Add(aiHit);
                    Debug.Log("Hit AI");
                    AIHitbox hitbox = hit.GetComponent<AIHitbox>();
                    float aiDistance = Vector3.Distance(hit.transform.position, transform.position);
                    float calculatedDamage = damage * (1 - (aiDistance / radius));
                    Debug.Log($"Rocket Damage on AI: {calculatedDamage}");
                    if (player.PV.IsMine && calculatedDamage > 0)
                        hitbox.aiAbstractClass.Damage((int)calculatedDamage, player.PV.ViewID, damageSource: "rpg");
                }
            }
        }
        Debug.Log(hitsDebugMessage);
        //Destroy the grenade object on explosion
        Destroy(gameObject);
    }


}
