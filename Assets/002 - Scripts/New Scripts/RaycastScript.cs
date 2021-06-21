using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastScript : MonoBehaviour
{
    public GameObject firstRayHit;
    public MyPlayerManager pManager;

    public int playerRewiredID;
    public Transform puCollider;
    public CrosshairScript crosshairScript;
    public GameObject target;
    public AIHitbox aiHitbox;
    public PlayerHitbox targetHitbox;
    public LayerMask layerMask;

    public PlayerInventory pInventory;
    public PlayerProperties pProperties;
    public WeaponProperties wProperties;
    public PlayerController pController;
    public TeamInfo tInfo;

    public float raycastRange = 1000;
    public float targetDistance;

    Vector3 raySpawn;
    public RaycastHit hit;

    [Header("MANUAL LINKING")]
    public PlayerProperties thisPlayer;
    public PlayerMPProperties playerMPProperties;

    private void Start()
    {
        //pManager = GameObject.FindGameObjectWithTag("Player Manager").GetComponent<PlayerManager>();
        puCollider = GetComponent<Transform>();

        if (pManager != null)
        {
            pController = pManager.allPlayers[playerRewiredID].gameObject.GetComponent<PlayerController>();
        }
    }

    private void FixedUpdate()
    {
        if (!pController.isDualWielding)
        {
            if (pInventory.activeWeapIs == 0)
            {
                if (pInventory.weaponsEquiped[0] != null)
                {
                    wProperties = pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>();
                }
            }
            else if (pInventory.activeWeapIs == 1)
            {
                wProperties = pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>();
            }

            raySpawn = puCollider.transform.position + new Vector3(0, 0f, 0);

            if (wProperties != null)
            {
                raycastRange = wProperties.RedReticuleRange;
            }

            ShootInspectorRay();
            ShootPlayerRay();
            ShootObstacleRay();
        }
        else
        {
            crosshairScript.RRisActive = false;
            crosshairScript.friendlyRRisActive = false;
            targetHitbox = null;
            aiHitbox = null;
        }
    }

    void ShootInspectorRay()
    {
        if (Physics.Raycast(raySpawn, puCollider.transform.forward * raycastRange, out hit, raycastRange, layerMask)) // Need a Raycast Range Overload to work with LayerMask
        {
            firstRayHit = hit.transform.gameObject;
        }
    }

    void ShootPlayerRay()
    {
        Vector3 origin = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y - 0.150f);

        Debug.DrawRay(raySpawn, puCollider.transform.forward * raycastRange, Color.green);

        if (Physics.Raycast(raySpawn, puCollider.transform.forward * raycastRange, out hit, raycastRange, layerMask)) // Need a Raycast Range Overload to work with LayerMask
        {
            if (hit.transform.gameObject.GetComponent<AIHitbox>() != null && hit.transform.gameObject.GetComponent<AIHitbox>().gameObject.layer == 13)
            {
                //Debug.Log("Hitting AI Hitbox");
                target = hit.transform.gameObject.GetComponent<AIHitbox>().aiGO;
                aiHitbox = hit.transform.gameObject.GetComponent<AIHitbox>();
                targetDistance = hit.distance;
            }
            else if (hit.transform.gameObject.GetComponent<PlayerHitbox>() != null && hit.transform.gameObject.GetComponent<PlayerHitbox>().gameObject.layer == 13)
            {
                if (hit.transform.gameObject.GetComponent<PlayerHitbox>().player.GetComponent<PlayerProperties>().playerRewiredID != playerRewiredID) //If its player's own Hitbox, ignore
                {
                    target = hit.transform.gameObject.GetComponent<PlayerHitbox>().player;
                    targetHitbox = hit.transform.gameObject.GetComponent<PlayerHitbox>();
                    targetDistance = hit.distance;
                }
            }
        }
    }

    void ShootObstacleRay()
    {
        if (Physics.Raycast(raySpawn, puCollider.transform.forward * raycastRange, out hit, 10000, layerMask)) // Need a Raycast Range Overload to work with LayerMask
        {

            if (hit.transform.gameObject.GetComponent<PlayerHitbox>() != null)
            {
                targetDistance = hit.distance;

                if (targetDistance <= wProperties.RedReticuleRange && target != null)
                {
                    if (string.Equals(target.GetComponent<AllPlayerScripts>().playerMPProperties.team.Trim(), playerMPProperties.team.Trim()))
                    {
                        crosshairScript.friendlyRRisActive = true;
                    }
                    else
                    {
                        crosshairScript.RRisActive = true;
                    }
                }
                else if (targetDistance > wProperties.RedReticuleRange && target != null)
                {
                    crosshairScript.RRisActive = false;
                    crosshairScript.friendlyRRisActive = false;
                }

                if (hit.transform.gameObject.layer == 12)
                {
                    crosshairScript.RRisActive = false;
                    crosshairScript.friendlyRRisActive = false;
                    targetHitbox = null;
                }
            }

            if (hit.transform.gameObject.GetComponent<AIHitbox>() != null)
            {

                //Debug.Log("Check 2");
                targetDistance = hit.distance;

                if (wProperties != null)
                {
                    if (targetDistance <= wProperties.RedReticuleRange && target != null)
                    {
                        //Debug.Log("Here 2");
                        if (string.Equals(hit.transform.gameObject.GetComponent<AIHitbox>().team.Trim(), playerMPProperties.team.Trim()))
                        {
                            //Debug.Log("Here 3");
                            crosshairScript.friendlyRRisActive = true;
                        }
                        else
                        {
                            //Debug.Log("Here 4");
                            crosshairScript.RRisActive = true;
                        }
                    }
                    else if (targetDistance > wProperties.RedReticuleRange && target != null)
                    {
                        //Debug.Log("Here 5");
                        crosshairScript.RRisActive = false;
                        crosshairScript.friendlyRRisActive = false;
                    }

                    if (hit.transform.gameObject.layer == 12)
                    {
                        //Debug.Log("Here 6");
                        crosshairScript.RRisActive = false;
                        crosshairScript.friendlyRRisActive = false;
                        aiHitbox = null;
                    }
                }
            }
        }
        else
        {
            //Debug.Log("No hit");
            target = null;
            aiHitbox = null;
            targetHitbox = null;
            targetDistance = raycastRange;
            crosshairScript.RRisActive = false;
            crosshairScript.friendlyRRisActive = false;
        }
    }
}

