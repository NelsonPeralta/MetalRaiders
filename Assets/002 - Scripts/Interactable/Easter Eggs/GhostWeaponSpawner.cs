using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostWeaponSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float timeToSpawn;
    public bool spawnAtStart;
    public GameObject[] weapons;
    public GameObject weaponPlaceHolder;
    Vector3 placeHolderPosition;
    Quaternion placeHolderQuat;
    bool DespawningCharacter;

    [Header("Models")]
    public GameObject character;
    public GameObject disappearEffect;
    
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip appearClip;
    public AudioClip disappearClip;

    private void Start()
    {
        if (weaponPlaceHolder)
        {
            placeHolderPosition = weaponPlaceHolder.transform.position;
            placeHolderQuat = weaponPlaceHolder.transform.rotation;
        }
        if (!spawnAtStart)
        {
            if (weaponPlaceHolder)
                weaponPlaceHolder.gameObject.SetActive(false);
            if (character)
                character.SetActive(false);
            StartCoroutine(DespawnCharacter());
        }
    }

    IEnumerator DespawnCharacter()
    {
        DespawningCharacter = true;
        yield return new WaitForSeconds(0.75f);
        if (character.activeSelf && disappearEffect)
        {
            var eff = Instantiate(disappearEffect, character.transform.position, character.transform.rotation);
            Destroy(eff, 2);
            audioSource.clip = disappearClip;
            audioSource.Play();
        }
        if (character)
            character.SetActive(false);
        
        StartCoroutine(RespawnWeapon());
    }

    IEnumerator RespawnWeapon()
    {
        yield return new WaitForSeconds(timeToSpawn);
        if (!character.activeSelf && disappearEffect)
        {
            var eff = Instantiate(disappearEffect, character.transform.position, character.transform.rotation);
            Destroy(eff, 2);
            audioSource.clip = appearClip;
            audioSource.Play();
        }
        if (character)
            character.SetActive(true);
        
        DespawningCharacter = false;
        SpawnNewWeapon();
    }

    void SpawnNewWeapon()
    {        
        var newWeap = Instantiate(weapons[Random.Range(0, weapons.Length)], placeHolderPosition,
            placeHolderQuat); //* Quaternion.Euler(180, 0, 180)
        newWeap.name = newWeap.name.Replace("(Clone)", "");

        if (timeToSpawn > 60)
            Destroy(newWeap, 60);
        else
            Destroy(newWeap, 30);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player>() && character.activeSelf)
            StartCoroutine(DespawnCharacter());
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<Player>() && !DespawningCharacter)
            StartCoroutine(DespawnCharacter());
    }
}
