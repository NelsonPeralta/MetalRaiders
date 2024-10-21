using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class OddballSpawnPoint : MonoBehaviour
{
    [SerializeField] OddballSkull _oddball;


    [SerializeField] int _resetBall;
    [SerializeField] float _check;

    private void Awake()
    {
        _oddball.thisRoot.parent = null;
    }

    private void Start()
    {
        if (GameManager.instance.gameType == GameManager.GameType.Oddball)
        {
            SpawnOddball();
            _check = 1;
        }
        else _oddball.DisableOddball();
    }

    private void Update()
    {
        if (GameManager.instance.gameType == GameManager.GameType.Oddball && PhotonNetwork.IsMasterClient
            && CurrentRoomManager.instance.gameStarted && !CurrentRoomManager.instance.gameOver)
        {
            if (_check > 0)
            {
                _check -= Time.deltaTime;

                if (_check < 0)
                {
                    print($"OddballSpawnPoint {_oddball.thisRoot.transform.parent == null} {_oddball.gameObject.activeInHierarchy} " +
                        $"{Vector3.Distance(transform.position, _oddball.rb.transform.position)} {GameManager.instance.GetAllPhotonPlayers().Where(item => item.playerInventory.playerOddballActive).Count()}");

                    if (_oddball.rb.transform.position.y < -20)
                    {
                        NetworkGameManager.instance.AskMasterClientToSpawnOddball(Vector3.up * -999, Vector3.zero);
                    }
                    if (_oddball.thisRoot.transform.parent == null && !_oddball.gameObject.activeInHierarchy
                        && GameManager.instance.GetAllPhotonPlayers().Where(item => item.playerInventory.playerOddballActive).Count() == 0)
                    {
                        _resetBall++;
                        print($"oddball has disapeared for {_resetBall} seconds");

                        if (_resetBall >= 10)
                        {
                            NetworkGameManager.instance.AskMasterClientToSpawnOddball(Vector3.up * -999, Vector3.zero);
                        }
                    }
                    else if (_oddball.thisRoot.transform.parent == null && _oddball.gameObject.activeInHierarchy && Vector3.Distance(transform.position, _oddball.rb.transform.position) > 3
                        && GameManager.instance.GetAllPhotonPlayers().Where(item => item.playerInventory.playerOddballActive).Count() == 0)
                    {
                        _resetBall++;
                        print($"oddball has disapeared for {_resetBall} seconds");

                        if (_resetBall >= 30)
                        {
                            NetworkGameManager.instance.AskMasterClientToSpawnOddball(Vector3.up * -999, Vector3.zero);
                        }
                    }
                    else
                    {
                        _resetBall = 0;
                    }
                    _check = 0.5f;
                }
            }
        }
    }

    public void SpawnOddball()
    {
        print("SpawnOddball");
        _resetBall = 0;
        StartCoroutine(SpawnOddball_Coroutine());
    }


    IEnumerator SpawnOddball_Coroutine()
    {
        _oddball.DisableOddball();
        _oddball.rb.velocity = Vector3.zero;
        _oddball.rb.angularVelocity = Vector3.zero;

        _oddball.transform.root.rotation = Quaternion.identity;
        _oddball.transform.root.position = transform.position;

        yield return new WaitForSeconds(1);

        print("SpawnOddball_Coroutine");
        _oddball.thisRoot.gameObject.SetActive(true);
    }
}
