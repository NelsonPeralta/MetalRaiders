using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerWorldUIMarker : MonoBehaviour
{
    public GameObject holder { get { return _holder; } }

    [SerializeField] Player _player;
    [SerializeField] int _controllerTarget;
    [SerializeField] Player _targetPlayer;

    [SerializeField] GameObject _holder;
    [SerializeField] PlayerWorldUIMarkerHolder _holderScript;
    [SerializeField] TMP_Text _text;
    [SerializeField] GameObject _redMarker;
    [SerializeField] GameObject _greenMarker;

    int damping = 1, tries = 0;

    private void Awake()
    {
        _player.OnPlayerTeamChanged += OnPlayerTeamDelegate;
        _player.OnPlayerHitPointsChanged += OnPlayerHitPointsChanged;
    }
    private void Start()
    {
        StartCoroutine(LateStart());
        _holderScript.OnEnabledThis += OnHolderEnabled;
    }

    private void Update()
    {
        if (!_targetPlayer)
            return;

        Vector3 targetPostition = new Vector3(_targetPlayer.transform.position.x,
                                        this.transform.position.y,
                                        _targetPlayer.transform.position.z);
        this.transform.LookAt(targetPostition);
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);

        try
        {
            _targetPlayer = GameManager.instance.localPlayers[_controllerTarget];
            _text.text = _player.nickName;

            _targetPlayer.OnPlayerDeath -= OnPlayerDeath;
            _targetPlayer.OnPlayerDeath += OnPlayerDeath;
        }
        catch (System.Exception e)
        {
            StartCoroutine(LateStart());
            tries++;

            if (tries == 15)
                gameObject.SetActive(false);
        }
    }

    public void OnPlayerTeamDelegate(Player player)
    {
        if (GameManager.instance.teamMode.ToString().Contains("Classic"))
        {
            Debug.Log("Player Marker");
            if (!player.isMine)
            {
                try
                {
                    if (player.team != GameManager.GetMyPlayer().team)
                    {
                        _greenMarker.gameObject.SetActive(false);
                        _holder.gameObject.SetActive(false);
                    }
                    if (player.team == GameManager.GetMyPlayer().team)
                    {
                        _greenMarker.gameObject.SetActive(true);
                        _redMarker.gameObject.SetActive(false);
                    }
                }
                catch (System.Exception e) { Debug.LogWarning(e); }
            }
        }
        else
        {
            _greenMarker.gameObject.SetActive(false);

            Debug.Log("PlayerWorldUIMarker");
            _holder.gameObject.SetActive(false);
        }
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
        try
        {
            Debug.Log("PlayerWorldUIMarker OnEnable");
            Debug.Log(_player.nickName);

            _targetPlayer = GameManager.instance.localPlayers[_controllerTarget];
            //_text.text = _player.nickName + "\n" + _player.hitPoints;
            _text.text = _player.nickName;
        }
        catch (System.Exception e)
        {
            Debug.Log("Error with PlayerWorldUIMarker");
            Debug.LogWarning(e);

            gameObject.SetActive(false);
        }
    }

    void OnPlayerDeath(Player player)
    {
        //_holder.SetActive(false);
    }
}
