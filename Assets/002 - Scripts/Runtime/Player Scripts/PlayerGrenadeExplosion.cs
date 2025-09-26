using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrenadeExplosion : MonoBehaviour
{
    [Header("Settings")]
    public AudioClip explosionClip;
    public float radius;
    public int damage;
    public float power;
    public float throwForce;
    public int despawnTime = 3;
    public Player playerWhoThrewGrenade;
    public GameObject grenadeObject;
    void Start()
    {
        Destroy(gameObject, despawnTime);
    }

    public void Explode()
    {

        if (grenadeObject)
            Destroy(grenadeObject);

        GetComponent<AudioSource>().clip = explosionClip;
        GetComponent<AudioSource>().Play();

        if (playerWhoThrewGrenade.PV.IsMine)
        {
            //Explosion force
            Vector3 explosionPos = transform.position;
            //Use overlapshere to check for nearby colliders
            Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
            List<GameObject> objectsHit = new List<GameObject>();
            for (int i = 0; i < colliders.Length; i++) // foreach(Collider hit in colliders)
            {
                Collider hit = colliders[i];
                Rigidbody rb = hit.GetComponent<Rigidbody>();

                //Add force to nearby rigidbodies
                if (rb != null)
                    rb.AddExplosionForce(power * 5, explosionPos, radius, 3.0F);

                if (hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<PlayerHitbox>().player.isDead && !hit.GetComponent<PlayerHitbox>().player.isRespawning)
                {
                    GameObject playerHit = hit.GetComponent<PlayerHitbox>().player.gameObject;
                    if (!objectsHit.Contains(playerHit))
                    {
                        objectsHit.Add(playerHit);
                        float playerDistance = Vector3.Distance(hit.transform.position, transform.position);
                        float calculatedDamage = damage * (1 - (playerDistance / radius));
                        Log.Print(() =>"Damage= " + calculatedDamage + " playerDistance= " + playerDistance + " radius= " + radius);
                        //player.GetComponent<PlayerProperties>().BleedthroughDamage(calculatedDamage, false, 99);
                        if (playerWhoThrewGrenade.PV.IsMine && calculatedDamage > 0)
                            playerHit.GetComponent<Player>().Damage((int)calculatedDamage, false, playerWhoThrewGrenade.PV.ViewID);
                    }
                }
                if (hit.GetComponent<AIHitbox>() && !hit.GetComponent<AIHitbox>().aiAbstractClass.isDead)
                {
                    GameObject aiHit = hit.GetComponent<AIHitbox>().aiAbstractClass.gameObject;
                    if (!objectsHit.Contains(aiHit))
                    {
                        objectsHit.Add(aiHit);
                        Log.Print(() =>"Hit AI");
                        AIHitbox hitbox = hit.GetComponent<AIHitbox>();
                        float aiDistance = Vector3.Distance(hit.transform.position, transform.position);
                        float calculatedDamage = damage * (1 - (aiDistance / radius));
                        Log.Print(() =>$"Rocket Damage on AI: {calculatedDamage}");
                        if (playerWhoThrewGrenade.PV.IsMine && calculatedDamage > 0)
                            hitbox.aiAbstractClass.Damage((int)calculatedDamage, playerWhoThrewGrenade.PV.ViewID);
                    }
                }
            }
        }
    }
}
