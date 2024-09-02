using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class PlayerWorldUIMarker : MonoBehaviour
{
    public bool seen
    {
        get { return _seen; }
        set
        {
            _seen = value;
            if (value) _seenCd = 0.2f;

            _redMarker.gameObject.SetActive(value && (GameManager.instance.teamMode == GameManager.TeamMode.None || (GameManager.instance.teamMode == GameManager.TeamMode.Classic && _player.team != GameManager.GetRootPlayer().team)));
            _text.gameObject.SetActive(value && (GameManager.instance.teamMode == GameManager.TeamMode.None || (GameManager.instance.teamMode == GameManager.TeamMode.Classic && _player.team != GameManager.GetRootPlayer().team)));
        }
    }

    public Player targetPlayer { get { return _targetPlayer; } }

    public TMP_Text text { get { return _text; } }
    public GameObject holder { get { return _holder; } }

    [SerializeField] Player _player;
    [SerializeField] int _controllerTarget;
    [SerializeField] Player _targetPlayer;

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

    private void Awake()
    {
        _player.OnPlayerTeamChanged += OnPlayerTeamDelegate;
        _player.OnPlayerHitPointsChanged += OnPlayerHitPointsChanged;

        _redMarker.gameObject.SetActive(false);
        _greenMarker.gameObject.SetActive(false);
        _text.gameObject.SetActive(false);
        _deadTag.gameObject.SetActive(false);
    }
    private void Start()
    {
        _holderScript.OnEnabledThis += OnHolderEnabled;

        _greenMarkerImage = _greenMarker.GetComponent<Image>();
    }

    private void Update()
    {
        if (!_targetPlayer)
        {
            try
            {
                _targetPlayer = GameManager.GetLocalPlayer(_controllerTarget);
            }
            catch { }
            return;
        }

        Vector3 targetPostition = new Vector3(_targetPlayer.transform.position.x,
                                        this.transform.position.y,
                                        _targetPlayer.transform.position.z);
        this.transform.LookAt(targetPostition);


        if (_seenCd > 0)
        {
            _seenCd -= Time.deltaTime;

            if (_seenCd < 0)
                if (seen) seen = false;
        }


        if (_targetPlayer)
        {
            _greenMarker.gameObject.SetActive(GameManager.instance.teamMode == GameManager.TeamMode.Classic && _player.team == _targetPlayer.team);


            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                if (_player.team == _targetPlayer.team) _text.gameObject.SetActive(true);

                if ((_player.isDead || _player.isRespawning) && _player.team == _targetPlayer.team) _deadTag.gameObject.SetActive(true); else _deadTag.gameObject.SetActive(false);
            }
            else
            {
                _deadTag.gameObject.SetActive(false);
            }
        }

        if (_targetPlayer)
        {
            //print($"PlayerWorldUIMarker {name} {_targetPlayer.isDead} {_targetPlayer.isRespawning} {_player.isDead} {_player.isRespawning}");
            if (_targetPlayer.isDead || _targetPlayer.isRespawning || _player.isDead || _player.isRespawning)
                holder.SetActive(false);
            else
                holder.SetActive(true);
        }


        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic && _player.team == _targetPlayer.team)
        {
            if (!_player.isDead && !_player.isRespawning)
            {
                if (_player.isTakingDamage)
                {
                    //_greenMarkerImage.color = new Color(255, 110, 0, 255);
                    ColorUtility.TryParseHtmlString(GameManager.colorDict["orange"], out _tCol);
                    _greenMarkerImage.color = _tCol;
                }
                else if (_player.playerController.isCurrentlyShootingForMotionTracker)
                {
                    _greenMarkerImage.color = Color.yellow;
                }
                else
                {
                    _greenMarkerImage.color = Color.green;
                }
            }
        }
    }

    public void OnPlayerTeamDelegate(Player player)
    {
        //if (GameManager.instance.teamMode.ToString().Contains("Classic"))
        //{
        //    Debug.Log("Player Marker");
        //    if (!player.isMine)
        //    {
        //        try
        //        {
        //            if (player.team != GameManager.GetRootPlayer().team)
        //            {
        //                _greenMarker.gameObject.SetActive(false);
        //                _holder.gameObject.SetActive(false);
        //            }
        //            if (player.team == GameManager.GetRootPlayer().team)
        //            {
        //                _greenMarker.gameObject.SetActive(true);
        //                _redMarker.gameObject.SetActive(false);
        //            }
        //        }
        //        catch (System.Exception e) { Debug.LogWarning(e); }
        //    }
        //}
        //else
        //{
        //    _greenMarker.gameObject.SetActive(false);

        //    _holder.gameObject.SetActive(false);
        //}
    }

    public void OnPlayerHitPointsChanged(Player player)
    {
        //try
        //{
        //    _text.text = _player.nickName + "\n" + _player.hitPoints;
        //}
        //catch (System.Exception e)
        //{

        //}
    }

    void OnHolderEnabled(PlayerWorldUIMarkerHolder playerWorldUIMarkerHolder)
    {
        //try
        //{
        //    Debug.Log("PlayerWorldUIMarker OnEnable");
        //    Debug.Log(_player.nickName);

        //    _targetPlayer = GameManager.instance.localPlayers[_controllerTarget];
        //    //_text.text = _player.nickName + "\n" + _player.hitPoints;
        //    _text.text = _player.nickName;
        //}
        //catch (System.Exception e)
        //{
        //    Debug.Log("Error with PlayerWorldUIMarker");
        //    Debug.LogWarning(e);

        //    gameObject.SetActive(false);
        //}
    }

    void OnPlayerDeath(Player player)
    {
        //_holder.SetActive(false);
    }
}
