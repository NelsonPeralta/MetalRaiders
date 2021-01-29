using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderMaxDis : MonoBehaviour
{
    [Header("AI Scripts")]
    public Skeleton skeleton;
    public Watcher watcher;

    private void Start()
    {
        if (skeleton)
            GetComponent<SphereCollider>().radius = skeleton.maxRange;

        if (watcher)
            GetComponent<SphereCollider>().radius = watcher.maxRangeDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (skeleton)
            if (skeleton.target)
                if (other.gameObject == skeleton.target.gameObject)
                {
                    skeleton.IsInMeleeRange = false;
                    skeleton.IsInMidRange = true;
                    skeleton.IsOutOfRange = false;
                }
        if (watcher)
            if (watcher.target)
                if (other.gameObject == watcher.target.gameObject)
                {
                    watcher.isInMeleeRange = false;
                    watcher.isInRange = true;
                }
    }

    private void OnTriggerExit(Collider other)
    {
        if (skeleton)
            if (skeleton.target)
                if (other.gameObject == skeleton.target.gameObject)
                    if (skeleton.target != null)
                    {
                        skeleton.IsInMidRange = false;
                        skeleton.IsOutOfRange = true;
                    }
        if (watcher)
            if (watcher.target)
                if (other.gameObject == watcher.target.gameObject)
                    if (watcher.target)
                    {
                        watcher.isInMeleeRange = false;
                        watcher.isInRange = false;
                    }
    }
}
