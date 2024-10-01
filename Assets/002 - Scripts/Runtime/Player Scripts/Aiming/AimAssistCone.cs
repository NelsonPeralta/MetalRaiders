using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using Rewired;
using System.Linq;
using static UnityEditor.Progress;

public class AimAssistCone : MonoBehaviour
{
    public delegate void AimAssistConeEvent(AimAssistCone aimAssistCapsule);
    public AimAssistConeEvent OnReticuleFrictionEnabled, OnReticuleFrictionDisabled;

    public Player player;
    public PlayerInventory playerInventory;
    public AimAssist aimAssist;


    public List<GameObject> frictionColliders;
    [SerializeField] bool _reticuleFriction;


    public List<GameObject> collidingHitboxes = new List<GameObject>();
    [SerializeField] GameObject _targetCollisionHitbox, _hitboxRayHitGo, _obstructionHitGo;
    [SerializeField] float distanceToHitbox, distanceToObstruction;

    [SerializeField] int _reticuleFrictionTick;


    [SerializeField] Transform _targetHitboxWitness;
    [SerializeField] PlayerHitboxDetector _invisibleHitboxDetector;


    GameObject _preCollidingHitbox, _tempHbGo;
    List<GameObject> _collidingHitboxesTemp = new List<GameObject>();




    // private
    int _reticuleFrictionLayer = 19;
    float _raycastRange;
    bool _obstructed;
    RaycastHit _obsHit;
    List<RaycastHit> _aimAssistRaycastHitsList = new List<RaycastHit>();

    ReticuleMagnetism _reticuleMagnetism;








    public bool reticuleFriction
    {
        get { return _reticuleFriction; }
        set
        {
            _reticuleFriction = value;

            if (value) { _reticuleFrictionTick = Mathf.Clamp(_reticuleFrictionTick + 3, 0, 30); } else { _reticuleFrictionTick = Mathf.Clamp(_reticuleFrictionTick - 2, 0, 30); }
        }
    }

    public float reticuleFrictionTick
    {
        get
        {
            return _reticuleFrictionTick;
        }
    }

    public GameObject closestHbToCorsshairCenter
    {
        get { return _targetCollisionHitbox; }
        set
        {
            if (value != _targetCollisionHitbox)
            {
                _preCollidingHitbox = _targetCollisionHitbox;
                if (closestHbToCorsshairCenter && !value)
                    aimAssist.ResetRedReticule();
                _targetCollisionHitbox = value;
                //Debug.Log($"AimAssistCone {_preCollidingHitbox} {_collidingHitbox}");

            }

            //if (value == null && player.playerController.rid == 0) print("Nulling targetCollisionHitbox");
        }
    }

    public GameObject hitboxRayHitGo
    {
        get { return _hitboxRayHitGo; }
        set
        {
            var previousValue = hitboxRayHitGo;

            if (value == null)
            {
                //aimAssist.ResetRedReticule();
            }

            _hitboxRayHitGo = value;
            //if (value == null && player.playerController.rid == 0) print("Nulling hitboxRayHitGo");
        }
    }

    public PlayerHitboxDetector invisibleHitboxDetector { get { return invisibleHitboxDetector; } }





    private void Awake()
    {
        _reticuleMagnetism = GetComponent<ReticuleMagnetism>();
    }





    private void Update()
    {
        if (frictionColliders.Count > 0)
            for (int i = 0; i < frictionColliders.Count; i++)
                if (!frictionColliders[i].gameObject.activeSelf || !frictionColliders[i].gameObject.activeInHierarchy)
                    frictionColliders.Remove(frictionColliders[i]);

        if (player.GetComponent<PlayerController>().activeControllerType == ControllerType.Joystick)
            reticuleFriction = _reticuleMagnetism.trueHit;
        else
            reticuleFriction = false;

        //HitboxRay();


        _collidingHitboxesTemp = new List<GameObject>(collidingHitboxes);

        if (_collidingHitboxesTemp.Count > 0)
        {
            _collidingHitboxesTemp = _collidingHitboxesTemp.OrderBy(item => Vector3.Angle(player.mainCamera.transform.forward, (item.transform.position - player.mainCamera.transform.position))).ToList();
            //hitboxRayHitGo = _aimAssistRaycastHitsList[0].collider.gameObject;



            for (int i = _collidingHitboxesTemp.Count; i-- > 0;)
            {

                if (!_collidingHitboxesTemp[i].gameObject.activeSelf || !_collidingHitboxesTemp[i].gameObject.activeInHierarchy /*|| collidingHitboxes[i].GetComponent<Hitbox>().ignoreForAimAssistList*/)
                    _collidingHitboxesTemp.Remove(_collidingHitboxesTemp[i]);
                //else
                //{
                //    if (player.playerController.rid == 0)
                //        print($"{collidingHitboxes[i].transform.name} has an angle of : {Vector3.Angle(player.mainCamera.transform.forward, (collidingHitboxes[i].transform.position - player.mainCamera.transform.position))}");
                //    _anglesOfCollision.Add(Vector3.Angle(player.mainCamera.transform.forward, (collidingHitboxes[i].transform.position - player.mainCamera.transform.position)));
                //}
            }
        }


        if (closestHbToCorsshairCenter && (!closestHbToCorsshairCenter.activeSelf || !closestHbToCorsshairCenter.activeInHierarchy))
        {
            _collidingHitboxesTemp.Remove(closestHbToCorsshairCenter);
            closestHbToCorsshairCenter = null;
        }





        if (player && player.playerInventory && player.playerInventory.activeWeapon)
        {

            Vector3 v = new Vector3(player.playerInventory.activeWeapon.redReticuleHint * 10, transform.localScale.y, player.playerInventory.activeWeapon.redReticuleHint * 10);
            transform.localScale = v;

            v = new Vector3(1, 1, player.playerInventory.activeWeapon.currentRedReticuleRange);
            transform.parent.localScale = v;

            v = new Vector3(1, 1, 1);
            if (player.allPlayerScripts.playerController.activeControllerType == Rewired.ControllerType.Joystick)
                v = new Vector3(2f, 1, 2f);

            _invisibleHitboxDetector.transform.localScale = v;
            _raycastRange = playerInventory.activeWeapon.currentRedReticuleRange;











            if (_collidingHitboxesTemp.Count > 0)
            {
                closestHbToCorsshairCenter = _collidingHitboxesTemp[0];

                foreach (var item in _collidingHitboxesTemp)
                    if ((item.GetComponent<Hitbox>().isHead) && playerInventory.activeWeapon.isHeadshotCapable)
                    {
                        closestHbToCorsshairCenter = item;
                        break;
                    }

                foreach (var item in _collidingHitboxesTemp)
                    if ((item.GetComponent<Hitbox>().isGroin) && playerInventory.activeWeapon.isHeadshotCapable)
                    {
                        closestHbToCorsshairCenter = item;
                        break;
                    }

                //else
                //{
                //    if (Vector3.Distance(item.transform.position, player.mainCamera.transform.position) < Vector3.Distance(targetCollisionHitbox.transform.position, player.mainCamera.transform.position))
                //        targetCollisionHitbox = item;
                //}







                _aimAssistRaycastHitsList = Physics.RaycastAll(player.mainCamera.transform.position, (closestHbToCorsshairCenter.transform.position - player.mainCamera.transform.position),
                     _raycastRange, GameManager.instance.hitboxlayerMask).ToList();

                for (int i = _aimAssistRaycastHitsList.Count; i-- > 0;)
                {
                    if (_aimAssistRaycastHitsList[i].collider.gameObject != closestHbToCorsshairCenter)
                    {
                        //print($"Removing {_aimAssistRaycastHitsList[i].collider} cuz its not {hitboxRayHitGo.name}");
                        _aimAssistRaycastHitsList.RemoveAt(i);
                    }
                }





                //print($"_aimAssistRaycastHitsList {_aimAssistRaycastHitsList.Count} {_aimAssistRaycastHitsList[0].collider.name}");
                if (_aimAssistRaycastHitsList.Count > 0)
                {
                    for (int i = _aimAssistRaycastHitsList.Count; i-- > 0;)
                        if (_aimAssistRaycastHitsList[i].collider.transform.root == transform.root) _aimAssistRaycastHitsList.RemoveAt(i);

                    if (_aimAssistRaycastHitsList.Count > 0)
                    {
                        //_aimAssistRaycastHitsList = _aimAssistRaycastHitsList.OrderBy(x => Vector3.Distance(player.mainCamera.transform.position, x.point)).ToList();
                        //_aimAssistRaycastHitsList = _aimAssistRaycastHitsList.OrderBy(x => Vector3.Angle(player.mainCamera.transform.forward, x.point)).ToList();
                        _targetHitboxWitness.forward = closestHbToCorsshairCenter.transform.position - player.mainCamera.transform.position;
                        //print($"_aimAssistRaycastHitsList hit: {_aimAssistRaycastHitsList[0].collider.name}");
                        hitboxRayHitGo = _aimAssistRaycastHitsList[0].collider.gameObject;

                        distanceToHitbox = Vector3.Distance(_aimAssistRaycastHitsList[0].point, player.mainCamera.transform.position);

                        if (Physics.Raycast(player.mainCamera.transform.position, (closestHbToCorsshairCenter.transform.position - player.mainCamera.transform.position)
                            , out _obsHit, _raycastRange, GameManager.instance.obstructionMask))
                        {
                            _obstructionHitGo = _obsHit.transform.gameObject;
                            distanceToObstruction = Vector3.Distance(_obsHit.point, player.mainCamera.transform.position);
                        }
                        else
                        {
                            _obstructionHitGo = null;
                        }
                    }
                    else
                    {
                        hitboxRayHitGo = null;
                        _obstructionHitGo = null;
                    }
                }
                else
                {
                    hitboxRayHitGo = null;
                    _obstructionHitGo = null;
                }










                //if (Physics.Raycast(player.mainCamera.transform.position, (targetCollisionHitbox.transform.position - player.mainCamera.transform.position),
                //    out hit, _raycastRange, GameManager.instance.hitboxlayerMask))
                //{
                //    print($"AimAssistCone Raycast {hit.collider.name}");
                //    if (hit.transform.root.gameObject != player.gameObject)
                //    {
                //        hitboxRayHitGo = hit.transform.gameObject;
                //        distanceToHitbox = Vector3.Distance(hit.point, player.mainCamera.transform.position);



                //        if (Physics.Raycast(player.mainCamera.transform.position, (targetCollisionHitbox.transform.position - player.mainCamera.transform.position)
                //            , out _obsHit, _raycastRange, GameManager.instance.obstructionMask))
                //        {
                //            print($"AimAssistCone Raycast 1");
                //            _obstructionHitGo = _obsHit.transform.gameObject;
                //            distanceToObstruction = Vector3.Distance(_obsHit.point, player.mainCamera.transform.position);
                //        }
                //        else
                //        {
                //            print($"AimAssistCone Raycast 2");
                //            _obstructionHitGo = null;
                //        }
                //    }
                //    else
                //    {
                //        print($"AimAssistCone Raycast 3");

                //    }
                //}
                //else
                //{
                //    distanceToHitbox = distanceToObstruction = 0;
                //    print($"AimAssistCone Raycast 4");

                //    hitboxRayHitGo = null;
                //    _obstructionHitGo = null;
                //}
            }
            else
            {
                hitboxRayHitGo = null;
                _obstructionHitGo = null;
            }




            _obstructed = false;
            if (_obstructionHitGo && hitboxRayHitGo && distanceToObstruction < distanceToHitbox) _obstructed = true;






            if (hitboxRayHitGo && !_obstructed && closestHbToCorsshairCenter)
            {
                if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                {
                    try
                    {
                        if (closestHbToCorsshairCenter.GetComponent<ActorHitbox>())
                        {
                            aimAssist.targetHitbox = closestHbToCorsshairCenter;
                            aimAssist.redReticuleIsOn = true;
                            playerInventory.activeWeapon.crosshair.color = Crosshair.Color.Red;
                        }
                        else if (closestHbToCorsshairCenter.GetComponent<PlayerHitbox>().player.team == player.team)
                        {
                            aimAssist.targetHitbox = null;
                            aimAssist.redReticuleIsOn = false;
                            playerInventory.activeWeapon.crosshair.color = Crosshair.Color.Green;
                        }
                        else
                        {
                            aimAssist.targetHitbox = closestHbToCorsshairCenter;
                            aimAssist.redReticuleIsOn = true;
                            playerInventory.activeWeapon.crosshair.color = Crosshair.Color.Red;
                        }
                    }
                    catch { }
                }
                else
                {
                    aimAssist.targetHitbox = closestHbToCorsshairCenter;
                    aimAssist.redReticuleIsOn = true;
                    playerInventory.activeWeapon.crosshair.color = Crosshair.Color.Red;
                }

                //aimAssist.crosshairScript.ActivateRedCrosshair();
            }
            else
            {
                aimAssist.targetHitbox = null;
                closestHbToCorsshairCenter = null;
                try { playerInventory.activeWeapon.crosshair.color = Crosshair.Color.Blue; } catch { }
                //aimAssist.ResetRedReticule();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.activeSelf || !other.gameObject.activeInHierarchy)
        {
            //print($"{other.name} is inactive");
        }
        else
        {


            if (other.gameObject.layer != _reticuleFrictionLayer)
                if (!collidingHitboxes.Contains(other.gameObject) && other.gameObject.transform.root != player.transform)
                    collidingHitboxes.Add(other.gameObject);

            if (other.gameObject.layer == _reticuleFrictionLayer)
                if (!frictionColliders.Contains(other.gameObject) && other.gameObject.transform.root != player.transform)
                    frictionColliders.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == closestHbToCorsshairCenter)
        {
            //Debug.Log($"OnTriggerExit from AimAssistCapsule: {other.name}");
            closestHbToCorsshairCenter = null;
        }

        if (collidingHitboxes.Contains(other.gameObject))
            collidingHitboxes.Remove(other.gameObject);
        if (frictionColliders.Contains(other.gameObject))
            frictionColliders.Remove(other.gameObject);

        if (collidingHitboxes.Count == 0)
            aimAssist.ResetRedReticule();
    }


    public void OnActiveWeaponChanged(PlayerInventory playerInventory)
    {
        try
        {
            WeaponProperties activeWeapon = playerInventory.activeWeapon;
            float h = activeWeapon.redReticuleHint;

            //Vector3 v = new Vector3(1, 1, activeWeapon.currentRedReticuleRange / 2);
            //transform.parent.localScale = v;

            Vector3 v = new Vector3(h * 10, transform.localScale.y, h * 10);
            transform.localScale = v;

            v = new Vector3(1, 1, activeWeapon.currentRedReticuleRange / 2);
            transform.parent.localScale = v;
        }
        catch (System.Exception e) { }
    }
}
