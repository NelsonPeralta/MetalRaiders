using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticuleMagnetism : MonoBehaviour
{
    public Player player;
    public PlayerMovement movement;

    RaycastHit hit;
    public LayerMask layerMask;
    public float raycastRange = 1000;


    [SerializeField] GameObject _firstRayHit;
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

            _firstRayHit = value;
        }
    }

    [SerializeField] List<Vector3> _hitScreenPosList;
    [SerializeField] Vector3 _previousHitScreenPos;
    [SerializeField] Vector3 _newHitScreenPos;


    [SerializeField] float xDiff, yDiff;
    [SerializeField] int xFact, yFact;
    [SerializeField] float xMag, yMag;

    [SerializeField] LayerMask obstructionMask;
    [SerializeField] bool _obstruction;


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
        _friendly = false;
        try
        { raycastRange = player.playerInventory.activeWeapon.currentRedReticuleRange; }
        catch (System.Exception) { }

        if (Physics.Raycast(player.mainCamera.transform.position, player.mainCamera.transform.forward, out hit, raycastRange, layerMask))
        {
            if (hit.transform.root.gameObject != player.gameObject)
            {
                {
                    if (hit.transform.gameObject.layer == 0)
                        firstRayHit = null;
                    else
                    {
                        firstRayHit = hit.transform.gameObject;
                    }



                    try
                    {

                        ReticuleFriction rf = firstRayHit.GetComponent<ReticuleFriction>();


                        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                            if (rf.player && (rf.player.team == player.team))
                            {
                                _friendly = true;
                                return;
                            }

                        _obstruction = false;
                        _obsDir = (firstRayHit.transform.position - player.mainCamera.transform.position).normalized;
                        _obsDis = Vector3.Distance(player.mainCamera.transform.position, firstRayHit.transform.position);

                        if (Physics.Raycast(player.mainCamera.transform.position, _obsDir, _obsDis, obstructionMask))
                            _obstruction = true;

                        if (_obstruction)
                        {
                            rf = null;
                            _hitScreenPosList.Clear();
                            return;
                        }

                    }
                    catch { }









                    //if(movement.speed == 0)
                    _hitScreenPosList.Add(hit.transform.position); // When player does not move and target moves
                                                                   //_hitScreenPosList.Add(hit.point); // When player moves and target does not
                    if (_hitScreenPosList.Count > 4)
                        _hitScreenPosList.RemoveAt(0);
                }
            }
        }
        else
        {
            firstRayHit = null;
            _hitScreenPosList.Clear();
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
        //if (Mathf.Abs(yDiff) > 1f) // Prevents from working if there is the minimal movement in Y axis when moving horizontally
        if (Mathf.Abs(yDiff) > 0) // Prevents from working if there is the minimal movement in Y axis when moving horizontally
        {
            yMag = Mathf.Clamp((Mathf.Abs(yDiff) / yFact) * -Mathf.Sign(yDiff), -1, 1);
            player.playerCamera.verticalAxisTarget.Rotate(Vector3.right * yMag);
        }

        //if (Mathf.Abs(xDiff) > 0.5f)
        if (Mathf.Abs(xDiff) > 0)
        {
            xMag = Mathf.Clamp((Mathf.Abs(xDiff) / xFact) * Mathf.Sign(xDiff), -1, 1);
            player.playerCamera.horizontalAxisTarget.Rotate(Vector3.up * xMag);
        }
    }
}
