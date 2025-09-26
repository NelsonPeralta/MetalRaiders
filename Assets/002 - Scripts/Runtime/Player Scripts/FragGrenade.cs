using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// DEPRECATED: Use class ExplosiveProjectile
/// </summary>

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
    List<IDamageable> hits = new List<IDamageable>();

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip throwSound;
    public AudioClip impactSound;
    public AudioClip explosionSound;

    Transform[] ignore;






    [SerializeField] float _velocityMagnitude;





    private void Start()
    {
        PlaySound(throwSound);

        //ignore = player.GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        try
        {
            _velocityMagnitude = GetComponent<Rigidbody>().linearVelocity.magnitude;

            if (hasHitObject)
                if (GetComponent<Rigidbody>().linearVelocity.magnitude != 5)
                {
                    Vector3 dir = GetComponent<Rigidbody>().linearVelocity;
                    dir.Normalize();
                    //GetComponent<Rigidbody>().velocity.Normalize();
                    GetComponent<Rigidbody>().linearVelocity = dir * 5;
                }
        }
        catch
        {

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 22 && !hasHitObject && !ignore.Contains(collision.transform)) // Non-Interactable Layer
        {
            hasHitObject = true;
            Log.Print(() =>$"Grenade Collided with: {collision.gameObject.name}");
            Log.Print(() =>GetComponent<Rigidbody>().linearVelocity.magnitude);
            StartCoroutine(ExplosionCountdown());
            PlaySound(impactSound);

            //GetComponent<Rigidbody>().velocity.Normalize();
            //GetComponent<Rigidbody>().velocity *= 0.5f;
        }
    }

    IEnumerator ExplosionCountdown()
    {
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
                    Player player = hit.GetComponent<PlayerHitbox>().player;
                    objectsHit.Add(playerHit);
                    float playerDistance = Vector3.Distance(hit.transform.position, transform.position);
                    float calculatedDamage = damage * (1 - (playerDistance / radius));
                    Log.Print(() =>"Damage= " + calculatedDamage + " playerDistance= " + playerDistance + " radius= " + radius);

                    //player.GetComponent<PlayerProperties>().BleedthroughDamage(calculatedDamage, false, 99);
                    if (this.player.PV.IsMine && calculatedDamage > 0)
                        playerHit.GetComponent<Player>().Damage((int)calculatedDamage, false, this.player.PV.ViewID, damageSourceCleanName : "fraggrenade");
                }
            }
            if (hit.GetComponent<AIHitbox>() && !hit.GetComponent<AIHitbox>().aiAbstractClass.isDead)
            {
                GameObject aiHit = hit.GetComponent<AIHitbox>().aiAbstractClass.gameObject;
                if (!objectsHit.Contains(aiHit))
                {
                    objectsHit.Add(aiHit);
                    AIHitbox hitbox = hit.GetComponent<AIHitbox>();
                    float aiDistance = Vector3.Distance(hit.transform.position, transform.position);
                    float calculatedDamage = damage * (1 - (aiDistance / radius));
                    Log.Print(() =>$"Frag grenade Damage on AI: {calculatedDamage}");
                    if (player.PV.IsMine && calculatedDamage > 0)
                        hitbox.aiAbstractClass.Damage((int)calculatedDamage, player.PV.ViewID, damageSource: "fraggrenade");
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
