using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AIGrenade : MonoBehaviour
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
    public AudioSource impactSound;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 22 && !hasHitObject) // Non-Interactable Layer
        {
            StartCoroutine(ExplosionCountdown());
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

        //Explosion force
        Vector3 explosionPos = transform.position;
        //Use overlapshere to check for nearby colliders
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        List<GameObject> objectsHit = new List<GameObject>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider hit = colliders[i];
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            //Add force to nearby rigidbodies
            if (rb != null && rb.gameObject.layer != gameObject.layer)
                rb.AddExplosionForce(power, explosionPos, radius, 3.0F);

            if (hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<PlayerHitbox>().player.isDead && !hit.GetComponent<PlayerHitbox>().player.isRespawning)
            {
                GameObject playerHit = hit.GetComponent<PlayerHitbox>().player.gameObject;
                if (!objectsHit.Contains(playerHit))
                {
                    objectsHit.Add(playerHit);
                    float playerDistance = Vector3.Distance(hit.transform.position, transform.position);
                    float calculatedDamage = damage * (1 - (playerDistance / radius));
                    //Debug.Log("Damage= " + calculatedDamage + " playerDistance= " + playerDistance + " radius= " + radius);
                    //player.GetComponent<PlayerProperties>().BleedthroughDamage(calculatedDamage, false, 99);
                    if (playerWhoThrewGrenade.GetComponent<PhotonView>().IsMine && calculatedDamage > 0)
                        playerHit.GetComponent<Player>().Damage((int)calculatedDamage, false, playerWhoThrewGrenade.GetComponent<PhotonView>().ViewID);
                }
            }
        }

        Destroy(gameObject);
    }
}
