using Rewired;
using UnityEngine;

public class AimAssist : MonoBehaviour
{
    public GameObject closestHbToCrosshairCenter
    {
        get { return _closestHbToCrosshairCenter; }
        set
        {
            if (value != _closestHbToCrosshairCenter)
            {
                _preTargetHitboxRoot = _closestHbToCrosshairCenter;
                _closestHbToCrosshairCenter = value;
                //Debug.Log($"targetHitboxRoot {_preTargetHitboxRoot} {_targetHitboxRoot}");
            }
        }
    }

    public Vector3 targetPointPosition
    {
        get { Log.Print(() => _targetPointPosition); return _targetPointPosition; }
    }

    public bool redReticuleIsOn
    {
        get { return _redReticuleIsOn; }
        set
        {
            _redReticuleIsOn = value;
            //if (value) { _redReticuleTick = Mathf.Clamp(_redReticuleTick + 1, 0, 30); } else { _redReticuleTick = Mathf.Clamp(_redReticuleTick - 1, 0, 30); }
        }
    }

    public int redReticuleTick
    {
        get
        {
            if (player.playerController.activeControllerType == ControllerType.Custom || player.GetComponent<PlayerController>().activeControllerType == ControllerType.Joystick)
                return _redReticuleTick;

            return 0;
        }
    }

    public bool invisibleAimAssistOn { get { if (_invisibleHitboxDetector.collidingHitboxes.Count > 0) return true; else return false; } }

    public bool theObjectIntheMiddleIsBehindTheCamera { get { return player.playerCamera.playerCameraCenterPointCheck.theObjectIntheMiddleIsBehindTheCamera; } }







    public Player player;
    public Transform aimAssistRotationControl;








    [SerializeField] LayerMask _layerMask, _obstructionMask;
    [SerializeField] Transform bulletSpawnPoint_Forward;
    [SerializeField] PlayerHitboxDetector _invisibleHitboxDetector;
    [SerializeField] GameObject _tempTargetHitbox, _closestHbToCrosshairCenter;







    bool _redReticuleIsOn;
    int _redReticuleTick;
    float targetHitboxDistance;
    Vector3 raySpawn;
    RaycastHit hit;
    Vector3 _targetPointPosition, bspDir, targetDir, middleDir, newDir, targetHitboxDir;
    Quaternion originalBbulletSpawnPointRelativePos;
    GameObject _preTargetHitboxRoot;










    private void Start()
    {
        originalBbulletSpawnPointRelativePos = aimAssistRotationControl.transform.localRotation;
    }

    private void Update()
    {
        if (player && !player.isMine) return;




        if (redReticuleIsOn) { _redReticuleTick = Mathf.Clamp(_redReticuleTick + 3, 0, 30); } else { _redReticuleTick = Mathf.Clamp(_redReticuleTick - 2, 0, 30); }


        if (closestHbToCrosshairCenter)
        {
            if (redReticuleIsOn)
            {
                // https://forum.unity.com/threads/find-a-point-on-a-line-between-two-vector3.140700/
                // https://www.varsitytutors.com/hotmath/hotmath_help/topics/adding-and-subtracting-vectors#:~:text=To%20add%20or%20subtract%20two,v2%E2%9F%A9%20be%20two%20vectors.&text=The%20sum%20of%20two%20or,method%20or%20the%20triangle%20method%20.
                // https://answers.unity.com/questions/459532/how-to-get-a-point-on-a-direction.html

                //Vector3 bspDir = (bulletSpawnPoint_Forward.transform.position - bulletSpawnPoint.position).normalized;
                //Vector3 targetDir = (target.transform.position - bulletSpawnPoint.position).normalized;


                _targetPointPosition = closestHbToCrosshairCenter.transform.position;
                if (GameManager.instance.thirdPersonMode != GameManager.ThirdPersonMode.On && !player.playerInventory.isHoldingHeavy)
                {

                    bspDir = (bulletSpawnPoint_Forward.transform.position - aimAssistRotationControl.position).normalized;
                    targetDir = (closestHbToCrosshairCenter.transform.position - aimAssistRotationControl.position);

                    middleDir = bspDir + targetDir;

                    aimAssistRotationControl.forward = (middleDir);
                }
                else
                {
                    if (aimAssistRotationControl.transform.localRotation != originalBbulletSpawnPointRelativePos)
                        aimAssistRotationControl.transform.localRotation = originalBbulletSpawnPointRelativePos;
                }

                //bulletSpawnPoint.LookAt(target.transform);
            }
            //else if (_invisibleHitboxDetector.collidingHitboxes.Count > 0) // DEPRECATED
            //{
            //    try { if (player.isDead || player.isRespawning) { return; } } catch { }

            //    _tempTargetHitbox = _invisibleHitboxDetector.collidingHitboxes[0];



            //    for (int i = _invisibleHitboxDetector.collidingHitboxes.Count; i-- > 0;)
            //        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            //        {
            //            if (_tempTargetHitbox.GetComponent<ActorHitbox>() || (_tempTargetHitbox.GetComponent<PlayerHitbox>().player.team == player.team))
            //                if (_invisibleHitboxDetector.collidingHitboxes[i].GetComponent<Hitbox>().isHead)
            //                {
            //                    _tempTargetHitbox = _invisibleHitboxDetector.collidingHitboxes[i];
            //                    break;
            //                }
            //                else
            //                {
            //                    if (Vector3.Distance(_invisibleHitboxDetector.collidingHitboxes[i].transform.position, player.mainCamera.transform.position) < Vector3.Distance(_tempTargetHitbox.transform.position, player.mainCamera.transform.position))
            //                        _tempTargetHitbox = _invisibleHitboxDetector.collidingHitboxes[i];
            //                }
            //        }
            //        else
            //        {
            //            if (_invisibleHitboxDetector.collidingHitboxes[i].GetComponent<Hitbox>().isHead)
            //            {
            //                _tempTargetHitbox = _invisibleHitboxDetector.collidingHitboxes[i];
            //                break;
            //            }
            //            else
            //            {
            //                if (Vector3.Distance(_invisibleHitboxDetector.collidingHitboxes[i].transform.position, player.mainCamera.transform.position) < Vector3.Distance(_tempTargetHitbox.transform.position, player.mainCamera.transform.position))
            //                    _tempTargetHitbox = _invisibleHitboxDetector.collidingHitboxes[i];
            //            }
            //        }

            //    {
            //        //foreach (var item in _invisibleHitboxDetector.collidingHitboxes)
            //        //    if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            //        //    {
            //        //        try
            //        //        {
            //        //            if (_tempTargetHitbox.GetComponent<ActorHitbox>() || (_tempTargetHitbox.GetComponent<PlayerHitbox>().player.team == player.team))
            //        //                if (item.GetComponent<Hitbox>().isHead)
            //        //                {
            //        //                    _tempTargetHitbox = item;
            //        //                    break;
            //        //                }
            //        //                else
            //        //                {
            //        //                    if (Vector3.Distance(item.transform.position, player.mainCamera.transform.position) < Vector3.Distance(_tempTargetHitbox.transform.position, player.mainCamera.transform.position))
            //        //                        _tempTargetHitbox = item;
            //        //                }
            //        //        }
            //        //        catch
            //        //        {
            //        //            if (item.GetComponent<Hitbox>().isHead)
            //        //            {
            //        //                _tempTargetHitbox = item;
            //        //                break;
            //        //            }
            //        //            else
            //        //            {
            //        //                if (Vector3.Distance(item.transform.position, player.mainCamera.transform.position) < Vector3.Distance(_tempTargetHitbox.transform.position, player.mainCamera.transform.position))
            //        //                    _tempTargetHitbox = item;
            //        //            }
            //        //        }
            //        //    }
            //        //    else
            //        //    {

            //        //        if (item.GetComponent<Hitbox>().isHead)
            //        //        {
            //        //            _tempTargetHitbox = item;
            //        //            break;
            //        //        }
            //        //        else
            //        //        {
            //        //            if (Vector3.Distance(item.transform.position, player.mainCamera.transform.position) < Vector3.Distance(_tempTargetHitbox.transform.position, player.mainCamera.transform.position))
            //        //                _tempTargetHitbox = item;
            //        //        }
            //        //    }

            //    }



            //    if (_tempTargetHitbox.GetComponent<PlayerHitbox>() &&
            //        (GameManager.instance.teamMode == GameManager.TeamMode.Classic &&
            //        _tempTargetHitbox.GetComponent<PlayerHitbox>().player.team == player.team))
            //    {
            //        if (aimAssistRotationControl.transform.localRotation != originalBbulletSpawnPointRelativePos)
            //            aimAssistRotationControl.transform.localRotation = originalBbulletSpawnPointRelativePos;

            //        _targetPointPosition = Vector3.zero;

            //        return;
            //    }

            //    targetHitboxDir = (_tempTargetHitbox.transform.position - aimAssistRotationControl.position);
            //    targetHitboxDistance = Vector3.Distance(_tempTargetHitbox.transform.position, aimAssistRotationControl.position);




            //    if (!Physics.Raycast(player.mainCamera.transform.position, targetHitboxDir, targetHitboxDistance, obstructionMask))
            //    {
            //        if (GameManager.instance.thirdPersonMode != GameManager.ThirdPersonMode.On && !player.playerInventory.isHoldingHeavy)
            //        {
            //            _targetPointPosition = _tempTargetHitbox.transform.position;

            //            bspDir = (bulletSpawnPoint_Forward.transform.position - aimAssistRotationControl.position).normalized;
            //            targetDir = (_tempTargetHitbox.transform.position - aimAssistRotationControl.position);

            //            newDir = bspDir + (targetDir * 0.2f);


            //            aimAssistRotationControl.forward = newDir;
            //        }
            //        else
            //        {
            //            if (aimAssistRotationControl.transform.localRotation != originalBbulletSpawnPointRelativePos)
            //                aimAssistRotationControl.transform.localRotation = originalBbulletSpawnPointRelativePos;
            //            _targetPointPosition = Vector3.zero;
            //        }
            //    }
            //    else
            //    {
            //        if (aimAssistRotationControl.transform.localRotation != originalBbulletSpawnPointRelativePos)
            //            aimAssistRotationControl.transform.localRotation = originalBbulletSpawnPointRelativePos;
            //        _targetPointPosition = Vector3.zero;
            //    }
            //}
            else
            {
                if (aimAssistRotationControl.transform.localRotation != originalBbulletSpawnPointRelativePos)
                    aimAssistRotationControl.transform.localRotation = originalBbulletSpawnPointRelativePos;
                _targetPointPosition = Vector3.zero;
            }
        }
        else
        {
            _targetPointPosition = Vector3.zero;


            if (aimAssistRotationControl.transform.localRotation != originalBbulletSpawnPointRelativePos)
                aimAssistRotationControl.transform.localRotation = originalBbulletSpawnPointRelativePos;
        }
    }


    public void ResetRedReticule()
    {
        if (player.playerController.rid == 0 && player.isMine) Log.Print(() => "RESETREDRETICULE");
        redReticuleIsOn = false;
        closestHbToCrosshairCenter = null;
    }
}

