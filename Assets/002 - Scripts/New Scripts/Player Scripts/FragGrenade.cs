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
        PlaySound(throwSound);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 22 && !hasHitObject) // Non-Interactable Layer
        {
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
                        hit.GetComponent<AIHitbox>().UpdateAIHealth(true, calculatedDamage, playerWhoThrewGrenade);
                    }
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
