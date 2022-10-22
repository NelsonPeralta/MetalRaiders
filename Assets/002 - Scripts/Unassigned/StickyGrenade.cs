using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyGrenade : MonoBehaviour
{
    [Header("Settings")]
    public float radius = 5.0F;
    public int damage = 100;
    public float power = 350.0F;
    public float grenadeTimer = 1.0f;
    public float throwForce = 625.0f;
    public Transform explosionPrefab;

    [Header("Background Info")]
    public Player player;
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

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log($"Sticky Grenade Collision: {collision.gameObject.name}");
        if (!explosionTimerStarted && !hasHitObject)
        {
            if (collision.gameObject.layer == 13)
            {
                if (collision.gameObject.GetComponent<PlayerHitbox>())
                {
                        gameObject.transform.parent = collision.gameObject.transform;

                        GetComponent<Rigidbody>().useGravity = false;
                        GetComponent<Rigidbody>().isKinematic = true;

                        stuckPlayerID = collision.gameObject.GetComponent<PlayerHitbox>().player.GetComponent<Player>().playerRewiredID;
                        Debug.Log("Stuck Player");

                        playerStuck = true; // Without this line player stuck is always 0 so player 0 always die even when not stuck
                        hasHitObject = true;
                        explosionTimer = grenadeTimer;
                        explosionTimerStarted = true;
                        PlaySound(impactSound);
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
                if (collision.gameObject.GetComponent<PlayerController>() == null
                    && collision.gameObject.layer != 22 && collision.transform.root.gameObject != player.gameObject)
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
                cc.GetComponent<PlayerImpactReceiver>().AddImpact(player, exDir, power * 5);
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
                        playerHit.GetComponent<Player>().Damage((int)calculatedDamage, false, player.PV.ViewID, damageSource: "stickygrenade");
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
                        hitbox.aiAbstractClass.Damage((int)calculatedDamage, player.PV.ViewID, damageSource: "stickygrenade");
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
