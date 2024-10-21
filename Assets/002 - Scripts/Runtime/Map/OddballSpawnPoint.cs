using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class OddballSpawnPoint : MonoBehaviour
{
    [SerializeField] OddballSkull _oddball;


    int _secondsTheBallIsInactiveAndNoPlayersAreHoldingIt;
    float _check;

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
                    if (_oddball.transform.root == null && _oddball.gameObject.activeSelf
                        && GameManager.instance.GetAllPhotonPlayers().Where(item => item.playerInventory.playerOddballActive).Count() == 0)
                    {
                        _secondsTheBallIsInactiveAndNoPlayersAreHoldingIt++;
                        print($"oddball has disapeared for {_secondsTheBallIsInactiveAndNoPlayersAreHoldingIt} seconds");

                        if(_secondsTheBallIsInactiveAndNoPlayersAreHoldingIt == 5)
                        {

                        }
                    }
                    else
                    {
                        _secondsTheBallIsInactiveAndNoPlayersAreHoldingIt = 0;
                    }
                }
            }
        }
    }

    public void SpawnOddball()
    {
        print("SpawnOddball");
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
