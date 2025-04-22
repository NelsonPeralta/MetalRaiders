using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.Linq;
using UnityEngine;

public class FlagSpawnPoint : MonoBehaviour
{
    public GameManager.Team team;

    public bool teammateOnFlag { set { if (value == true) _teammateOnFlagReset = 0.2f; } }


    [SerializeField] Flag _flag;
    [SerializeField] GameObject _canvasHolder;

    [SerializeField] int _resetFlag;
    [SerializeField] float _check, _teammateOnFlagReset;


    private void Awake()
    {
        if (GameManager.instance.gameType == GameManager.GameType.CTF)
        {
            _flag.spawnPoint = this;
            _flag.scriptRoot.parent = null;

            _canvasHolder.SetActive(true);
        }
        else
            Destroy(gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.instance.oneObjMode == GameManager.OneObjMode.On)
            _flag.scriptRoot.gameObject.SetActive(false);


        if (GameManager.instance.gameType == GameManager.GameType.CTF)
        {
            _check = 1;
            _resetFlag = 0;
            SpawnFlagAtStand();
        }

        else _flag.scriptRoot.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.gameType == GameManager.GameType.CTF && PhotonNetwork.IsMasterClient
            && CurrentRoomManager.instance.gameStarted && !CurrentRoomManager.instance.gameOver)
        {
            if (_teammateOnFlagReset > 0)
            {
                _teammateOnFlagReset -= Time.deltaTime;
            }



            if (_check > 0)
            {
                _check -= Time.deltaTime;

                if (_check < 0)
                {
                    //print($"OddballSpawnPoint {_flag.scriptRoot.transform.parent == null} {_flag.gameObject.activeInHierarchy} " +
                    //    $"{Vector3.Distance(transform.position, _flag.rb.transform.position)} {GameManager.instance.GetAllPhotonPlayers().Where(item => item.team != team && item.hasEnnemyFlag).Count()}");

                    if (_flag.transform.position.y < -20)
                    {
                        if (PhotonNetwork.IsMasterClient)
                            NetworkGameManager.instance.AskMasterClientToSpawnFlag(Vector3.up * -999, Vector3.zero, team, initialCall: false, masterCall: false);

                    }
                    else if (_flag.scriptRoot.transform.parent == null && !_flag.gameObject.activeInHierarchy
                        && GameManager.instance.GetAllPhotonPlayers().Where(item => item.team != team && item.playerInventory.hasEnnemyFlag).Count() == 0)
                    {

                        if (GameManager.instance.oneObjMode == GameManager.OneObjMode.Off)
                        {
                            _resetFlag++;
                            print($"flag has disapeared for {_resetFlag} half seconds");

                            if (_resetFlag >= 10)
                            {
                                if (PhotonNetwork.IsMasterClient)
                                    NetworkGameManager.instance.AskMasterClientToSpawnFlag(Vector3.up * -999, Vector3.zero, team, initialCall: false, masterCall: false);
                            }
                        }
                        else
                        {
                            if (!GameManager.instance.OneObjModeRoundOver)
                            {
                                if (team == GameManager.Team.Red && (GameManager.instance.OneObjModeRoundCounter == 0 || GameManager.instance.OneObjModeRoundCounter % 2 == 0))
                                //if (team == GameManager.Team.Red && GameManager.instance.OneObjModeRoundCounter % 2 == 1)
                                {
                                    _resetFlag++;
                                    print($"flag has disapeared for {_resetFlag} half seconds");

                                    if (_resetFlag >= 10)
                                    {
                                        if (PhotonNetwork.IsMasterClient)
                                            NetworkGameManager.instance.AskMasterClientToSpawnFlag(Vector3.up * -999, Vector3.zero, team, initialCall: false, masterCall: false);
                                    }
                                }
                                else if (team == GameManager.Team.Blue && GameManager.instance.OneObjModeRoundCounter % 2 == 1)
                                //else if (team == GameManager.Team.Blue && (GameManager.instance.OneObjModeRoundCounter == 0 || GameManager.instance.OneObjModeRoundCounter % 2 == 0))
                                {
                                    _resetFlag++;
                                    print($"flag has disapeared for {_resetFlag} halfseconds");

                                    if (_resetFlag >= 10)
                                    {
                                        if (PhotonNetwork.IsMasterClient)
                                            NetworkGameManager.instance.AskMasterClientToSpawnFlag(Vector3.up * -999, Vector3.zero, team, initialCall: false, masterCall: false);
                                    }
                                }
                            }
                        }
                    }
                    else if (_flag.scriptRoot.transform.parent == null && _flag.state != Flag.State.atbase && _flag.gameObject.activeInHierarchy && GameManager.instance.GetAllPhotonPlayers().Where(item => item.team != team && item.hasEnnemyFlag).Count() == 0)
                    {
                        _resetFlag++;

                        if (_teammateOnFlagReset > 0)
                            _resetFlag += 2;



                        print($"flag has disapeared for {_resetFlag} half seconds");

                        if (_resetFlag >= 30)
                        {
                            if (PhotonNetwork.IsMasterClient)
                                NetworkGameManager.instance.AskMasterClientToSpawnFlag(Vector3.up * -999, Vector3.zero, team, initialCall: false, masterCall: false);
                        }
                    }
                    else
                    {
                        _resetFlag = 0;
                    }
                    _check = 0.5f;
                }
            }
        }
    }


    public void SpawnFlagAtStand()
    {
        if (GameManager.instance.oneObjMode == GameManager.OneObjMode.Off)
        {
            _resetFlag = 0;
            print("SpawnFlag");
            StartCoroutine(SpawnFlagAtStand_Coroutine());
        }
        else
        {
            if (team == GameManager.Team.Red && (GameManager.instance.OneObjModeRoundCounter == 0 || GameManager.instance.OneObjModeRoundCounter % 2 == 0))
            //if (team == GameManager.Team.Red && GameManager.instance.OneObjModeRoundCounter % 2 == 1)
            {
                _resetFlag = 0;
                print("SpawnFlag");
                StartCoroutine(SpawnFlagAtStand_Coroutine());
            }
            else if (team == GameManager.Team.Blue && GameManager.instance.OneObjModeRoundCounter % 2 == 1)
            //else if (team == GameManager.Team.Blue && (GameManager.instance.OneObjModeRoundCounter == 0 || GameManager.instance.OneObjModeRoundCounter % 2 == 0))
            {
                _resetFlag = 0;
                print("SpawnFlag");
                StartCoroutine(SpawnFlagAtStand_Coroutine());
            }
        }
    }

    IEnumerator SpawnFlagAtStand_Coroutine()
    {
        print("SpawnFlagAtStand_Coroutine 1");
        _flag.ChangeState(Flag.State.atbase);
        _flag.scriptRoot.gameObject.SetActive(false);
        _flag.rb.velocity = Vector3.zero;
        _flag.rb.angularVelocity = Vector3.zero;
        _flag.rb.mass = 999;

        _flag.transform.root.rotation = transform.rotation;
        _flag.transform.root.position = transform.position + (Vector3.up * 1.5f);

        yield return new WaitForSeconds(1);

        print("SpawnFlagAtStand_Coroutine 2");
        _flag.scriptRoot.gameObject.SetActive(true);
    }
}
