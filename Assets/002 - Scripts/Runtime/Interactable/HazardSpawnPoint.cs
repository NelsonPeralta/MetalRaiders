using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardSpawnPoint : MonoBehaviour
{
    public Hazard hazard;
    public GameObject destroyedHazard;

    private void Awake()
    {
        try { hazard = transform.GetChild(0).GetComponent<Hazard>(); hazard.hazardSpawnPoint = this; } catch { }
    }

    public IEnumerator Reset_Coroutine()
    {
        hazard.gameObject.SetActive(false);
        destroyedHazard.gameObject.SetActive(true);

        yield return new WaitForSeconds(30);

        hazard.ResetHitPoints();
        hazard.gameObject.SetActive(true);
        destroyedHazard.gameObject.SetActive(false);
    }
}
