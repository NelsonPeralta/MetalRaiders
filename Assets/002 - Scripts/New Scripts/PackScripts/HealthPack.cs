using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public ChildManager cManager;
    PlayerProperties pProperties;
    public GameObject packFX;
    public GameObject motionTrackerIcon;

    [Header("Respawn Settings")]
    public SphereCollider sCollider;
    public float spawnTime = 180;
    public bool canRespawn = false;
    public bool spawnsAtStart = false;

    private void Start()
    {
        if (!spawnsAtStart)
        {
            StartCoroutine(Respawn(spawnTime));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            pProperties = other.gameObject.GetComponent<PlayerProperties>();

            if(pProperties.healthSlider.value < pProperties.maxHealth && pProperties.needsHealthPack)
            {
                StartCoroutine(AllowHealthRegeneration());
            }

            
        }
    }

    IEnumerator AllowHealthRegeneration()
    {
        pProperties.needsHealthPack = false;
        packFX.SetActive(false);

        if(canRespawn)
        {
            StartCoroutine(Respawn(spawnTime));
        }

        yield return new WaitForSeconds(5);

        pProperties.needsHealthPack = true;

        if (!canRespawn)
        {
            Destroy(this.gameObject);
        }
    }

    IEnumerator Respawn(float respawnTime)
    {
        sCollider.enabled = false;
        gameObject.layer = 23;

        foreach (GameObject child in cManager.allChildren)
        {
            child.SetActive(false);
        }

        yield return new WaitForSeconds(respawnTime);

        sCollider.enabled = true;
        gameObject.layer = 0;

        foreach (GameObject child in cManager.allChildren)
        {
            child.SetActive(true);
        }
    }
}
