using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class AimAssistCapsule : MonoBehaviour
{
    public delegate void AimAssistCapsuleEvent(AimAssistCapsule aimAssistCapsule);
    public AimAssistCapsuleEvent OnReticuleFrictionEnabled, OnReticuleFrictionDisabled;

    public Player player;
    public GameObject _collidingHitbox;
    public List<GameObject> collidingHitboxes;
    public List<GameObject> frictionColliders;
    public AimAssist aimAssist;
    RaycastHit hit;
    public LayerMask layerMask;
    public float raycastRange = 1000;
    [SerializeField] GameObject _firstRayHit;

    public bool ReticuleFriction;
    bool _reticuleFriction;

    public bool reticuleFriction
    {
        get { return _reticuleFriction; }
        set { _reticuleFriction = value; }
    }

    public GameObject collidingHitbox
    {
        get { return _collidingHitbox; }
        set
        {
            if (collidingHitbox && !value)
                aimAssist.ResetRedReticule();
            _collidingHitbox = value;
        }
    }

    public GameObject firstRayHit
    {
        get { return _firstRayHit; }
        set
        {
            var previousValue = firstRayHit;

            if (value == null)
            {
                //aimAssist.ResetRedReticule();
            }

            _firstHitPoint = null;
            _firstRayHit = value;
        }
    }

    Vector3? _firstHitPoint;
    public Vector3? firstHitPoint
    {
        get { return _firstHitPoint; }
        set { _firstHitPoint = (Vector3)value; }
    }

    private void Update()
    {
        if (frictionColliders.Count > 0)
            for (int i = 0; i < frictionColliders.Count; i++)
                if (!frictionColliders[i].gameObject.activeSelf || !frictionColliders[i].gameObject.activeInHierarchy)
                    frictionColliders.Remove(frictionColliders[i]);

        if (player.GetComponent<PlayerController>().activeControllerType == ControllerType.Joystick)
        {
            if (frictionColliders.Count == 0)
                reticuleFriction = false;
            else
                reticuleFriction = true;
        }

        ReticuleFriction = reticuleFriction;

        //if (player.GetComponent<PlayerController>().rewiredPlayer.GetButtonUp("Shoot"))
        //{
        //    player.GetComponent<PlayerController>().OnPlayerFireButtonUp?.Invoke(player.GetComponent<PlayerController>());
        //}

        Ray();

        if (collidingHitboxes.Count > 0)
            for (int i = 0; i < collidingHitboxes.Count; i++)
                if (!collidingHitboxes[i].gameObject.activeSelf || !collidingHitboxes[i].gameObject.activeInHierarchy)
                    collidingHitboxes.Remove(collidingHitboxes[i]);

        if (collidingHitbox && (!collidingHitbox.activeSelf || !collidingHitbox.activeInHierarchy))
        {
            collidingHitboxes.Remove(collidingHitbox);
            collidingHitbox = null;
        }

        try
        {
            WeaponProperties activeWeapon = player.playerInventory.activeWeapon;
            Vector3 v = new Vector3(1, 1, activeWeapon.currentRedReticuleRange / 2);
            transform.parent.localScale = v;

            GetComponent<CapsuleCollider>().radius = activeWeapon.redReticuleHint / 10;
        }
        catch (System.Exception e) { }



        //////////

        bool obstruction = false;
        GameObject chb = null;

        if (collidingHitboxes.Count > 0)
        {
            var hb = collidingHitboxes[0];

            foreach (var item in collidingHitboxes)
                if (Vector3.Distance(item.transform.position, player.mainCamera.transform.position) < Vector3.Distance(hb.transform.position, player.mainCamera.transform.position))
                    hb = item;

            chb = hb;
        }

        if (firstRayHit)
        {
            if (firstRayHit.layer == 0)
            {
                if (collidingHitboxes.Count > 0)
                    if (Vector3.Distance(firstRayHit.transform.position, player.mainCamera.transform.position) < Vector3.Distance(chb.transform.position, player.mainCamera.transform.position))
                        obstruction = true;
            }
            else
                chb = firstRayHit;
        }

        collidingHitbox = chb;
        if (!obstruction && collidingHitbox)
        {
            aimAssist.closestHbToCrosshairCenter = collidingHitbox;
            aimAssist.redReticuleIsOn = true;
        }
        else
        {
            aimAssist.ResetRedReticule();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.activeSelf || !other.gameObject.activeInHierarchy)
            return;

        if (other.gameObject.layer != 21)
            if (!collidingHitboxes.Contains(other.gameObject) && other.gameObject.transform.root != player.transform)
                collidingHitboxes.Add(other.gameObject);

        if (other.gameObject.layer == 21)
            if (!frictionColliders.Contains(other.gameObject) && other.gameObject.transform.root != player.transform)
                frictionColliders.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == collidingHitbox)
        {
            //Debug.Log($"OnTriggerExit from AimAssistCapsule: {other.name}");
            collidingHitbox = null;
        }

        if (collidingHitboxes.Contains(other.gameObject))
            collidingHitboxes.Remove(other.gameObject);
        if (frictionColliders.Contains(other.gameObject))
            frictionColliders.Remove(other.gameObject);

        if (collidingHitboxes.Count == 0)
            aimAssist.ResetRedReticule();
    }

    void Ray()
    {
        try
        { raycastRange = player.playerInventory.activeWeapon.currentRedReticuleRange; }
        catch (System.Exception) { }

        if (Physics.Raycast(player.mainCamera.transform.position, player.mainCamera.transform.forward, out hit, raycastRange, layerMask))
        {
            if (hit.transform.root.gameObject != player.gameObject)
            {
                firstRayHit = hit.transform.gameObject;
                firstHitPoint = hit.point;
            }
        }
        else
        {
            firstRayHit = null;
        }
    }
}
