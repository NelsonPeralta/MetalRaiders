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
            int p = _playersInRange.Count;
            _playersInRange = value;

            if (p != _playersInRange.Count)
            {
                _grey.SetActive(false);
                _red.SetActive(false);
                _blue.SetActive(false);

                if (playersInRange.Count == 0)
                {
                    _grey.SetActive(true);
                    //_red.SetActive(false);
                    //_blue.SetActive(false);
                }
                else if (playersInRange.Count == 1)
                {
                    _timer = 0;
                    if (playersInRange[0].isMine)
                    {
                        //_red.SetActive(false);
                        _blue.SetActive(true);

                        _audioSource.clip = _hillControlled;
                        GameManager.GetRootPlayer().announcer.AddClip(_hillControlled);
                    }
                    else
                    {
                        _red.SetActive(true);
                        //_blue.SetActive(false);
                    }
                }
                else if (playersInRange.Count > 1)
                {
                    _grey.SetActive(true);
                    //_red.SetActive(false);
                    //_blue.SetActive(false);

                    _audioSource.clip = _hillContested;
                    GameManager.GetRootPlayer().announcer.AddClip(_hillContested);
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

    float _timer;


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
        if (playersInRange.Count == 1)
        {
            if (_timer < 1)
            {
                _timer += Time.deltaTime;

                if (_timer >= 1)
                {
                    if (playersInRange[0].isMine)
                    {
                        NetworkGameManager.instance.AddPlayerPoint(playersInRange[0].pid);
                    }

                    _timer = 0;
                }
            }
        }
    }
}
