using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using UnityEngine.SceneManagement;

public class MenuGamePadCursor : MonoBehaviour
{
    [SerializeField] Camera _camera;


    public Player player { get { return _player; } set { _player = value; } }

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
                if (_buttonUnderCursor != null)
                {
                    _buttonUnderCursor.GetComponent<Image>().sprite = _buttonUnderCursorUnselectedSprite;
                    _buttonUnderCursor = null;
                }
            }
        }
    }




    public GameObject gamepadCursor { get { return _gamePadCursor; } }




    public Rewired.Player rewiredPlayer;
    public int rwid;


    List<RaycastResult> raycastResults = new List<RaycastResult>();
    RaycastResult rr;

    [SerializeField] GameObject _gamePadCursor;
    [SerializeField] ControllerType _controllerType;




    ControllerType _preControllerType;
    Player _player;




    [SerializeField] Vector3 _startPos, _cachedScreenPos;





    private void Awake()
    {
        rewiredPlayer = ReInput.players.GetPlayer(0);

    }


    // Start is called before the first frame update
    void Start()
    {
        _cachedScreenPos = _startPos;
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

            if(SceneManager.GetActiveScene().buildIndex > 0)
            {
                pointerData.position = _camera.WorldToScreenPoint(_gamePadCursor.transform.position);
            }
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

            if (SceneManager.GetActiveScene().buildIndex == 0 || (SceneManager.GetActiveScene().buildIndex > 0) && player)
            {
                _cachedScreenPos += new Vector3(((Mathf.Abs(rewiredPlayer.GetAxis("Move Horizontal")) > 0.15f) ? rewiredPlayer.GetAxis("Move Horizontal") * 25 : 0),
                    ((Mathf.Abs(rewiredPlayer.GetAxis("Move Vertical")) > 0.15f) ? rewiredPlayer.GetAxis("Move Vertical") * 25 : 0), 0);


                _cachedScreenPos.x = Mathf.Clamp(_cachedScreenPos.x, -900, 900);
                _cachedScreenPos.y = Mathf.Clamp(_cachedScreenPos.y, -500, 500);



                _gamePadCursor.transform.localPosition = _cachedScreenPos;
            }


            //if (_gamePadCursor.transform.position.x < 50)
            //    _gamePadCursor.transform.position += new Vector3(50, _gamePadCursor.transform.position.y, _gamePadCursor.transform.position.z);



            //_camera.WorldToViewportPoint



            //_gamePadCursor.transform.localPosition +=
            //    new Vector3(_gamePadCursor.transform.localPosition.x, ((Mathf.Abs(rewiredPlayer.GetAxis("Move Vertical")) > 0.15f) ? rewiredPlayer.GetAxis("Move Vertical") * 25 : 0), 0);


            //_gamePadCursor.transform.localPosition +=
            //    new Vector3(((Mathf.Abs(rewiredPlayer.GetAxis("Move Horizontal")) > 0.15f) ? rewiredPlayer.GetAxis("Move Horizontal") * 25 : 0), _gamePadCursor.transform.localPosition.y, 0);

            //transform.localPosition += new Vector3(Mathf.Sign(rewiredPlayer.GetAxis("move_x")) * 3, Mathf.Sign(rewiredPlayer.GetAxis("move_y")) * 3, 0);
        }
    }




    [SerializeField] PointerEventData pointerData = new PointerEventData(EventSystem.current) { pointerId = -1, };
    [SerializeField] List<RaycastResult> _eventSystemRaycastResults = new List<RaycastResult>();
    [SerializeField] List<RaycastResult> _preEentSystemRaycastResults = new List<RaycastResult>();
    public List<RaycastResult> RaycastMouse()
    {
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



    [SerializeField] List<GameObject> _rawHit = new List<GameObject>();
    [SerializeField] Button _buttonUnderCursor;
    Sprite _buttonUnderCursorUnselectedSprite;


    void RepopulateButtons()
    {
        print($"MenuGamePadCursor {_eventSystemRaycastResults.Count} {_preEentSystemRaycastResults.Count}");

        _preEentSystemRaycastResults.Clear(); _rawHit.Clear();
        _preEentSystemRaycastResults.AddRange(_eventSystemRaycastResults);


        _rawHit = _eventSystemRaycastResults.Select(r => r.gameObject).ToList();


        foreach (var r in _eventSystemRaycastResults)
        {
            if (r.gameObject.GetComponent<Button>() && r.gameObject.GetComponent<Button>().transition == Selectable.Transition.SpriteSwap)
            {
                if (_buttonUnderCursor != null)
                    if (_buttonUnderCursor.transition == Selectable.Transition.SpriteSwap)
                        _buttonUnderCursor.GetComponent<Image>().sprite = _buttonUnderCursorUnselectedSprite;




                _buttonUnderCursor = r.gameObject.GetComponent<Button>();
                _buttonUnderCursorUnselectedSprite = _buttonUnderCursor.GetComponent<Image>().sprite;
                _buttonUnderCursor.GetComponent<Image>().sprite = _buttonUnderCursor.spriteState.highlightedSprite;

                if (_buttonUnderCursor.GetComponent<EventTrigger>())
                    _buttonUnderCursor.GetComponent<EventTrigger>().OnPointerEnter(pointerData);


                break;
            }
            else
            {
                if (_buttonUnderCursor != null)
                {
                    _buttonUnderCursor.GetComponent<Image>().sprite = _buttonUnderCursorUnselectedSprite;
                    if (_buttonUnderCursor.GetComponent<EventTrigger>())
                        _buttonUnderCursor.GetComponent<EventTrigger>().OnPointerExit(pointerData);





                    _buttonUnderCursor = null;
                }
            }
        }
    }
}