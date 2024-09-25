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

    public bool trueHit { get { return _hitScreenPosList.Count > 0; } }




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


    [SerializeField] float xDiff, yDiff;
    [SerializeField] int xFact, yFact;
    [SerializeField] float xMag, yMag;

    [SerializeField] bool _obstruction;

    [SerializeField] GameManager.Team _playerTeam, _hitTeam;


    Vector3 _obsDir;
    float _obsDis;
    bool _friendly;


    // Update is called once per frame
    void Update()
    {
        if (!player.isMine)
            return;

        if (player.GetComponent<PlayerController>().activeControllerType == Rewired.ControllerType.Keyboard ||
            player.GetComponent<PlayerController>().activeControllerType == Rewired.ControllerType.Mouse)
            return;

        if (player.GetComponent<PlayerController>().isAiming)
            return;

        Ray();

        if (!_friendly)
        {
            CalculateDirection();
            Magnetism();
        }
    }

    void Ray()
    {
        _friendly = _obstruction = false;


        if (player && player.playerInventory && player.playerInventory.activeWeapon)
        {
            raycastRange = player.playerInventory.activeWeapon.currentRedReticuleRange;

            if (Physics.Raycast(player.mainCamera.transform.position, player.mainCamera.transform.forward, out hit, raycastRange * 1.5f, magnetismMask))
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



                            //if(movement.speed == 0)
                            _hitScreenPosList.Add(hit.transform.position); // When player does not move and target moves
                                                                           //_hitScreenPosList.Add(hit.point); // When player moves and target does not
                            if (_hitScreenPosList.Count > 4)
                                _hitScreenPosList.RemoveAt(0);
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
            yDiff = 0;
            xDiff = 0;
        }
        else if (_hitScreenPosList.Count == 4)
        {
            yDiff = 0;
            xDiff = 0;

            for (int i = 0; i < _hitScreenPosList.Count; i++)
            {
                if (i > 0)
                {
                    xDiff += (player.mainCamera.WorldToScreenPoint(_hitScreenPosList[i]).x) -
                         (player.mainCamera.WorldToScreenPoint(_hitScreenPosList[i - 1]).x);

                    yDiff += (player.mainCamera.WorldToScreenPoint(_hitScreenPosList[i]).y) -
                         (player.mainCamera.WorldToScreenPoint(_hitScreenPosList[i - 1]).y);
                }
            }

            try
            {
                //if (firstRayHit.GetComponent<ReticuleFriction>().player.GetComponent<Movement>().calulatedVelocity.magnitude <= .5)
                //{
                //    xDiff = 0;
                //    yDiff = 0;
                //}
            }
            catch { }
        }
    }

    void Magnetism()
    {
        // Increase yFact and xFact in Inspector to weaken magnetism



        //if (Mathf.Abs(yDiff) > 1f) // Prevents from working if there is the minimal movement in Y axis when moving horizontally
        if (Mathf.Abs(yDiff) > 0) // Prevents from working if there is the minimal movement in Y axis when moving horizontally
        {
            yMag = Mathf.Clamp((Mathf.Abs(yDiff) / yFact) * -Mathf.Sign(yDiff), -0.9f, 0.9f);
            player.playerCamera.verticalAxisTarget.Rotate(Vector3.right * yMag);
        }

        //if (Mathf.Abs(xDiff) > 0.5f)
        if (Mathf.Abs(xDiff) > 0)
        {
            xMag = Mathf.Clamp((Mathf.Abs(xDiff) / xFact) * Mathf.Sign(xDiff), -0.9f, 0.9f);
            player.playerCamera.horizontalAxisTarget.Rotate(Vector3.up * xMag);
        }
    }
}
