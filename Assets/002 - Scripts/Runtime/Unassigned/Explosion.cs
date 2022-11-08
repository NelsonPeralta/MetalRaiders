using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public delegate void ExplosionEvent(Explosion explosion);
    public ExplosionEvent OnObjectAdded;

    public Player player { get { return _player; } set { _player = value; } }

    [SerializeField] Player _player;

    [Header("Settings")]
    public float damage; // Determined in Weapon Properties Script
    public float radius;
    public float explosionPower;

    List<GameObject> objectsHit = new List<GameObject>();


    private void Start()
    {
        Explode();
    }

    void Explode()
    {
        //Use overlapshere to check for nearby colliders
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);

        for (int i = 0; i < colliders.Length; i++) // foreach(Collider hit in colliders)
        {
            Collider hit = colliders[i];
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            CharacterController cc = hit.GetComponent<CharacterController>();

            int characterControllerDivider = 3;
            float hitDistance = Vector3.Distance(transform.position, hit.transform.position);
            float calculatedPower = (explosionPower * (1 - (hitDistance / radius)));

            //Add force to nearby rigidbodies
            if (rb != null)
                rb.AddExplosionForce(calculatedPower, explosionPos, radius, 3.0F);

            if (cc)
            {
                Vector3 exDir = (cc.transform.position - this.transform.position).normalized;
                cc.GetComponent<PlayerImpactReceiver>().AddImpact(exDir, calculatedPower / characterControllerDivider);
            }

            if (hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<PlayerHitbox>().player.isDead && !hit.GetComponent<PlayerHitbox>().player.isRespawning)
            {
                GameObject playerHit = hit.GetComponent<PlayerHitbox>().player.gameObject;
                if (!objectsHit.Contains(playerHit))
                {
                    objectsHit.Add(playerHit);
                    OnObjectAdded?.Invoke(this);
                    float playerDistance = Vector3.Distance(hit.transform.position, transform.position);
                    float calculatedDamage = damage * (1 - (playerDistance / radius));
                    try
                    {
                        hit.GetComponent<PlayerHitbox>().Damage((int)calculatedDamage, false, player.pid);
                    }
                    catch { hit.GetComponent<PlayerHitbox>().Damage((int)calculatedDamage); }
                }
            }
            else if (hit.GetComponent<IDamageable>() != null)
            {
                GameObject hitObject = hit.gameObject;
                if (!objectsHit.Contains(hitObject))
                {
                    objectsHit.Add(hitObject);
                    OnObjectAdded?.Invoke(this);
                    float playerDistance = Vector3.Distance(hit.transform.position, transform.position);
                    int calculatedDamage = (int)(damage * (1 - (playerDistance / radius)));
                    try
                    {
                        hit.GetComponent<IDamageable>().Damage(calculatedDamage);
                    }
                    catch { }
                }
            }
        }
        Destroy(gameObject, 10);
    }

}
