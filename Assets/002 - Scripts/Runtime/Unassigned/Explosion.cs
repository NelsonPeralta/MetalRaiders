using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public delegate void ExplosionEvent(Explosion explosion);
    public ExplosionEvent OnObjectAdded;

    public Player player { get { return _player; } set { _player = value; } }
    public bool stuck { get { return _stuck; } set { _stuck = value; } }

    [SerializeField] Player _player;
    [SerializeField] string damageSource;

    [Header("Settings")]
    public float damage; // Determined in Weapon Properties Script
    public float radius;
    public float explosionPower;

    List<GameObject> objectsHit = new List<GameObject>();
    bool _stuck;

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
            float hitDistance = Vector3.Distance(transform.position, hit.transform.position);

            if (hitDistance > radius)
                continue;


            Rigidbody rb = hit.GetComponent<Rigidbody>();
            CharacterController cc = hit.GetComponent<CharacterController>();


            float disRatio = 1 - (hitDistance / radius);
            float calculatedPower = explosionPower * disRatio;
            int calculatedDamage = (int)(damage * disRatio);

            int characterControllerDivider = 3;

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

                    try
                    {
                        if (stuck)
                            damageSource = "Stuck";
                        if (player.isMine)
                            hit.GetComponent<PlayerHitbox>().Damage((int)calculatedDamage, false, player.pid, damageSource: this.damageSource);
                    }
                    catch { if (hit.GetComponent<PlayerHitbox>().player.isMine) hit.GetComponent<PlayerHitbox>().Damage((int)calculatedDamage); }
                }
            }
            else if (hit.GetComponent<IDamageable>() != null)
            {
                if (hit.GetComponent<ActorHitbox>())
                    if (objectsHit.Contains(hit.GetComponent<ActorHitbox>().actor.gameObject))
                        return;
                    else
                        objectsHit.Add(hit.GetComponent<ActorHitbox>().actor.gameObject);
                else;

                GameObject hitObject = hit.gameObject;

                if (!objectsHit.Contains(hitObject))
                {
                    Debug.Log(hitObject.name);
                    Debug.Log(calculatedDamage);

                    objectsHit.Add(hitObject);
                    OnObjectAdded?.Invoke(this);
                    try
                    {
                        hit.GetComponent<IDamageable>().Damage(calculatedDamage, false, player.pid);
                    }
                    catch (System.Exception e) { Debug.LogWarning(e); hit.GetComponent<IDamageable>().Damage(calculatedDamage); }
                }
            }
        }
        Destroy(gameObject, 10);
    }

}
