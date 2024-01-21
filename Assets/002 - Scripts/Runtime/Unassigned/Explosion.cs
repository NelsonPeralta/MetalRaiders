using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public delegate void ExplosionEvent(Explosion explosion);
    public ExplosionEvent OnObjectAdded;

    public Player player { get { return _player; } set { Debug.Log($"SETTING EXPLOSION PLAYER {value.name}"); _player = value; } }
    public bool stuck { get { return _stuck; } set { _stuck = value; } }

    public string damageSource { get { return _damageSource; } set { _damageSource = value; } }

    [SerializeField] Player _player;
    [SerializeField] string _damageSource;

    [Header("Settings")]
    public float damage; // Determined in Weapon Properties Script
    public float radius;
    public float explosionPower;

    [SerializeField] LayerMask _obsLayerMask;

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
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius).Distinct().ToArray();

        for (int i = 0; i < colliders.Length; i++) // foreach(Collider hit in colliders)
        {
            Collider col = colliders[i];
            float hitDistance = Vector3.Distance(transform.position, col.transform.position);

            if (hitDistance > radius)
                continue;

            Debug.Log($"EXPLOSION {col.name}");

            // Check if the is an obstruction between explosion and the hit
            if (!col.name.Contains("llectible"))
            {
                Transform or = transform;

                Vector3 directionToTarget = (col.transform.position - or.position).normalized;
                RaycastHit hit;
                if (Physics.Raycast(or.position, directionToTarget, out hit, hitDistance, _obsLayerMask))
                {
                    //Debug.Log($"EXPLOSION. Hit {col.name} but colliding with {hit.collider.name}");
                    if (hit.collider != col) // Checks if the object obstructing is not the hit itself if it happens to be of layer Default
                        continue;
                }
            }


            Rigidbody rb = col.GetComponent<Rigidbody>();


            float disRatio = 1 - (hitDistance / radius);
            float calculatedPower = explosionPower * disRatio;
            int calculatedDamage = (int)(damage * disRatio);

            int characterControllerDivider = 3;

            //Add force to nearby rigidbodies
            if (rb != null)
                rb.AddExplosionForce(calculatedPower, explosionPos, radius, 3.0F);


            if (col.GetComponent<PlayerHitbox>() && !col.GetComponent<PlayerHitbox>().player.isDead && !col.GetComponent<PlayerHitbox>().player.isRespawning)
            {
                GameObject playerHit = col.GetComponent<PlayerHitbox>().player.gameObject;
                if (!objectsHit.Contains(playerHit))
                {
                    objectsHit.Add(playerHit);
                    OnObjectAdded?.Invoke(this);

                    try
                    {
                        Debug.Log("EXPLOSION DAMAGING PLAYER");
                        if (stuck)
                            _damageSource = "Stuck";
                        if (player.isMine)
                            col.GetComponent<PlayerHitbox>().Damage((int)calculatedDamage, false, player.photonId, damageSource: this._damageSource, impactDir: (col.transform.position - transform.position));
                    }
                    catch { if (col.GetComponent<PlayerHitbox>().player.isMine) col.GetComponent<PlayerHitbox>().Damage((int)calculatedDamage); }
                }
            }
            else if (col.GetComponent<IDamageable>() != null)
            {
                if (col.GetComponent<ActorHitbox>())
                    if (objectsHit.Contains(col.GetComponent<ActorHitbox>().actor.gameObject))
                        return;
                    else
                        objectsHit.Add(col.GetComponent<ActorHitbox>().actor.gameObject);
                else;

                GameObject hitObject = col.gameObject;

                if (!objectsHit.Contains(hitObject))
                {
                    Debug.Log(hitObject.name);
                    //Debug.Log(calculatedDamage);

                    objectsHit.Add(hitObject);
                    OnObjectAdded?.Invoke(this);
                    try
                    {
                        Debug.Log(calculatedDamage);
                        Debug.Log(player.photonId);
                        Debug.Log(col.transform.position - transform.position);
                        hitObject.GetComponent<IDamageable>().Damage(calculatedDamage, false, player.photonId, impactDir: (col.transform.position - transform.position));
                    }
                    catch (System.Exception e) { Debug.LogWarning(e + " " + hitObject.name); }
                }
            }


        }
        //Destroy(gameObject, 10);
    }

    public void DisableIn5Seconds()
    {
        StartCoroutine(DIsableIn5Seconds_Coroutine());
    }

    IEnumerator DIsableIn5Seconds_Coroutine()
    {
        yield return new WaitForSeconds(5);

        gameObject.SetActive(false);
    }
}
