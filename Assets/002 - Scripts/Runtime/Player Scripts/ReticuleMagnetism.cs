using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticuleMagnetism : MonoBehaviour
{
    public GameObject magnetismHit
    {
        get { return _magnetismHit; }
        set
        {
            var previousValue = magnetismHit;

            if (value == null)
            {
                //aimAssist.ResetRedReticule();
            }

            _magnetismHit = value;
        }
    }

    public bool trueHit { get { return _trueHit; } }




    public Player player;
    public PlayerMovement movement;

    RaycastHit hit, _obsHit;
    public LayerMask magnetismMask, obstructionMask;
    public float raycastRange = 50;


    [SerializeField] GameObject _magnetismHit, _obstructionHit;
    [SerializeField] float _distFromMagTrans;

    [SerializeField] List<Vector3> _hitScreenPosList;
    [SerializeField] Vector3 _previousHitScreenPos;
    [SerializeField] Vector3 _newHitScreenPos;


    [SerializeField] float xMagDir, yMagDir;
    [SerializeField] int xFact, yFact;
    [SerializeField] float xMag, yMag;
    [SerializeField] float _distanceFact;

    [SerializeField] bool _obstruction;

    [SerializeField] GameManager.Team _playerTeam, _hitTeam;


    Vector3 _obsDir;
    float _obsDis;
    bool _friendly, _trueHit;


    // Update is called once per frame
    void Update()
    {
        if (!player.isMine)
            return;

        if (player.GetComponent<PlayerController>().activeControllerType == Rewired.ControllerType.Keyboard ||
            player.GetComponent<PlayerController>().activeControllerType == Rewired.ControllerType.Mouse)
            return;

        //if (player.GetComponent<PlayerController>().isAiming)
        //    return;

        Ray();

        if (!_friendly && _distFromMagTrans > 0)
        {
            CalculateDirection();
            Magnetism();
        }
        else
        {
            yMagDir = 0;
            xMagDir = 0;
        }
    }




    void Ray()
    {
        _distFromMagTrans = 0;
        _friendly = _obstruction = _trueHit = false;


        if (player && player.playerInventory && player.playerInventory.activeWeapon)
        {
            raycastRange = player.playerInventory.activeWeapon.currentRedReticuleRange * 3;

            if (player.playerController.rid == 0)
            {
                //print($"position of my camera is {player.mainCamera.transform.position} and direction is {player.mainCamera.transform.forward}");
                //print($"the point is {player.mainCamera.transform.position + (2.4f * player.mainCamera.transform.forward)}");
            }

            if (Physics.Raycast(player.mainCamera.transform.position
                + ((GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || player.playerInventory.isHoldingHeavy) ? (-PlayerCamera.THIRD_PERSON_LOCAL_OFFSET.z * player.mainCamera.transform.forward) : Vector3.zero),
                player.mainCamera.transform.forward, out hit, raycastRange, magnetismMask))
            {
                if (hit.transform.root.gameObject != player.gameObject)
                {
                    magnetismHit = hit.transform.gameObject;

                    _obsDir = (hit.point - player.mainCamera.transform.position).normalized;
                    _obsDis = Vector3.Distance(hit.point, player.mainCamera.transform.position);

                    if (!Physics.Raycast(player.mainCamera.transform.position, _obsDir, out _obsHit, _obsDis, obstructionMask))
                    {
                        ReticuleFriction rf = magnetismHit.GetComponent<ReticuleFriction>(); if (rf == null) try { rf = hit.transform.GetComponent<Player>().reticuleFriction; } catch { }

                        if (rf)
                        {
                            _distFromMagTrans = Vector3.Distance(rf.transform.position, transform.position);

                            _playerTeam = player.team;
                            if (rf.player) _hitTeam = rf.player.team;


                            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                                if (rf.player && (rf.player.team == player.team))
                                {
                                    _friendly = true;
                                    return;
                                }


                            _trueHit = true;
                            if (!player.playerController.isAiming)
                            {
                                _hitScreenPosList.Add(hit.transform.position); // When player does not move and target moves
                                                                               //_hitScreenPosList.Add(hit.point); // When player moves and target does not
                                if (_hitScreenPosList.Count > 4)
                                    _hitScreenPosList.RemoveAt(0);
                            }
                        }
                    }
                    else
                    {
                        _obstructionHit = _obsHit.transform.gameObject;
                        _obstruction = true;

                        magnetismHit = null;
                        _hitScreenPosList.Clear();
                    }
                }
                else
                {
                    magnetismHit = null;

                }
            }
            else
            {
                magnetismHit = null;
                _hitScreenPosList.Clear();
            }
        }
    }

    void CalculateDirection()
    {
        if (_hitScreenPosList.Count == 0)
        {
            yMagDir = 0;
            xMagDir = 0;
        }
        else if (_hitScreenPosList.Count == 4)
        {
            yMagDir = 0;
            xMagDir = 0;

            for (int i = 0; i < _hitScreenPosList.Count; i++)
            {
                if (i > 0)
                {
                    xMagDir += (player.mainCamera.WorldToScreenPoint(_hitScreenPosList[i]).x) -
                         (player.mainCamera.WorldToScreenPoint(_hitScreenPosList[i - 1]).x);

                    yMagDir += (player.mainCamera.WorldToScreenPoint(_hitScreenPosList[i]).y) -
                         (player.mainCamera.WorldToScreenPoint(_hitScreenPosList[i - 1]).y);
                }
            }
        }
    }






    void Magnetism()
    {
        // Increase yFact and xFact in Inspector to weaken magnetism
        _distanceFact = Mathf.Clamp(_distFromMagTrans / raycastRange, 1, 0);


        //if (Mathf.Abs(yDiff) > 1f) // Prevents from working if there is the minimal movement in Y axis when moving horizontally
        if (Mathf.Abs(yMagDir) > 0) // Prevents from working if there is the minimal movement in Y axis when moving horizontally
        {
            yMag = Mathf.Clamp((Mathf.Abs(yMagDir) / (yFact * _distanceFact)) * -Mathf.Sign(yMagDir), -0.9f, 0.9f);
            //player.playerCamera.verticalAxisTarget.Rotate(Vector3.right * yMag);

            player.playerCamera.AddToUpDownRotation(yMag);
        }

        //if (Mathf.Abs(xDiff) > 0.5f)
        if (Mathf.Abs(xMagDir) > 0)
        {
            xMag = Mathf.Clamp((Mathf.Abs(xMagDir) / (xFact * _distanceFact)) * Mathf.Sign(xMagDir), -0.9f, 0.9f);
            //player.playerCamera.horizontalAxisTarget.Rotate(Vector3.up * xMag);

            player.playerCamera.AddToLeftRightRotation(xMag);
        }
    }
}
