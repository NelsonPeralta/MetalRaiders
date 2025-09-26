using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWorldUIMarker : MonoBehaviour
{
    public bool seen
    {
        get { return _seen; }
        set
        {
            _seen = value;
            if (value) _seenCd = 0.2f;

            _redMarker.gameObject.SetActive(value && (GameManager.instance.teamMode == GameManager.TeamMode.None || (GameManager.instance.teamMode == GameManager.TeamMode.Classic && _rootPlayer.team != GameManager.GetRootPlayer().team)));
            _text.gameObject.SetActive(value && (GameManager.instance.teamMode == GameManager.TeamMode.None || (GameManager.instance.teamMode == GameManager.TeamMode.Classic && _rootPlayer.team != GameManager.GetRootPlayer().team)));
        }
    }

    public Player lookAtThisPlayer { get { return _lookAtThisPlayer; } }

    public TMP_Text text { get { return _text; } }
    public GameObject holder { get { return _holder; } }

    [SerializeField] Player _rootPlayer;
    [SerializeField] int _controllerTarget;
    [SerializeField] Player _lookAtThisPlayer;

    [SerializeField] GameObject _holder, _deadTag;
    [SerializeField] PlayerWorldUIMarkerHolder _holderScript;
    [SerializeField] TMP_Text _text;
    [SerializeField] GameObject _redMarker;
    [SerializeField] GameObject _greenMarker;
    [SerializeField] Image _greenMarkerImage;
    [SerializeField] bool _seen;
    [SerializeField] float _seenCd;

    int damping = 1, tries = 0;
    Color _tCol;




    // add near other private fields
    private Color _orangeColor;
    private Color _fallbackOrange = new Color(1f, 110f / 255f, 0f, 1f); // cached fallback
    private bool _orangeColorParsed = false;


    private void Awake()
    {
        _rootPlayer = transform.root.GetComponent<Player>();

        _rootPlayer.OnPlayerIdAssigned += OnPlayerIdAssigned;

        _redMarker.gameObject.SetActive(false);
        _greenMarker.gameObject.SetActive(false);
        _text.gameObject.SetActive(false);
        _deadTag.gameObject.SetActive(false);
    }
    private void Start()
    {

        _greenMarkerImage = _greenMarker.GetComponent<Image>();

        // parse and cache the "orange" color once (avoid parsing every frame)
        if (GameManager.colorDict != null && GameManager.colorDict.TryGetValue("orange", out string hex))
        {
            if (ColorUtility.TryParseHtmlString(hex, out _orangeColor))
                _orangeColorParsed = true;
            Log.Print(() =>"_orangeColorParsed parsed");
        }
    }

    private void Update()
    {
        if (!CurrentRoomManager.instance.gameStarted) return;

        if (_lookAtThisPlayer == null)
        {
            _lookAtThisPlayer = GameManager.GetLocalPlayer(_controllerTarget);
            if (_lookAtThisPlayer == null)
                return;
        }
        else
        {
            // small micro-opt: avoid redundant SetActive calls on the child
            var child0 = transform.childCount > 0 ? transform.GetChild(0) : null;
            if (child0 != null && child0.gameObject.activeSelf != _lookAtThisPlayer.isAlive)
                child0.gameObject.SetActive(_lookAtThisPlayer.isAlive);
        }


        Vector3 targetPostition = new Vector3(_lookAtThisPlayer.transform.position.x,
                                        this.transform.position.y,
                                        _lookAtThisPlayer.transform.position.z);
        this.transform.LookAt(targetPostition);


        if (_seenCd > 0)
        {
            _seenCd -= Time.deltaTime;

            if (_seenCd < 0)
                if (seen) seen = false;
        }


        if (_lookAtThisPlayer)
        {
            //_greenMarker.gameObject.SetActive(GameManager.instance.teamMode == GameManager.TeamMode.Classic && _rootPlayer.team == _lookAtThisPlayer.team);

            bool greenShould = GameManager.instance.teamMode == GameManager.TeamMode.Classic && _rootPlayer.team == _lookAtThisPlayer.team;
            if (_greenMarker != null && _greenMarker.activeSelf != greenShould)
                _greenMarker.SetActive(greenShould);

            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                //if (_rootPlayer.team == _lookAtThisPlayer.team) _text.gameObject.SetActive(true);
                if (_text != null && !_text.gameObject.activeSelf)
                    _text.gameObject.SetActive(true);


                if ((_rootPlayer.isDead || _rootPlayer.isRespawning) && _rootPlayer.team == _lookAtThisPlayer.team)
                {
                    if (!_deadTag.gameObject.activeSelf)
                        _deadTag.gameObject.SetActive(true);
                }
                else if (!_deadTag.gameObject.activeSelf)
                {
                    _deadTag.gameObject.SetActive(false);
                }
            }
            else
            {
                if (!_deadTag.gameObject.activeSelf)
                    _deadTag.gameObject.SetActive(false);
            }
        }

        if (_lookAtThisPlayer)
        {
            //PrintOnlyInEditor.Log($"PlayerWorldUIMarker {name} {_targetPlayer.isDead} {_targetPlayer.isRespawning} {_player.isDead} {_player.isRespawning}");
            if (_lookAtThisPlayer.isDead || _lookAtThisPlayer.isRespawning || _rootPlayer.isDead || _rootPlayer.isRespawning)
                holder.SetActive(false);
            else
                holder.SetActive(true);
        }


        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic && _rootPlayer.team == _lookAtThisPlayer.team)
        {
            if (!_rootPlayer.isDead && !_rootPlayer.isRespawning)
            {
                if (_rootPlayer.isTakingDamage)
                {
                    if (_orangeColorParsed)
                    {
                        if (_greenMarkerImage.color != _orangeColor)
                            _greenMarkerImage.color = _orangeColor;
                    }
                    else
                    {
                        // fallback (cached fallbackColor struct avoids realloc)
                        if (_greenMarkerImage.color != _fallbackOrange)
                            _greenMarkerImage.color = _fallbackOrange;
                    }
                }
                else if (_rootPlayer.playerController.isCurrentlyShootingForMotionTracker)
                {
                    if (_greenMarkerImage.color != Color.yellow)
                        _greenMarkerImage.color = Color.yellow;
                }
                else
                {
                    if (_greenMarkerImage.color != Color.green)
                        _greenMarkerImage.color = Color.green;
                }
            }
        }
    }


    void OnPlayerIdAssigned(Player p)
    {
        if (p.isMine && p.rid == _controllerTarget) gameObject.SetActive(false);
    }
}
