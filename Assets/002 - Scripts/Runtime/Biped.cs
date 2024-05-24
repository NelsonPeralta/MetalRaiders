using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HitPoints))]
public class Biped : MonoBehaviour
{
    public Vector3 originalSpawnPosition;


    public int ultraMergeCount
    {
        get { return _ultraMergeCount; }
        set
        {
            _ultraMergeCount = value;

            if (_ultraMergeCount == 12)
            {
                print("ULTRA MERGE!");
                SpawnUltraBindExplosion();
            }
        }
    }

    [SerializeField] protected int _ultraMergeCount;




    public virtual void SpawnUltraBindExplosion() { }
}
