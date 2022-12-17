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
    [SerializeField] TMP_Text _text;
    [SerializeField] GameObject _redMarker;
    [SerializeField] GameObject _greenMarker;

    int damping = 1;

    private void Awake()
    {
        _player.OnPlayerTeamChanged += OnPlayerTeamDelegate;
    }
    private void Start()
    {
        StartCoroutine(LateStart());
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
        yield return new WaitForEndOfFrame();

        try
        {
            _targetPlayer = GameManager.instance.localPlayers[_controllerTarget];
            _text.text = _player.nickName;

            _targetPlayer.OnPlayerDeath -= OnPlayerDeath;
            _targetPlayer.OnPlayerDeath += OnPlayerDeath;
        }
        catch (System.Exception e) { Debug.LogWarning(e); gameObject.SetActive(false); }
    }

    public void OnPlayerTeamDelegate(Player player)
    {
        if (GameManager.instance.teamMode.ToString().Contains("Classic"))
        {
            Debug.Log("Player Marker");
            if (!player.isMine)
            {
                if (player.team != GameManager.GetMyPlayer().team)
                {
                    _greenMarker.gameObject.SetActive(false);
                    _holder.gameObject.SetActive(false);
                }
                if (player.team == GameManager.GetMyPlayer().team)
                {
                    _redMarker.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            _greenMarker.gameObject.SetActive(false);

            Debug.Log("PlayerWorldUIMarker");
            _holder.gameObject.SetActive(false);
        }
    }

    void OnPlayerDeath(Player player)
    {
        _holder.SetActive(false);
    }
}
