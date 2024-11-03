using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Flag : MonoBehaviour
{
    public enum State { atbase, away, stolen }

    public State state { get { return _state; } }
    public AudioClip flagCapturedClip { get { return _capturedClip; } }


    public Transform scriptRoot;
    public FlagSpawnPoint spawnPoint;
    public Rigidbody rb;


    [SerializeField] AudioClip _takenClip, _stolenClip, _droppedClip, _resetClip, _capturedClip;
    [SerializeField] GameObject _redBanner, _blueBanner, _canvases;
    [SerializeField] State _state;


    float  _triggerReset;

    // Start is called before the first frame update
    void Start()
    {
        _state = State.atbase;

        _redBanner.SetActive(GameManager.instance.gameType == GameManager.GameType.CTF && spawnPoint.team == GameManager.Team.Red);
        _blueBanner.SetActive(GameManager.instance.gameType == GameManager.GameType.CTF && spawnPoint.team == GameManager.Team.Blue);
        _canvases.SetActive(true);

        if (GameManager.instance.gameType == GameManager.GameType.CTF)
        {
            if (spawnPoint.team == GameManager.Team.Red) GameManager.instance.redFlag = this;
            if (spawnPoint.team == GameManager.Team.Blue) GameManager.instance.blueFlag = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_triggerReset > 0) _triggerReset -= Time.deltaTime;


        
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        _triggerReset = 1f;
    }




    private void OnTriggerStay(Collider other)
    {
        if (CurrentRoomManager.instance.gameStarted && !CurrentRoomManager.instance.gameOver)
        {
            print($"FLAG OnTriggerStay {other.transform.root.name}");
            if (_triggerReset <= 0 && scriptRoot.gameObject.activeSelf && other.transform.root.GetComponent<Player>() &&
                !other.transform.root.GetComponent<Player>().hasEnnemyFlag &&
                other.transform.root.GetComponent<Player>() && other.transform.root.GetComponent<Player>().team != spawnPoint.team)
            {
                if (other.transform.root.GetComponent<Player>().isAlive)
                {
                    print($"{other.name} has taken the flag");
                    //other.transform.root.GetComponent<Player>().playerInventory.EquipFlag(); // do locally then network
                    NetworkGameManager.instance.EquipFlagToPlayer_RPC(other.transform.root.GetComponent<Player>().photonId, (int)(other.transform.root.GetComponent<Player>().team == GameManager.Team.Red ? GameManager.Team.Blue : GameManager.Team.Red));
                }
            }else if (_triggerReset <= 0 && scriptRoot.gameObject.activeSelf && other.transform.root.GetComponent<Player>()
                && other.transform.root.GetComponent<Player>().team == spawnPoint.team)
            {
                spawnPoint.teammateOnFlag = true;
            }
        }
    }

    public void ChangeState(State s)
    {
        _state = s;

        if (_state == State.away) GameManager.GetRootPlayer().announcer.AddClip(_droppedClip);
        //else if (_state == State.atbase) GameManager.GetRootPlayer().announcer.AddClip(_resetClip);
        else if (_state == State.stolen)
        {
            //bool _toldRedTeam, _toldBlueTeam;


            foreach (Player p in GameManager.GetLocalPlayers())
            {
                if (spawnPoint.team != p.team)
                {
                    GameManager.GetRootPlayer().announcer.AddClip(_takenClip);
                    break;
                }
            }

            foreach (Player p in GameManager.GetLocalPlayers())
            {
                if (spawnPoint.team == p.team)
                {
                    GameManager.GetRootPlayer().announcer.AddClip(_stolenClip);
                    break;
                }
            }
        }
    }

    public void PlayerResetClip()
    {
        GameManager.GetRootPlayer().announcer.AddClip(_resetClip);
    }
}
