using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragGrenade : MonoBehaviour
{
    [Header("Settings")]
    public float radius = 5.0F;
    public int damage = 100;
    public float power = 350.0F;
    public float grenadeTimer = 1.0f;
    public float throwForce = 625.0f;
    public Transform explosionPrefab;
    float velocityOnCollision = 5f;

    [Header("Background Info")]
    public PlayerProperties playerWhoThrewGrenade;
    public int playerRewiredID;
    public string team;
    bool hasHitObject;
    bool playerStuck;
    bool explosionTimerStarted = false;
    bool hasExploded;
    int stuckPlayerID;
    float explosionTimer;
    GameObject[] playersHit = new GameObject[4];
    GameObject[] AIsHit = new GameObject[20];

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip throwSound;
    public AudioClip impactSound;
    public AudioClip explosionSound;

    private void Start()
    {
        PlaySound(throwSound);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 22 && !hasHitObject) // Non-Interactable Layer
        {
            Debug.Log($"Grenade Collided with: {collision.gameObject.name}");
            Debug.Log(GetComponent<Rigidbody>().velocity);
            StartCoroutine(ExplosionCountdown());
            PlaySound(impactSound);
        }
    }

    IEnumerator ExplosionCountdown()
    {
        hasHitObject = true;
        yield return new WaitForSeconds(grenadeTimer);        
        Explosion();
    }

    void Explosion()
    {
        var explosionGO = Instantiate(explosionPrefab, transform.position, transform.rotation);
        Destroy(explosionGO.gameObject, 3f);
        explosionGO.GetComponent<AudioSource>().clip = explosionSound;
        explosionGO.GetComponent<AudioSource>().Play();

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
                    Debug.Log("Damage= " + calculatedDamage + " playerDistance= " + playerDistance + " radius= " + radius);
                    //player.GetComponent<PlayerProperties>().BleedthroughDamage(calculatedDamage, false, 99);
                    if (playerWhoThrewGrenade.PV.IsMine && calculatedDamage > 0)
                        playerHit.GetComponent<PlayerProperties>().Damage((int)calculatedDamage, false, playerWhoThrewGrenade.PV.ViewID);
                }
            }
            if (hit.GetComponent<AIHitbox>() && !hit.GetComponent<AIHitbox>().aiAbstractClass.IsDead())
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
                    if (playerWhoThrewGrenade.PV.IsMine && calculatedDamage > 0)
                        hitbox.aiAbstractClass.Damage((int)calculatedDamage, playerWhoThrewGrenade.PV.ViewID);
                }
            }
        }

        Destroy(gameObject);
    }

    void PlaySound(AudioClip ac)
    {
        audioSource.clip = ac;
        audioSource.Play();
    }
}
