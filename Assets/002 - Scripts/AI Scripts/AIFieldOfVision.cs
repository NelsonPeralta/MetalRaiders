using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFieldOfVision : MonoBehaviour
{
    public Transform player0;
    public Transform player1;
    public Transform player2;
    public Transform player3;
    public Transform closestPlayer;
    public bool player0InFOV;
    public bool player1InFOV;
    public bool player2InFOV;
    public bool player3InFOV;
    public float player0Distance;
    public float player1Distance;
    public float player2Distance;
    public float player3Distance;
    public float closestPlayerDistance;
    public float maxAngle;
    public float maxRadius;

    public LayerMask layers;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxRadius);

        Vector3 fovLine1 = Quaternion.AngleAxis(maxAngle, transform.up) * transform.forward * maxRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-maxAngle, transform.up) * transform.forward * maxRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);

        if (player0 != null)
        {
            if (player0.gameObject.activeSelf)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, (player0.position - transform.position).normalized * maxRadius);
            }
        }

        if (player1 != null)
        {
            if (player1.gameObject.activeSelf)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, (player1.position - transform.position).normalized * maxRadius);
            }
        }

        if (player2 != null)
        {
            if (player2.gameObject.activeSelf)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, (player2.position - transform.position).normalized * maxRadius);
            }
        }

        if (player3 != null)
        {
            if (player3.gameObject.activeSelf)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, (player3.position - transform.position).normalized * maxRadius);
            }
        }

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, transform.forward * maxRadius);
    }

    public bool inFoV(Transform checkingObject, Transform target, float maxAngle, float maxRadius)
    {
        Collider[] overlaps = new Collider[100];
        int count = Physics.OverlapSphereNonAlloc(checkingObject.position, maxRadius, overlaps, layers);

        for (int i = 0; i < count + 1; i++)
        {
            if (overlaps[i] != null)
            {
                if (overlaps[i].gameObject.name == target.name)
                {
                    Vector3 directionBetween = (target.position - checkingObject.position).normalized;
                    directionBetween.y *= 0;

                    float angle = Vector3.Angle(checkingObject.forward, directionBetween);

                    if (angle <= maxAngle)
                    {
                        Ray ray = new Ray(checkingObject.position, target.position - checkingObject.position);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, maxRadius, layers))
                        {
                            if (hit.transform == target)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    void BoolManager()
    {
        if (player0 != null)
        {
            if (inFoV(transform, player0, maxAngle, maxRadius))
            {
                player0InFOV = true;
            }
            else
            {
                player0InFOV = false;
            }
        }

        //////////

        if (player1 != null)
        {
            if (inFoV(transform, player1, maxAngle, maxRadius))
            {
                player1InFOV = true;
            }
            else
            {
                player1InFOV = false;
            }
        }

        //////////

        if (player2 != null)
        {
            if (inFoV(transform, player2, maxAngle, maxRadius))
            {
                player2InFOV = true;
            }
            else
            {
                player2InFOV = false;
            }
        }

        //////////

        if (player3 != null)
        {
            if (inFoV(transform, player3, maxAngle, maxRadius))
            {
                player3InFOV = true;
            }
            else
            {
                player3InFOV = false;
            }
        }
    }

    void DistanceCalculator()
    {
        if (player0 != null)
        {
            player0Distance = Vector3.Distance(player0.position, gameObject.transform.position);


        }
        else
        {
            player0Distance = 999;
        }

        if (player1 != null)
        {
            player1Distance = Vector3.Distance(player1.position, gameObject.transform.position);


        }
        else
        {
            player1Distance = 999;
        }

        if (player2 != null)
        {
            player2Distance = Vector3.Distance(player2.position, gameObject.transform.position);


        }
        else
        {
            player2Distance = 999;
        }


        if (player3 != null)
        {
            player3Distance = Vector3.Distance(player3.position, gameObject.transform.position);


        }
        else
        {
            player3Distance = 999;
        }
    }

    void ClosestPlayer()
    {
        if(!player0InFOV && !player2InFOV && !player3InFOV && !player3InFOV)
        {
            closestPlayer = null;
            closestPlayerDistance = 999;
        }

        if (player0InFOV)
        {
            if (closestPlayer != null)
            {
                if (player0Distance < closestPlayerDistance)
                {
                    closestPlayer = player0;
                    closestPlayerDistance = player0Distance;
                }
            }
            else
            {
                closestPlayer = player0;
                closestPlayerDistance = player0Distance;
            }
        }

        
        if (player1InFOV)
        {
            if (closestPlayer != null)
            {
                if (player1Distance < closestPlayerDistance)
                {
                    closestPlayer = player1;
                    closestPlayerDistance = player1Distance;
                }
            }
            else
            {
                closestPlayer = player1;
                closestPlayerDistance = player1Distance;
            }
        }

        /*
        if (player2InFOV)
        {
            if (closestPlayer != null)
            {
                if (player2Distance < closestPlayerDistance)
                {
                    player2 = closestPlayer;
                    closestPlayerDistance = player2Distance;
                }
            }
            else
            {
                closestPlayer = player2;
                closestPlayerDistance = player2Distance;
            }
        }

        if (player3InFOV)
        {
            if (closestPlayer != null)
            {
                if (player3Distance < closestPlayerDistance)
                {
                    player3 = closestPlayer;
                    closestPlayerDistance = player3Distance;
                }
            }
            else
            {
                closestPlayer = player3;
                closestPlayerDistance = player3Distance;
            }
        }
        */
    }

    private void Start()
    {
        InvokeRepeating("UpdateFunction", 0, 1f);
    }

    void UpdateFunction()
    {
        if (player0 != null)
        {
            inFoV(transform, player0, maxAngle, maxRadius);
        }
        if (player1 != null)
        {
            inFoV(transform, player1, maxAngle, maxRadius);
        }
        if (player2 != null)
        {
            inFoV(transform, player2, maxAngle, maxRadius);
        }
        if (player3 != null)
        {
            inFoV(transform, player3, maxAngle, maxRadius);
        }

        BoolManager();
        DistanceCalculator();
        ClosestPlayer();
    }

    /*
    private void Update()
    {
        if (player0 != null)
        {
            inFoV(transform, player0, maxAngle, maxRadius);
        }
        if (player1 != null)
        {
            inFoV(transform, player1, maxAngle, maxRadius);
        }
        if (player2 != null)
        {
            inFoV(transform, player2, maxAngle, maxRadius);
        }
        if (player3 != null)
        {
            inFoV(transform, player3, maxAngle, maxRadius);
        }

        BoolManager();
        DistanceCalculator();
        ClosestPlayer();
    }*/
}
