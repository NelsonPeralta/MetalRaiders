using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStickyGrenade : MonoBehaviour
{
    [Header("Settings")]
    public float radius = 5.0F;
    public int damage = 100;
    public float power = 350.0F;
    public float grenadeTimer = 1.0f;
    public float throwForce = 625.0f;
    public Transform explosionPrefab;

    [Header("Background Info")]
    public GameObject playerWhoThrewGrenade;
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
        /*
        //Random rotation of the grenade
        GetComponent<Rigidbody>().AddRelativeTorque
           (Random.Range(500, 1500), //X Axis
            Random.Range(0, 0),          //Y Axis
            Random.Range(0, 0)           //Z Axis
            * Time.deltaTime * 5000);
            */
        //Launch the projectile forward by adding force to it at start
        //GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * throwForce);


    }


    private void OnTriggerEnter(Collider collision)
    {
        if (!explosionTimerStarted && !hasHitObject)
        {
            if (collision.gameObject.layer == 13)
            {
                if (collision.gameObject.GetComponent<PlayerHitbox>())
                {
                    if (collision.gameObject.GetComponent<PlayerHitbox>().player.GetComponent<PlayerProperties>().playerRewiredID != playerRewiredID)
                    {
                        gameObject.transform.parent = collision.gameObject.transform;

                        GetComponent<Rigidbody>().useGravity = false;
                        GetComponent<Rigidbody>().isKinematic = true;

                        stuckPlayerID = collision.gameObject.GetComponent<PlayerHitbox>().player.GetComponent<PlayerProperties>().playerRewiredID;
                        Debug.Log("Stuck Player");

                        playerStuck = true; // Without this line player stuck is always 0 so player 0 always die even when not stuck
                        hasHitObject = true;
                        explosionTimer = grenadeTimer;
                        explosionTimerStarted = true;
                        PlaySound(impactSound);
                    }
                }
                else if (collision.gameObject.GetComponent<AIHitbox>())
                {
                    Debug.Log("Here!!!!");
                    gameObject.transform.parent = collision.gameObject.transform;

                    GetComponent<Rigidbody>().useGravity = false;
                    GetComponent<Rigidbody>().isKinematic = true;
                    //playerStuck = true; // Without this line player stuck is always 0 so player 0 always die even when not stuck
                    hasHitObject = true;
                    explosionTimer = grenadeTimer;
                    explosionTimerStarted = true;
                    PlaySound(impactSound);
                }
            }
            else
            {
                if (collision.gameObject.transform.root.GetComponent<PlayerController>() == null
                    && collision.gameObject.layer != 22)
                {
                    Debug.Log("Collision = " + collision.gameObject.name + " GO Layer: " + collision.gameObject.layer
                        + " Root GO : " + collision.gameObject.transform.root.gameObject.name);
                    gameObject.transform.parent = collision.gameObject.transform;

                    GetComponent<Rigidbody>().useGravity = false;
                    GetComponent<Rigidbody>().isKinematic = true;

                    hasHitObject = true;
                    explosionTimer = grenadeTimer;
                    explosionTimerStarted = true;
                    PlaySound(impactSound);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (explosionTimerStarted)
        {
            ExplosionCountdown();
        }
    }

    void ExplosionCountdown()
    {
        if (explosionTimerStarted)
        {
            explosionTimer -= Time.deltaTime;
        }

        if (explosionTimer <= 0 && !hasExploded)
        {
            Explosion();
            hasExploded = true;
            Destroy(gameObject);
        }
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
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            //Add force to nearby rigidbodies
            if (rb != null)
                rb.AddExplosionForce(power * 5, explosionPos, radius, 3.0F);

            if (hit.GetComponent<PlayerHitbox>() != null)
            {
                GameObject player;
                player = hit.GetComponent<PlayerHitbox>().player;
                float playerDistance = Vector3.Distance(hit.transform.position, transform.position);

                int playerHitID = player.GetComponent<PlayerProperties>().playerRewiredID;

                bool playerAlreadyHit = false;

                for (int i = 0; i < playersHit.Length; i++)
                {
                    if (playersHit[i] != null)
                    {
                        if (playerHitID == playersHit[i].GetComponent<PlayerProperties>().playerRewiredID)
                        {
                            playerAlreadyHit = true;
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
                        player.GetComponent<PlayerProperties>().BleedthroughDamage(calculatedDamage, false, 99);
                    }
                }
            }

            if (hit.GetComponent<AIHitbox>() != null)
            {
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
                        Debug.Log(hit.GetComponent<AIHitbox>().aiGO.name);
                        Debug.Log(calculatedDamage);
                        Debug.Log(aiDistance);
                        hit.GetComponent<AIHitbox>().UpdateAIHealth(false, calculatedDamage, playerWhoThrewGrenade);
                    }
                }
            }
        }
    }

    void PlaySound(AudioClip ac)
    {
        audioSource.clip = ac;
        audioSource.Play();
    }
}
