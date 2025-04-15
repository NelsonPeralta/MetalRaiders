using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System.Linq;

public class AimAssistCone : MonoBehaviour
{
    public delegate void AimAssistConeEvent(AimAssistCone aimAssistCapsule);
    public AimAssistConeEvent OnReticuleFrictionEnabled, OnReticuleFrictionDisabled;

    public Player player;
    public PlayerInventory playerInventory;
    public AimAssist aimAssist;
    public PlayerHitboxDetector hitboxDetector;


    public List<GameObject> _collidingHitboxesTemp = new List<GameObject>();




    [SerializeField] Transform _hitboxDetectorScaleControl;

    [SerializeField] GameObject _closestHbToCorsshairCenter, _hitboxRayHitGo, _obstructionHitGo;
    [SerializeField] float distanceToHitbox, distanceToObstruction, _angleBetweenCameraCenterAndClosestHitboxToCenter;

    [SerializeField] int _reticuleFrictionTick;


    [SerializeField] Transform _targetHitboxWitness;
    [SerializeField] PlayerHitboxDetector _invisibleHitboxDetector;


    GameObject _preCollidingHitbox, _tempHbGo;




    // private
    int _reticuleFrictionLayer = 19;
    float _raycastRange;
    bool _obstructed;
    RaycastHit _obsHit;
    List<RaycastHit> _aimAssistRaycastHitsList = new List<RaycastHit>();

    ReticuleMagnetism _reticuleMagnetism;
    Rigidbody _rb;

    float _coneXandYScale, _tempRedReticuleAngle, _tempRedReticuleRange, _correctedAngle;
    bool _reticuleFriction;






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
        get { return _closestHbToCorsshairCenter; }
        set
        {
            if (value != _closestHbToCorsshairCenter)
            {
                _preCollidingHitbox = _closestHbToCorsshairCenter;
                if (closestHbToCorsshairCenter && !value)
                {
                    aimAssist.ResetRedReticule();
                    _angleBetweenCameraCenterAndClosestHitboxToCenter = -1;
                }
                _closestHbToCorsshairCenter = value;

                if (_closestHbToCorsshairCenter != null)
                {
                    _angleBetweenCameraCenterAndClosestHitboxToCenter = Vector3.Angle(player.mainCamera.transform.forward, _closestHbToCorsshairCenter.transform.position - player.mainCamera.transform.position);
                }
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
        _rb = GetComponent<Rigidbody>();


        player.OnPlayerIdAssigned -= OnPlayerAssigned;
        player.OnPlayerIdAssigned += OnPlayerAssigned;
    }





    private void Update()
    {
        if (!player.isMine) return;


        if (!player.isAlive)
        {
            _collidingHitboxesTemp.Clear();

            reticuleFriction = false;

            closestHbToCorsshairCenter = null;
            hitboxRayHitGo = null;
            _obstructionHitGo = null;
        }
        else
        {
            if (player.GetComponent<PlayerController>().activeControllerType == ControllerType.Joystick)
                reticuleFriction = _reticuleMagnetism.trueHit;
            else
                reticuleFriction = false;

            //HitboxRay();


            _collidingHitboxesTemp = new List<GameObject>(hitboxDetector.collidingHitboxes);

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

                //Vector3 v = new Vector3(player.playerInventory.activeWeapon.redReticuleHint * (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || player.playerInventory.isHoldingHeavy ? 15 : 10), transform.localScale.y, player.playerInventory.activeWeapon.redReticuleHint * (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || player.playerInventory.isHoldingHeavy ? 15 : 10));
                //if (player.isDualWielding) v = new Vector3((player.playerInventory.activeWeapon.redReticuleHint * (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || player.playerInventory.isHoldingHeavy ? 15 : 10) + player.playerInventory.thirdWeapon.redReticuleHint * 10) / 2f, transform.localScale.y, (player.playerInventory.activeWeapon.redReticuleHint * (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || player.playerInventory.isHoldingHeavy ? 15 : 10) + player.playerInventory.thirdWeapon.redReticuleHint * (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || player.playerInventory.isHoldingHeavy ? 15 : 10)) / 2f);
                //transform.localScale = v;

                //v = new Vector3(1, 1, player.playerInventory.activeWeapon.currentRedReticuleRange);
                //transform.parent.localScale = v;

                //v = new Vector3(1, 1, 1);
                //if (player.allPlayerScripts.playerController.activeControllerType == Rewired.ControllerType.Joystick)
                //    v = new Vector3(2f, 1, 2f);

                //_invisibleHitboxDetector.transform.localScale = v;



                _tempRedReticuleAngle = player.playerInventory.activeWeapon.redReticuleDefaultRadius; // calculated using default angle and default RRR
                if (player.playerController.isAiming) _tempRedReticuleAngle = player.playerInventory.activeWeapon.redReticuleScopedRadius;
                else if (player.isDualWielding) _tempRedReticuleAngle = (player.playerInventory.activeWeapon.redReticuleDefaultRadius + player.playerInventory.thirdWeapon.redReticuleDefaultRadius) / 2f;
                _tempRedReticuleRange = player.playerInventory.activeWeapon.currentRedReticuleRange;
                if (player.isDualWielding) _tempRedReticuleRange = (player.playerInventory.activeWeapon.defaultRedReticuleRange + player.playerInventory.thirdWeapon.defaultRedReticuleRange) / 2f;
                _coneXandYScale = Mathf.Tan((_tempRedReticuleAngle * Mathf.PI) / 180) * _tempRedReticuleRange; // we calculate the opposite side of a triangle using the default RRR as the adjacent. Must be multiplied by 2 to take into account the whole heigh and width of the cone geometry
                //if (player.playerController.isAiming && player.playerInventory.activeWeapon.aimingMechanic == WeaponProperties.AimingMechanic.Zoom) _coneXandYScale = _coneXandYScale * 0.8f; // ARBITRARY because of geometry when zooming camera
                
                if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || playerInventory.isHoldingHeavy) _coneXandYScale *= 1.15f;
                _hitboxDetectorScaleControl.transform.localScale = new Vector3(_coneXandYScale * 2, _coneXandYScale * 2, _tempRedReticuleRange);
                _raycastRange = _tempRedReticuleRange;











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
                                aimAssist.closestHbToCrosshairCenter = closestHbToCorsshairCenter;
                                aimAssist.redReticuleIsOn = true;
                                playerInventory.activeWeapon.crosshair.color = Crosshair.Color.Red;

                                if (player.isDualWielding) playerInventory.thirdWeapon.crosshair.color = Crosshair.Color.Red;
                            }
                            else if (closestHbToCorsshairCenter.GetComponent<PlayerHitbox>().player.team == player.team)
                            {
                                aimAssist.closestHbToCrosshairCenter = null;
                                aimAssist.redReticuleIsOn = false;
                                playerInventory.activeWeapon.crosshair.color = Crosshair.Color.Green;

                                if (player.isDualWielding) playerInventory.thirdWeapon.crosshair.color = Crosshair.Color.Green;
                            }
                            else
                            {
                                aimAssist.closestHbToCrosshairCenter = closestHbToCorsshairCenter;
                                aimAssist.redReticuleIsOn = true;
                                playerInventory.activeWeapon.crosshair.color = Crosshair.Color.Red;
                                if (player.isDualWielding) playerInventory.thirdWeapon.crosshair.color = Crosshair.Color.Red;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        aimAssist.closestHbToCrosshairCenter = closestHbToCorsshairCenter;
                        aimAssist.redReticuleIsOn = true;
                        playerInventory.activeWeapon.crosshair.color = Crosshair.Color.Red;
                        if (player.isDualWielding) playerInventory.thirdWeapon.crosshair.color = Crosshair.Color.Red;
                    }

                    //aimAssist.crosshairScript.ActivateRedCrosshair();
                }
                else
                {
                    aimAssist.closestHbToCrosshairCenter = null;
                    closestHbToCorsshairCenter = null;
                    try { playerInventory.activeWeapon.crosshair.color = Crosshair.Color.Blue; } catch { }
                    if (player.isDualWielding) playerInventory.thirdWeapon.crosshair.color = Crosshair.Color.Blue;
                    //aimAssist.ResetRedReticule();
                }
            }

            if (_closestHbToCorsshairCenter != null)
                _angleBetweenCameraCenterAndClosestHitboxToCenter = Vector3.Angle(player.mainCamera.transform.forward, _closestHbToCorsshairCenter.transform.position - player.mainCamera.transform.position);

            //if (player.playerController.rid == 0 && player.isMine) print($"Update {_frame} {doNotClearListThisFrame}");
        }
    }



    private void OnTriggerStay(Collider other) // is called after update
    {

    }

    private void OnTriggerExit(Collider other)
    {

    }


    public void OnActiveWeaponChanged(PlayerInventory playerInventory)
    {

    }

    void OnPlayerAssigned(Player p)
    {
        if (!p.isMine)
        {
            try { GetComponent<Collider>().enabled = false; } catch { }
        }
    }
}
