using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class MenuGamePadCursor : MonoBehaviour
{
    [SerializeField] Camera _camera;


    public ControllerType controllerType
    {
        get { return _controllerType; }
        set
        {
            _preControllerType = _controllerType;
            _controllerType = value;


            if (value == ControllerType.Joystick && _preControllerType != ControllerType.Joystick)
            {
                _gamePadCursor.transform.position = Input.mousePosition;
            }
            else if (value != ControllerType.Joystick && _preControllerType == ControllerType.Joystick)
            {

            }
        }
    }
    public Rewired.Player rewiredPlayer;
    public int rwid;


    List<RaycastResult> raycastResults = new List<RaycastResult>();
    RaycastResult rr;

    [SerializeField] GameObject _gamePadCursor;
    [SerializeField] ControllerType _controllerType;

    ControllerType _preControllerType;


    private void Awake()
    {
        rewiredPlayer = ReInput.players.GetPlayer(0);
    }


    // Start is called before the first frame update
    void Start()
    {

    }


    private void Update()
    {
        controllerType = ReInput.controllers.GetLastActiveControllerType();
        _gamePadCursor.SetActive(controllerType == ControllerType.Joystick);
        Cursor.visible = !(controllerType == ControllerType.Joystick);


        if (rewiredPlayer.GetButtonDown("b_btn"))
        {
            Debug.Log("B BTN");

            {
                //MenuManager.Instance.OpenPreviousMenu();
            }
        }

        if (rewiredPlayer.GetButtonDown("Switch Grenades"))
        {
            Debug.Log("A BTN");
            raycastResults = RaycastMouse();

            RaycastResult rr = raycastResults.Find(ni => ni.gameObject.GetComponent<Button>());

            try { rr.gameObject.GetComponent<Button>().onClick.Invoke(); } catch { }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (controllerType == ControllerType.Joystick)
        {
            _gamePadCursor.transform.localPosition += new Vector3(((Mathf.Abs(rewiredPlayer.GetAxis("Move Horizontal")) > 0.15f) ? rewiredPlayer.GetAxis("Move Horizontal") * 10 : 0),
                ((Mathf.Abs(rewiredPlayer.GetAxis("Move Vertical")) > 0.15f) ? rewiredPlayer.GetAxis("Move Vertical") * 10 : 0), 0);

            //transform.localPosition += new Vector3(Mathf.Sign(rewiredPlayer.GetAxis("move_x")) * 3, Mathf.Sign(rewiredPlayer.GetAxis("move_y")) * 3, 0);
        }
    }


    public List<RaycastResult> RaycastMouse()
    {
        raycastResults.Clear();
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };

        pointerData.position = _camera.WorldToScreenPoint(_gamePadCursor.transform.position);
        pointerData.position = _gamePadCursor.transform.position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        Debug.Log(results.Count);

        return results;
    }
}