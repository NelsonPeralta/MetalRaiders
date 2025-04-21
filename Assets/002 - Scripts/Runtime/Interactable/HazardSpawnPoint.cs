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

    private void Start()
    {
        if (GameManager.instance.oneObjMode == GameManager.OneObjMode.On) GameManager.instance.OnOneObjRoundOverLocalEvent += OnOneObjRoundOverLocalEvent;
    }

    public IEnumerator Reset_Coroutine()
    {
        hazard.gameObject.SetActive(false);
        destroyedHazard.gameObject.SetActive(true);

        yield return new WaitForSeconds(30);

        ResetIceChunk();
    }

    public void ResetIceChunk()
    {
        hazard.ResetHitPoints();
        hazard.gameObject.SetActive(true);
        destroyedHazard.gameObject.SetActive(false);
    }

    void OnOneObjRoundOverLocalEvent()
    {
        StopAllCoroutines();
    }
}
