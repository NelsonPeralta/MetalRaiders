using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarePackagePlane : MonoBehaviour
{
    public GameObject start;
    public GameObject finish;

    public float timeToDestination;
    public float t;

    public bool movementFinished;

    [Header("Weapon Crates")]
    public GameObject smallWeaponCrate;
    public GameObject heavyWeaponCrate;
    public GameObject powerWeaponCrate;

    public GameObject smallWeaponCrateSpawnPoint;
    public GameObject heavyWeaponCrateSpawnPoint;
    public GameObject powerWeaponCrateSpawnPoint;

    public GameObject smallWeaponCrateDestinationPoint;
    public GameObject heavyWeaponCrateDestinationPoint;
    public GameObject powerWeaponCrateDestinationPoint;

    bool droppedWeaponCrates;

    private void LateUpdate()
    {
        if (!movementFinished)
        {
            t += Time.deltaTime / timeToDestination;
            transform.position = Vector3.Lerp(start.transform.position, finish.transform.position, t);

            if(t >= 0.5f && !droppedWeaponCrates)
            {
                var smallCrate = Instantiate(smallWeaponCrate, smallWeaponCrateSpawnPoint.transform.position, smallWeaponCrateSpawnPoint.transform.rotation);
                var heavyCrate = Instantiate(heavyWeaponCrate, heavyWeaponCrateSpawnPoint.transform.position, heavyWeaponCrateSpawnPoint.transform.rotation);
                var powerCrate = Instantiate(powerWeaponCrate, powerWeaponCrateSpawnPoint.transform.position, powerWeaponCrateSpawnPoint.transform.rotation);

                smallCrate.GetComponent<RandomWeaponCrate>().skySpawnPoint = smallWeaponCrateSpawnPoint;
                heavyCrate.GetComponent<RandomWeaponCrate>().skySpawnPoint = heavyWeaponCrateSpawnPoint;
                powerCrate.GetComponent<RandomWeaponCrate>().skySpawnPoint = powerWeaponCrateSpawnPoint;

                smallCrate.GetComponent<RandomWeaponCrate>().destination = smallWeaponCrateDestinationPoint;
                heavyCrate.GetComponent<RandomWeaponCrate>().destination = heavyWeaponCrateDestinationPoint;
                powerCrate.GetComponent<RandomWeaponCrate>().destination = powerWeaponCrateDestinationPoint;

                droppedWeaponCrates = true;

                Destroy(smallCrate, 30);
                Destroy(heavyCrate, 30);
                Destroy(powerCrate, 30);
            }

            if (t >= 1)
            {
                movementFinished = true;
            }
        }
    }
}
