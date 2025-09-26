using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HitPoints))]
public class Biped : MonoBehaviour
{
    public delegate void BipedDelegate();
    public BipedDelegate SplinterShardCountIncrease;

    public Vector3 originalSpawnPosition;


    public int splinterShardCount
    {
        get { return _splinterShardCount; }
        set
        {
            if (value > _splinterShardCount)
            {
                SplinterShardCountIncrease?.Invoke();
            }

            _splinterShardCount = value;

            if (_splinterShardCount == 12)
            {
                Log.Print(() => "ULTRA MERGE!");
                SpawnUltraBindExplosion();
            }

            else if (_splinterShardCount == 0) Debug.Log($"splinterShardCount RESET");

        }
    }

    public Transform targetTrackingCorrectTarget { get { return _targetTrackingCorrectTarget; } }


    [SerializeField] protected int _splinterShardCount;
    [SerializeField] Transform _targetTrackingCorrectTarget;




    public virtual void SpawnUltraBindExplosion() { }
}
