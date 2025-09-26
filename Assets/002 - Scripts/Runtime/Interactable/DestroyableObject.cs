using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HitPoints))]
public class DestroyableObject : MonoBehaviour
{
    public HitPoints hitPoints;

    private void Awake()
    {
        hitPoints = GetComponent<HitPoints>();
    }
    private void Start()
    {
        hitPoints.OnDeath -= OnDeath_Delegate;
        hitPoints.OnDeath += OnDeath_Delegate;
    }


    void OnDeath_Delegate(HitPoints hp)
    {
        Log.Print(() =>"DestroyableObject");
        gameObject.SetActive(false);
    }
}
