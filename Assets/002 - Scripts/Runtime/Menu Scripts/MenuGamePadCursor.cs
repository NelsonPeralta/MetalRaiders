using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using System.Linq;

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



        if (_gamePadCursor.activeSelf)
        {
            pointerData.position = _camera.WorldToScreenPoint(_gamePadCursor.transform.position);
            pointerData.position = _gamePadCursor.transform.position;
        }

        //if (rewiredPlayer.GetButtonDown("Switch Grenades"))
        {
            _eventSystemRaycastResults.Clear();
            EventSystem.current.RaycastAll(pointerData, _eventSystemRaycastResults);


            if (_eventSystemRaycastResults.Count != _preEentSystemRaycastResults.Count)
            {
                RepopulateButtons();

            }
            else
            {
                for (int i = 0; i < _eventSystemRaycastResults.Count; i++)
                {
                    if (_eventSystemRaycastResults[i].gameObject != _preEentSystemRaycastResults[i].gameObject)
                    {
                        RepopulateButtons();



                        break;
                    }
                }
            }





            if (rewiredPlayer.GetButtonDown("Switch Grenades"))
            {
                if (_buttonUnderCursor != null)
                    _buttonUnderCursor.onClick.Invoke();
            }





            //if (_eventSystemRaycastResults.Count > 0)
            //{
            //    if (_eventSystemRaycastResults != _preEentSystemRaycastResults)
            //    {
            //        print($"MenuGamePadCursor {_eventSystemRaycastResults.Count} {_preEentSystemRaycastResults.Count}");
            //        _preEentSystemRaycastResults.AddRange(_eventSystemRaycastResults);
            //        foreach (var r in _eventSystemRaycastResults)
            //        {
            //            if (r.gameObject.GetComponent<Button>())
            //                _buttonsFound.Add(r.gameObject.GetComponent<Button>());
            //        }
            //    }
            //}
            //_buttonsFound = (List<Button>)_eventSystemRaycastResults.Select(i => i.gameObject.GetComponent<Button>());

            //RaycastResult rr = raycastResults.Find(ni => ni.gameObject.GetComponent<Button>());

            //try { rr.gameObject.GetComponent<Button>().onClick.Invoke(); } catch { }
        }
        return;




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
            _gamePadCursor.transform.localPosition += new Vector3(((Mathf.Abs(rewiredPlayer.GetAxis("Move Horizontal")) > 0.15f) ? rewiredPlayer.GetAxis("Move Horizontal") * 25 : 0),
                ((Mathf.Abs(rewiredPlayer.GetAxis("Move Vertical")) > 0.15f) ? rewiredPlayer.GetAxis("Move Vertical") * 25 : 0), 0);

            //transform.localPosition += new Vector3(Mathf.Sign(rewiredPlayer.GetAxis("move_x")) * 3, Mathf.Sign(rewiredPlayer.GetAxis("move_y")) * 3, 0);
        }
    }




    [SerializeField] PointerEventData pointerData = new PointerEventData(EventSystem.current) { pointerId = -1, };
    [SerializeField] List<RaycastResult> _eventSystemRaycastResults = new List<RaycastResult>();
    [SerializeField] List<RaycastResult> _preEentSystemRaycastResults = new List<RaycastResult>();
    [SerializeField] List<Button> _buttonsFound = new List<Button>();
    public List<RaycastResult> RaycastMouse()
    {
        raycastResults.Clear(); _buttonsFound.Clear();
        pointerData = new PointerEventData(EventSystem.current) { pointerId = -1, };

        pointerData.position = _camera.WorldToScreenPoint(_gamePadCursor.transform.position);
        pointerData.position = _gamePadCursor.transform.position;

        _eventSystemRaycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, _eventSystemRaycastResults);

        foreach (var result in _eventSystemRaycastResults)
        {
            //_buttonsFound.Add(result.gameObject);
        }

        Debug.Log(_eventSystemRaycastResults.Count);

        return _eventSystemRaycastResults;
    }

    [SerializeField] Button _buttonUnderCursor;
    Sprite _buttonUnderCursorUnselectedSprite;


    void RepopulateButtons()
    {
        print($"MenuGamePadCursor {_eventSystemRaycastResults.Count} {_preEentSystemRaycastResults.Count}");

        _preEentSystemRaycastResults.Clear();
        _preEentSystemRaycastResults.AddRange(_eventSystemRaycastResults);
        _buttonsFound.Clear();




        foreach (var r in _eventSystemRaycastResults)
        {
            if (r.gameObject.GetComponent<Button>())
            {
                if (_buttonUnderCursor != null)
                    if (_buttonUnderCursor.transition == Selectable.Transition.SpriteSwap)
                        _buttonUnderCursor.GetComponent<Image>().sprite = _buttonUnderCursorUnselectedSprite;




                _buttonUnderCursor = r.gameObject.GetComponent<Button>();
                if (_buttonUnderCursor.transition == Selectable.Transition.SpriteSwap)
                {
                    _buttonUnderCursorUnselectedSprite = _buttonUnderCursor.GetComponent<Image>().sprite;
                    _buttonUnderCursor.GetComponent<Image>().sprite = _buttonUnderCursor.spriteState.highlightedSprite;
                }


                break;
            }


            if (_buttonUnderCursor != null)
                if (_buttonUnderCursor.transition == Selectable.Transition.SpriteSwap)
                    _buttonUnderCursor.GetComponent<Image>().sprite = _buttonUnderCursorUnselectedSprite;
        }
    }
}