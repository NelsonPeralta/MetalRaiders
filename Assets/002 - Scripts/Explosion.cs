using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public int damage;
    float radius;
    int power;

    // Start is called before the first frame update
    void Start()
    {
        Explode();
    }
        
    void Explode()
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

            CharacterController cc = hit.GetComponent<CharacterController>();
            if (cc)
            {
                Vector3 exDir = (cc.transform.position - this.transform.position).normalized;
                cc.GetComponent<PlayerImpactReceiver>().AddImpact(null, exDir, power * 5);
            }

            if (hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<PlayerHitbox>().player.isDead && !hit.GetComponent<PlayerHitbox>().player.isRespawning)
            {
                GameObject playerHit = hit.GetComponent<PlayerHitbox>().player.gameObject;
                if (!objectsHit.Contains(playerHit))
                {
                    Player player = hit.GetComponent<PlayerHitbox>().player;
                    objectsHit.Add(playerHit);
                    float playerDistance = Vector3.Distance(hit.transform.position, transform.position);
                    float calculatedDamage = damage * (1 - (playerDistance / radius));
                    Debug.Log("Damage= " + calculatedDamage + " playerDistance= " + playerDistance + " radius= " + radius);

                    //if (this.player.PV.IsMine && calculatedDamage > 0)
                    //    playerHit.GetComponent<Player>().Damage((int)calculatedDamage, false, this.player.PV.ViewID, damageSource: "fraggrenade");
                }
            }
        }

        Destroy(gameObject);
    }
}
