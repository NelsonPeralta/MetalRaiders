using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hill : MonoBehaviour
{
    public delegate void HillEvent(Hill hill);
    public HillEvent OnPlayerEntered;

    public List<Player> playersInRange
    {
        get { return _playersInRange; }
        set
        {
            int _previousPlayerCount = _playersInRange.Count;
            bool _previousState = _contested;

            _contested = false;
            _playersInRange = value;

            if (_previousPlayerCount != _playersInRange.Count)
            {
                _grey.SetActive(false);
                _red.SetActive(false);
                _blue.SetActive(false);

                if (playersInRange.Count == 0)
                {
                    _grey.SetActive(true);
                }
                else if (playersInRange.Count == 1)
                {
                    _timer = 0;



                    if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                    {
                        _red.SetActive(!playersInRange[0].isMine);
                        _blue.SetActive(playersInRange[0].isMine);
                    }
                    else if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                    {
                        _red.SetActive(playersInRange[0].team == GameManager.Team.Red);
                        _blue.SetActive(playersInRange[0].team == GameManager.Team.Blue);
                    }
                }
                else if (playersInRange.Count > 1)
                {
                    if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                    {
                        _contested = true;
                        _timer = 0;

                        _red.SetActive(true);


                    }
                    else
                    {
                        for (int i = 0; i < playersInRange.Count; i++)
                            if (playersInRange[i].team != playersInRange[0].team)
                            {
                                _contested = true;
                                _timer = 0;
                                _grey.SetActive(true);
                            }
                            else
                            {
                                _red.SetActive(playersInRange[0].team == GameManager.Team.Red);
                                _blue.SetActive(playersInRange[0].team == GameManager.Team.Blue);
                            }
                    }
                }

                if (_previousState != _contested || _previousPlayerCount == 0)
                    if (_contested)
                    {
                        _audioSource.clip = _hillContested;
                        GameManager.GetRootPlayer().announcer.AddClip(_hillContested);
                    }
                    else
                    {
                        _audioSource.clip = _hillControlled;
                        GameManager.GetRootPlayer().announcer.AddClip(_hillControlled);
                    }
            }
        }
    }

    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _announcement;
    [SerializeField] AudioClip _hillControlled;
    [SerializeField] AudioClip _hillContested;
    [SerializeField] AudioClip _hillMoved;

    [SerializeField] GameObject _grey;
    [SerializeField] GameObject _red;
    [SerializeField] GameObject _blue;

    [SerializeField] List<Player> _playersInRange = new List<Player>();
    [SerializeField] Player _closestPlayer;

    [SerializeField] GameObject _hillVfxHolder;

    float _timer;
    bool _contested;


    private void Start()
    {
        if (GameManager.instance.gameType != GameManager.GameType.Hill)
        {
            gameObject.SetActive(false);
        }
    }



    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerCapsule>())
        {
            Player p = other.transform.root.GetComponent<Player>();

            if (!p.isDead && !p.isRespawning && !playersInRange.Contains(p))
            {
                p.OnPlayerDeath += OnPlayerDeath;
                List<Player> list = new List<Player>(playersInRange);
                list.Add(p);

                playersInRange = list;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerCapsule>())
        {
            Player p = other.transform.root.GetComponent<Player>();

            if (playersInRange.Contains(p))
            {
                List<Player> list = new List<Player>(playersInRange);
                list.Remove(p);

                playersInRange = list;
            }
        }
    }

    void OnPlayerDeath(Player player)
    {
        List<Player> list = new List<Player>(playersInRange);
        list.Remove(player);

        playersInRange = list;
    }

    private void Update()
    {
        if (!CurrentRoomManager.instance.gameStarted) return;

        _hillVfxHolder.SetActive(!GameManager.GetRootPlayer().playerController.cameraIsFloating);



        if (!_contested && playersInRange.Count > 0)
        {
            if (_timer < 1)
            {
                _timer += Time.deltaTime;

                if (_timer >= 1)
                {
                    if (playersInRange[0].isMine && CurrentRoomManager.instance.gameStarted)
                    {
                        NetworkGameManager.instance.AddPlayerPoint(playersInRange[0].photonId);
                    }

                    _timer = 0;
                }
            }
        }
    }
}
