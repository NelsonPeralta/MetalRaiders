using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParachuteWeaponDrop : MonoBehaviour
{
    public Animator lidAnim;
    public GameObject weaponSpawnPoint;
    public GameObject parachute;
    public GameObject parachuteDropped;

    [Header("Weapons")]
    public GameObject[] weapons;

    [Header("Sounds and VFX")]
    public AudioSource audioSource;
    public AudioClip fall;
    public AudioClip hitGorund;
    public AudioClip openCrate;
    public AudioClip closeCrate;
    public GameObject dirtImpact;

    public Vector3 spawnPoint;
    bool fallFinished;
    bool weaponSpawned;
    bool hasStartedAnimation;

    public float t = 0;

    private void Awake()
    {
        spawnPoint = transform.position;
        gameObject.SetActive(false);
    }

    public void spawnCrate()
    {
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        parachute.SetActive(true);
        parachuteDropped.SetActive(false);
        audioSource.clip = fall;
        audioSource.volume = 0.25f;
        audioSource.Play();
        transform.position = spawnPoint;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().isKinematic = false;
        fallFinished = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log($"Parachute collision: {other.gameObject.name}");
        if (!fallFinished)
        {
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<Rigidbody>().isKinematic = true;

            parachute.SetActive(false);
            parachuteDropped.SetActive(true);

            var dirt = Instantiate(dirtImpact, transform.position, transform.rotation);
            Destroy(dirt, 1);

            audioSource.clip = hitGorund;
            audioSource.volume = 1;
            audioSource.Play();

            StartCoroutine(PlayOpenSound());
            hasStartedAnimation = true;

            lidAnim.Play("Open Crate");
            fallFinished = true;
            spawnRandomWeapon();
        }
    }

    public void spawnRandomWeapon()
    {
            int randomAllWeapons = Random.Range(0, weapons.Length);

            var weapon = Instantiate(weapons[randomAllWeapons], weaponSpawnPoint.transform.position, weaponSpawnPoint.transform.rotation);
            string nameCorrection = weapon.name.Replace("(Clone)", ""); ; // For some reason I cant do it directly so I have to do it like this
            weapon.name = nameCorrection;
            Destroy(weapon, 60);
        StartCoroutine(DisableCrate());
    }

    IEnumerator PlayOpenSound()
    {
        yield return new WaitForSeconds(0.375f);
        audioSource.clip = openCrate;
        audioSource.Play();
    }

    IEnumerator DisableCrate()
    {
        yield return new WaitForSeconds(60);
        gameObject.SetActive(false);
    }

    /*
    private void LateUpdate()
    {
        if (buyRandomWeapon)
        {
            CheckPlayerInput();
        }

        if (fallFromSky && !fallFinished)
        {
            t += Time.deltaTime / timeToFall;
            transform.position = Vector3.Lerp(skySpawnPoint.transform.position, destination.transform.position, t);

            if (t >= 1)
            {
                if (lidAnim != null)
                {
                    lidAnim.Play("Open Crate");
                }

                fallFinished = true;
            }
        }

        if (fallFinished && !weaponSpawned)
        {
            spawnRandomWeapon();
            weaponSpawned = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasStartedAnimation)
        {
            if (fallFromSky)
            {
                var dirt = Instantiate(dirtImpact, transform.position, transform.rotation);
                Destroy(dirt, 1);

                audioSource.clip = hitGorund;
                audioSource.Play();

                StartCoroutine(PlayOpenSound());
                hasStartedAnimation = true;
            }
        }

        if (buyRandomWeapon && other.gameObject.GetComponent<PlayerProperties>() != null)
        {
            AddPlayerInArray(other.gameObject.GetComponent<PlayerProperties>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (buyRandomWeapon && other.gameObject.GetComponent<PlayerProperties>() != null)
        {
            RemovePlayerFromArray(other.gameObject.GetComponent<PlayerProperties>());
        }
    }

    public void spawnRandomWeapon()
    {
        if (includeSmallWeapons)
        {
            int randomSmallWeapon = Random.Range(0, smallWeapons.Length);

            var small = Instantiate(smallWeapons[randomSmallWeapon], weaponSpawnPoint.transform.position, weaponSpawnPoint.transform.rotation);
            string nameCorrection = small.name.Replace("(Clone)", ""); ; // For some reason I cant do it directly so I have to do it like this
            small.name = nameCorrection;
            Destroy(small, 30);
        }
        else if (includeHeavyWeapons)
        {
            int randomHeavyWeapon = Random.Range(0, heavyWeapons.Length);

            var heavy = Instantiate(heavyWeapons[randomHeavyWeapon], weaponSpawnPoint.transform.position, weaponSpawnPoint.transform.rotation);
            string nameCorrection = heavy.name.Replace("(Clone)", ""); ; // For some reason I cant do it directly so I have to do it like this
            heavy.name = nameCorrection;
            Destroy(heavy, 30);
        }
        else if (includePowerWeapons)
        {
            int randomPowerWeapon = Random.Range(0, powerWeapons.Length);

            var power = Instantiate(powerWeapons[randomPowerWeapon], weaponSpawnPoint.transform.position, weaponSpawnPoint.transform.rotation);
            string nameCorrection = power.name.Replace("(Clone)", ""); ; // For some reason I cant do it directly so I have to do it like this
            power.name = nameCorrection;
            Destroy(power, 30);
        }
        else if (includeAllWeapons)
        {
            int randomAllWeapons = Random.Range(0, allWeapons.Length);

            var weapon = Instantiate(allWeapons[randomAllWeapons], weaponSpawnPoint.transform.position, weaponSpawnPoint.transform.rotation);
            string nameCorrection = weapon.name.Replace("(Clone)", ""); ; // For some reason I cant do it directly so I have to do it like this
            weapon.name = nameCorrection;
            Destroy(weapon, timeToReset);
        }
    }

    IEnumerator PlayOpenSound()
    {
        yield return new WaitForSeconds(0.375f);
        audioSource.clip = openCrate;
        audioSource.Play();
    }

    void AddPlayerInArray(PlayerProperties player)
    {
        if (player.playerRewiredID == 0)
        {
            players[0] = player;

            UpdatePlayerUIOnEnter();
        }
        if (player.playerRewiredID == 1)
        {
            players[1] = player;

            UpdatePlayerUIOnEnter();
        }
        if (player.playerRewiredID == 2)
        {
            players[2] = player;

            UpdatePlayerUIOnEnter();
        }
        if (player.playerRewiredID == 3)
        {
            players[3] = player;

            UpdatePlayerUIOnEnter();
        }
    }

    void RemovePlayerFromArray(PlayerProperties player)
    {
        if (player.playerRewiredID == 0)
        {
            players[0].InformerText.text = "";
            players[0] = null;
        }
        if (player.playerRewiredID == 1)
        {
            players[1].InformerText.text = "";
            players[1] = null;
        }
        if (player.playerRewiredID == 2)
        {
            players[2].InformerText.text = "";
            players[2] = null;
        }
        if (player.playerRewiredID == 3)
        {
            players[3].InformerText.text = "";
            players[3] = null;
        }
    }

    void CheckPlayerInput()
    {
        if (players[0] != null && players[0].pController.player.GetButtonShortPressDown("Reload"))
        {
            StartCoroutine(OpenCrateAndClose());
        }
    }

    IEnumerator OpenCrateAndClose()
    {
        if (lidAnim != null && timeReseted)
        {
            lidAnim.Play("Open Crate");
            audioSource.clip = buyingSound;
            audioSource.Play();
            timeReseted = false;
            spawnRandomWeapon();
        }

        yield return new WaitForSeconds(timeToReset);

        if (lidAnim != null)
        {
            ResetPlayerWeaponInPickupCollider();
            UpdatePlayerUIOnEnter();
            lidAnim.Play("Close Crate");
            audioSource.clip = closeCrate;
            audioSource.Play();
            timeReseted = true;
        }
    }

    void ResetPlayerWeaponInPickupCollider()
    {
        if (players[0] != null)
        {
            if (players[0].wPickup != null)
            {
                players[0].wPickup.weaponCollidingWith = null;
                players[0].wPickup.weaponCollidingWithInInventory = null;
            }
        }
    }

    void UpdatePlayerUIOnEnter()
    {
        if (players[0] != null)
        {
            if (players[0].gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && players[0].gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
            {
                players[0].InformerText.text = "Buy RANDOM WEAPON for: " + cost.ToString() + " Points";
            }
            else
            {
                players[0].InformerText.text = "Not enough Points (" + cost.ToString() + ")";
            }
        }
        if (players[1] != null)
        {
            if (players[1].gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && players[1].gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
            {
                players[1].InformerText.text = "Buy RANDOM WEAPON for: " + cost.ToString() + " Points";
            }
            else
            {
                players[1].InformerText.text = "Not enough Points (" + cost.ToString() + ")";
            }
        }
        if (players[2] != null)
        {
            if (players[2].gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && players[2].gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
            {
                players[2].InformerText.text = "Buy RANDOM WEAPON for: " + cost.ToString() + " Points";
            }
            else
            {
                players[2].InformerText.text = "Not enough Points (" + cost.ToString() + ")";
            }
        }
        if (players[3] != null)
        {
            if (players[3].gameObject.GetComponent<PlayerPoints>().swarmPoints >= cost && players[3].gameObject.GetComponent<PlayerPoints>().swarmPoints > 0)
            {
                players[3].InformerText.text = "Buy RANDOM WEAPON for: " + cost.ToString() + " Points";
            }
            else
            {
                players[3].InformerText.text = "Not enough Points (" + cost.ToString() + ")";
            }
        }
    }
    */
}
