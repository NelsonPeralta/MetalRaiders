using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderMinDis : MonoBehaviour
{
    [Header("AI Scripts")]
    public Skeleton skeleton;
    public Watcher watcher;

    private void Start()
    {
        if (skeleton)
            GetComponent<SphereCollider>().radius = skeleton.minRange;

        if (watcher)
            GetComponent<SphereCollider>().radius = watcher.maxMeleeDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (skeleton)
            if (skeleton.target)
                if (other.gameObject == skeleton.target.gameObject)
                {
                    skeleton.IsInMeleeRange = true;
                    skeleton.IsInMidRange = false;
                }

        if (watcher)
            if (watcher.target)
                if (other.gameObject == watcher.target.gameObject)
                {
                    watcher.isInMeleeRange = true;
                    watcher.isInRange = false;
                }
    }

    private void OnTriggerExit(Collider other)
    {
        if (skeleton)
            if (skeleton.target)
                if (other.gameObject == skeleton.target.gameObject)
                {
                    skeleton.IsInMeleeRange = false;
                    skeleton.IsInMidRange = true;
                }

        if (watcher)
            if (watcher.target)
                if (other.gameObject == watcher.target.gameObject)
                {
                    watcher.isInMeleeRange = false;
                    watcher.isInRange = true;
                }
    }
}
