using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AimAssist : MonoBehaviour
{
    public GameObject firstRayHit;
    public Player player;
    public bool redReticuleIsOn;

    public int playerRewiredID;
    public Transform puCollider;
    public CrosshairManager crosshairScript;
    public GameObject target;
    public ActorHitbox ActorHitbox;
    public PlayerHitbox targetHitbox;
    public LayerMask layerMask;

    public PlayerInventory pInventory;
    public Player pProperties;
    public WeaponProperties wProperties;
    public PlayerController pController;

    public float raycastRange = 1000;
    public float targetDistance;

    Vector3 raySpawn;
    RaycastHit hit;

    public Transform bulletSpawnPoint;
    public Quaternion originalBbulletSpawnPointRelativePos;

    [SerializeField] Transform bulletSpawnPoint_Forward;

    [Header("MANUAL LINKING")]
    public Player thisPlayer;

    public Camera mainCam;

    private void Start()
    {
        originalBbulletSpawnPointRelativePos = bulletSpawnPoint.transform.localRotation;
    }

    private void Update()
    {
        if (redReticuleIsOn)
        {
            // https://forum.unity.com/threads/find-a-point-on-a-line-between-two-vector3.140700/
            // https://www.varsitytutors.com/hotmath/hotmath_help/topics/adding-and-subtracting-vectors#:~:text=To%20add%20or%20subtract%20two,v2%E2%9F%A9%20be%20two%20vectors.&text=The%20sum%20of%20two%20or,method%20or%20the%20triangle%20method%20.
            // https://answers.unity.com/questions/459532/how-to-get-a-point-on-a-direction.html

            //Vector3 bspDir = (bulletSpawnPoint_Forward.transform.position - bulletSpawnPoint.position).normalized;
            //Vector3 targetDir = (target.transform.position - bulletSpawnPoint.position).normalized;

            Vector3 bspDir = (bulletSpawnPoint_Forward.transform.position - bulletSpawnPoint.position).normalized;
            Vector3 targetDir = (target.transform.position - bulletSpawnPoint.position);

            Vector3 middleDir = bspDir + targetDir;

            bulletSpawnPoint.forward = (middleDir);

            //bulletSpawnPoint.LookAt(target.transform);
        }
        else
        {
            if (bulletSpawnPoint.transform.localRotation != originalBbulletSpawnPointRelativePos)
                bulletSpawnPoint.transform.localRotation = originalBbulletSpawnPointRelativePos;
        }
    }

    private void FixedUpdate()
    {
        if (!player.PV.IsMine)
            return;
        RedReticule();
    }

    void RedReticule()
    {
        //if (!player.allPlayerScripts.playerInventory.activeWeapon)
        //    return;
        //if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, raycastRange, layerMask))
        //{
        //    if (hit.transform.root.gameObject != player.gameObject)
        //    {
        //        targetDistance = Vector3.Distance(hit.transform.position, player.transform.position);
        //        firstRayHit = hit.transform.gameObject;
        //        float gunRRR = player.allPlayerScripts.playerInventory.activeWeapon.GetComponent<WeaponProperties>().currentRedReticuleRange;

        //        if (!hit.transform.gameObject.GetComponent<PlayerHitbox>() && !hit.transform.gameObject.GetComponent<ActorHitbox>())
        //        {
        //            ResetRedReticule();
        //            return;
        //        }

        //        if (hit.transform.root.GetComponent<Player>() || hit.transform.GetComponent<ActorHitbox>())
        //            if (player.gameObject && targetDistance <= gunRRR)
        //                ActivateRedReticule();
        //            else
        //                ResetRedReticule();
        //    }
        //    else
        //        ResetRedReticule();
        //}
        //else
        //{
        //    ResetRedReticule();
        //}
        //Debug.DrawRay(mainCam.transform.position, mainCam.transform.forward * 100, Color.green);
    }

    public void ActivateRedReticule()
    {
        redReticuleIsOn = true;
        target = hit.transform.gameObject;
    }
    public void ResetRedReticule()
    {
        redReticuleIsOn = false;
        target = null;
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
            if (hit.transform.gameObject.GetComponent<ActorHitbox>() != null && hit.transform.gameObject.GetComponent<ActorHitbox>().gameObject.layer == 13)
            {
                //Debug.Log("Hitting AI Hitbox");
                target = hit.transform.gameObject.GetComponent<ActorHitbox>().actor.gameObject;
                ActorHitbox = hit.transform.gameObject.GetComponent<ActorHitbox>();
                targetDistance = hit.distance;
            }
            else if (hit.transform.gameObject.GetComponent<PlayerHitbox>() != null && hit.transform.gameObject.GetComponent<PlayerHitbox>().gameObject.layer == 13)
            {

                target = hit.transform.gameObject.GetComponent<PlayerHitbox>().player.gameObject;
                targetHitbox = hit.transform.gameObject.GetComponent<PlayerHitbox>();
                targetDistance = hit.distance;
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
                //Debug.Log("Detecting Player Hitbox. Target distance: " + targetDistance + ". RRR: + " + wProperties.RedReticuleRange);

                if (targetDistance <= wProperties.currentRedReticuleRange && target != null)
                {

                }
                else if (targetDistance > wProperties.currentRedReticuleRange && target != null)
                {

                }

            }

            if (hit.transform.gameObject.GetComponent<ActorHitbox>() != null)
            {

                //Debug.Log("Check 2");
                targetDistance = hit.distance;

                if (wProperties != null)
                {
                    if (targetDistance <= wProperties.currentRedReticuleRange && target != null)
                    {
                        //Debug.Log("Here 2");
                        //if (string.Equals(hit.transform.gameObject.GetComponent<ActorHitbox>().team.Trim(), playerMPProperties.team.Trim()))
                        //{
                        //    //Debug.Log("Here 3");
                        //    crosshairScript.friendlyRRisActive = true;
                        //}
                        //else
                        //if
                        //{
                        //    //Debug.Log("Here 4");
                        //    crosshairScript.RRisActive = true;
                        //}
                    }
                    else if (targetDistance > wProperties.currentRedReticuleRange && target != null)
                    {
                    }

                }
            }
        }
        else
        {
            //Debug.Log("No hit");
            target = null;
            ActorHitbox = null;
            targetHitbox = null;
            targetDistance = raycastRange;
        }
    }
}

