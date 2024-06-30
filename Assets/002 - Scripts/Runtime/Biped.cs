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

            if (_ultraMergeCount == 10)
            {
                print("ULTRA MERGE!");
                SpawnUltraBindExplosion();
            }
        }
    }

    public Transform targetTrackingCorrectTarget { get { return _targetTrackingCorrectTarget; } }


    [SerializeField] protected int _ultraMergeCount;
    [SerializeField] Transform _targetTrackingCorrectTarget;




    public virtual void SpawnUltraBindExplosion() { }
}
