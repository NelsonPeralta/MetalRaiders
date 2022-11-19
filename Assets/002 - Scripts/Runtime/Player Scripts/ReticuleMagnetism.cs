using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ReticuleMagnetism : MonoBehaviour
{
    public Player player;
    public Movement movement;

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
    [SerializeField] float xDiff;
    [SerializeField] float yDiff;

    [SerializeField] int xFact;
    [SerializeField] int yFact;


    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<PlayerController>().activeControllerType == Rewired.ControllerType.Keyboard ||
            player.GetComponent<PlayerController>().activeControllerType == Rewired.ControllerType.Mouse)
            return;

        if (player.GetComponent<PlayerController>().isAiming)
            return;

        Ray();
        CalculateDirection();
        Magnetism();
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
                {
                    if (hit.transform.gameObject.layer == 0)
                        firstRayHit = null;
                    else
                    {
                        firstRayHit = hit.transform.gameObject;
                    }

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
            float yMag = (Mathf.Abs(yDiff) / yFact) * -Mathf.Sign(yDiff);

            Vector3 locRot = player.mainCamera.transform.localRotation.eulerAngles;
            locRot += new Vector3(yMag, 0, 0);

            //cameraParent.transform.localRotation = Quaternion.Euler(locRot);
            player.playerInventory.transform.Rotate(Vector3.right * yMag);
        }

        //if (Mathf.Abs(xDiff) > 0.5f)
        if (Mathf.Abs(xDiff) > 0)
        {
            float xMag = (Mathf.Abs(xDiff) / xFact) * Mathf.Sign(xDiff);
            player.transform.Rotate(Vector3.up * xMag);
        }
    }
}
