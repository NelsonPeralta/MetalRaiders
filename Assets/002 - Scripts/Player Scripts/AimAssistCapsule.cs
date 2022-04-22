using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAssistCapsule : MonoBehaviour
{
    public Player player;
    public GameObject collidingHitbox;
    public AimAssist aimAssist;

    private void Update()
    {
        try
        {
            WeaponProperties activeWeapon = player.playerInventory.activeWeapon;
            Vector3 v = new Vector3(1, 1, activeWeapon.currentRedReticuleRange / 2);
            transform.parent.localScale = v;

            GetComponent<CapsuleCollider>().radius = activeWeapon.redReticuleHint / 10;
        }
        catch (System.Exception e)
        {

        }

        //GetComponent<CapsuleCollider>().radius = activeWeapon.redReticuleRadius;
        //GetComponent<CapsuleCollider>().radius = activeWeapon.redReticuleRadius;
    }
    private void OnTriggerStay(Collider other)
    {
        if (!collidingHitbox)
        {
            Debug.Log($"OnTriggerEnter from AimAssistCapsule: {other.name}");
            collidingHitbox = other.gameObject;
        }
        else
        {

            if (Vector3.Distance(other.transform.position, collidingHitbox.transform.position) < Vector3.Distance(collidingHitbox.transform.position, collidingHitbox.transform.position))
                collidingHitbox = other.gameObject;
        }
        aimAssist.target = collidingHitbox;
        aimAssist.redReticuleIsOn = true;
        aimAssist.crosshairScript.ActivateRedCrosshair();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == collidingHitbox)
        {
            Debug.Log($"OnTriggerExit from AimAssistCapsule: {other.name}");
            //transform.parent.localScale = new Vector3(0, 0, 0);
            //GetComponent<CapsuleCollider>().radius = 0;
            collidingHitbox = null;
        }
    }
}
