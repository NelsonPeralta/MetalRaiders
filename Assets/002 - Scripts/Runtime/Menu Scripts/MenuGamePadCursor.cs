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
    static Vector3 START_POS = new Vector3(-500, -300);


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
                _cachedScreenPos = START_POS;
                Cursor.lockState = CursorLockMode.Locked; // Must Unlock Cursor so it can detect buttons
                Cursor.visible = false;
                _gamePadCursor.transform.localPosition = _cachedScreenPos;

                _gamePadCursor.gameObject.SetActive(true);
            }
            else if (value != ControllerType.Joystick && _preControllerType == ControllerType.Joystick)
            {
                Cursor.lockState = CursorLockMode.None; // Must Unlock Cursor so it can detect buttons
                Cursor.visible = true;
                _cachedScreenPos = START_POS;
                _gamePadCursor.transform.localPosition = START_POS;
                _gamePadCursor.gameObject.SetActive(false);


                ClearAllGamepadData();
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
    bool _isReady;



    [SerializeField] Vector3 _startPos, _cachedScreenPos;

    public Rewired.Player rewiredPlayer2, rewiredPlayer3, rewiredPlayer4;




    private void Awake()
    {
        _gamePadCursor.SetActive(false);
        rewiredPlayer = ReInput.players.GetPlayer(0);
        rewiredPlayer2 = ReInput.players.GetPlayer(1);
        rewiredPlayer3 = ReInput.players.GetPlayer(2);
        rewiredPlayer4 = ReInput.players.GetPlayer(3);
    }


    private void Update()
    {

        if (_isReady)
        {


            controllerType = ReInput.controllers.GetLastActiveControllerType();


            if (_gamePadCursor.activeSelf)
            {
                pointerData.position = _camera.WorldToScreenPoint(_gamePadCursor.transform.position);
                pointerData.position = _gamePadCursor.transform.position;

                if (SceneManager.GetActiveScene().buildIndex > 0)
                {
                    pointerData.position = _camera.WorldToScreenPoint(_gamePadCursor.transform.position);
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





                    if (rewiredPlayer.GetButtonDown("mark"))
                    {
                        if (_buttonUnderCursor != null)
                            _buttonUnderCursor.onClick.Invoke();
                    }
                }
            }




            if (SceneManager.GetActiveScene().buildIndex == 0 && CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.Private && GameManager.instance.connection == GameManager.Connection.Local)
            {
                if (rewiredPlayer.GetButtonDown("Jump") || rewiredPlayer.GetButtonDown("Melee"))
                {
                    Launcher.instance.ChangeTeamOfLocalPlayer(0);
                }

                if (rewiredPlayer2.GetButtonDown("Jump") || rewiredPlayer2.GetButtonDown("Melee"))
                {
                    Launcher.instance.ChangeTeamOfLocalPlayer(1);
                }

                if (rewiredPlayer3.GetButtonDown("Jump") || rewiredPlayer3.GetButtonDown("Melee"))
                {
                    Launcher.instance.ChangeTeamOfLocalPlayer(2);
                }

                if (rewiredPlayer4.GetButtonDown("Jump") || rewiredPlayer4.GetButtonDown("Melee"))
                {
                    Launcher.instance.ChangeTeamOfLocalPlayer(3);
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_isReady)
        {

            if (controllerType == ControllerType.Joystick && _gamePadCursor.activeSelf)
            {

                if (SceneManager.GetActiveScene().buildIndex == 0 || (SceneManager.GetActiveScene().buildIndex > 0) && player)
                {
                    if (SceneManager.GetActiveScene().buildIndex == 0 ||
                        SceneManager.GetActiveScene().buildIndex > 0 && player && player.playerController.activeControllerType != ControllerType.Joystick ||
                        (SceneManager.GetActiveScene().buildIndex > 0 && player && player.playerController.activeControllerType == ControllerType.Joystick && player.playerController.pauseMenuOpen && !player.allPlayerScripts.scoreboardManager.scoreboardOpen))
                    {
                        _cachedScreenPos += new Vector3(((Mathf.Abs(rewiredPlayer.GetAxis("Move Horizontal")) > 0.15f) ? rewiredPlayer.GetAxis("Move Horizontal") * 20 * (rewiredPlayer.GetButton("Shoot") ? 2 : 1) * (rewiredPlayer.GetButton("Throw Grenade") ? 0.5f : 1) : 0),
                            ((Mathf.Abs(rewiredPlayer.GetAxis("Move Vertical")) > 0.15f) ? rewiredPlayer.GetAxis("Move Vertical") * 20 * (rewiredPlayer.GetButton("Shoot") ? 2 : 1) * (rewiredPlayer.GetButton("Throw Grenade") ? 0.5f : 1) : 0), 0);
                    }
                    else if (SceneManager.GetActiveScene().buildIndex > 0 && player && player.playerController.activeControllerType == ControllerType.Joystick && !player.playerController.pauseMenuOpen && player.allPlayerScripts.scoreboardManager.scoreboardOpen)
                    {
                        _cachedScreenPos += new Vector3(((Mathf.Abs(rewiredPlayer.GetAxis("Mouse X")) > 0.15f) ? rewiredPlayer.GetAxis("Mouse X") * 20 * (rewiredPlayer.GetButton("Shoot") ? 2 : 1) * (rewiredPlayer.GetButton("Throw Grenade") ? 0.5f : 1) : 0),
                           ((Mathf.Abs(rewiredPlayer.GetAxis("Mouse Y")) > 0.15f) ? rewiredPlayer.GetAxis("Mouse Y") * 20 * (rewiredPlayer.GetButton("Shoot") ? 2 : 1) * (rewiredPlayer.GetButton("Throw Grenade") ? 0.5f : 1) : 0), 0);
                    }


                    _cachedScreenPos.x = Mathf.Clamp(_cachedScreenPos.x, -900, 900);
                    _cachedScreenPos.y = Mathf.Clamp(_cachedScreenPos.y, -500, 500);



                    _gamePadCursor.transform.localPosition = _cachedScreenPos;
                }
            }
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
        if (_isReady)
        {


            print($"MenuGamePadCursor {_eventSystemRaycastResults.Count} {_preEentSystemRaycastResults.Count}");



            foreach (var r in _preEentSystemRaycastResults)
                if (r.gameObject.GetComponent<Button>() && r.gameObject.GetComponent<Button>().transition == Selectable.Transition.SpriteSwap)
                {
                    r.gameObject.GetComponent<Image>().sprite = _buttonUnderCursorUnselectedSprite;
                    if (r.gameObject.GetComponent<EventTrigger>())
                    {
                        print($"MenuGamePadCursor OnPointerExit {r.gameObject.name} {r.gameObject.activeSelf}");

                        if (SceneManager.GetActiveScene().buildIndex == 0 && MenuManager.Instance.GetOpenMenu().Equals("armory") && !r.gameObject.activeSelf)
                        {
                            // do nothing
                        }
                        else
                        {
                            r.gameObject.GetComponent<EventTrigger>().OnPointerExit(pointerData);
                        }

                    }
                }



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
                        {
                            print($"MenuGamePadCursor OnPointerExit {_buttonUnderCursor.gameObject.name} {_buttonUnderCursor.gameObject.activeSelf}");

                            if (SceneManager.GetActiveScene().buildIndex == 0 && MenuManager.Instance.GetOpenMenu().Equals("armory") && !_buttonUnderCursor.gameObject.activeSelf)
                            {
                                // do nothing
                            }
                            else
                            {
                                _buttonUnderCursor.gameObject.GetComponent<EventTrigger>().OnPointerExit(pointerData);
                            }
                        }





                        _buttonUnderCursor = null;
                    }
                }
            }
        }
    }

















    public void GetReady(ControllerType ct)
    {

        _controllerType = _preControllerType = ct;


        if (ct == ControllerType.Joystick)
        {
            Cursor.lockState = CursorLockMode.Locked; // Must Unlock Cursor so it can detect buttons
            Cursor.visible = false;


            _cachedScreenPos = START_POS;
            _gamePadCursor.transform.localPosition = _cachedScreenPos;
            _gamePadCursor.gameObject.SetActive(true);
        }
        else
        {
            _gamePadCursor.gameObject.SetActive(false);
            _cachedScreenPos = START_POS;
            _gamePadCursor.transform.localPosition = START_POS;

            if (_buttonUnderCursor != null)
            {
                _buttonUnderCursor.GetComponent<Image>().sprite = _buttonUnderCursorUnselectedSprite;
                _buttonUnderCursor = null;
            }

            Cursor.lockState = CursorLockMode.None; // Must Unlock Cursor so it can detect buttons
            Cursor.visible = true;
        }

        _isReady = true;
    }

    public void CloseFromPauseMenu()
    {
        _isReady = false;

        Cursor.lockState = CursorLockMode.Locked; // Must Unlock Cursor so it can detect buttons
        Cursor.visible = false;



        _gamePadCursor.gameObject.SetActive(false);
        _cachedScreenPos = START_POS;
        _gamePadCursor.transform.localPosition = START_POS;

        ClearAllGamepadData();
    }


    void ClearAllGamepadData()
    {
        _preEentSystemRaycastResults.Clear(); _rawHit.Clear();
        _eventSystemRaycastResults.Clear();

        if (_buttonUnderCursor != null)
        {
            _buttonUnderCursor.GetComponent<Image>().sprite = _buttonUnderCursorUnselectedSprite;
            _buttonUnderCursor = null;
        }
    }
}