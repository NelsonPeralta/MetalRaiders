using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Flag : MonoBehaviour
{
    public Transform scriptRoot;
    public FlagSpawnPoint spawnPoint;
    public Rigidbody rb;
    public AudioClip _takenClip, _stolenClip, _droppedClip, _resetClip;

    [SerializeField] GameObject _redBanner, _blueBanner;

    float _reset, _triggerReset;

    // Start is called before the first frame update
    void Start()
    {
        _redBanner.SetActive(GameManager.instance.gameType == GameManager.GameType.CTF && spawnPoint.team == GameManager.Team.Red);
        _blueBanner.SetActive(GameManager.instance.gameType == GameManager.GameType.CTF && spawnPoint.team == GameManager.Team.Blue);

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


        if (transform.position.y <= -20)
        {
            foreach (Player p in GameManager.GetLocalPlayers()) p.killFeedManager.EnterNewFeed($"<color=#31cff9>{spawnPoint.team} Flag Reset");
            spawnPoint.SpawnFlagAtStand();

            _reset = 15;
        }
        else
        {
            if (Vector3.Distance(spawnPoint.transform.position, rb.transform.position) > 3)
            {
                _reset -= Time.deltaTime;

                if (_reset <= 0)
                {
                    foreach (Player p in GameManager.GetLocalPlayers()) p.killFeedManager.EnterNewFeed($"<color=#31cff9>{spawnPoint.team} Flag Reset");
                    spawnPoint.SpawnFlagAtStand();
                    //PlayBallResetClip();

                    _reset = 15;
                }
            }
            else
            {
                _reset = 15;
            }
        }
    }

    private void OnEnable()
    {
        _reset = 15;
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
            }
        }
    }
}
