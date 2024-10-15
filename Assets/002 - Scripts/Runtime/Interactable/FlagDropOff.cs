using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagDropOff : MonoBehaviour
{
    [SerializeField] FlagSpawnPoint _flagSpawnPoint;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        if (CurrentRoomManager.instance.gameStarted && !CurrentRoomManager.instance.gameOver)
        {
            //print($"FLAG OnTriggerStay {other.transform.root.name}");
            if (other.transform.root.GetComponent<Player>() && other.transform.root.GetComponent<Player>().team == _flagSpawnPoint.team
                && other.transform.root.GetComponent<Player>().hasEnnemyFlag)
            {
                if (other.transform.root.GetComponent<Player>().isAlive)
                {
                    NetworkGameManager.instance.AddPlayerPoint(other.transform.root.GetComponent<Player>().photonId,
                    (int)(other.transform.root.GetComponent<Player>().team == GameManager.Team.Red ? GameManager.Team.Blue : GameManager.Team.Red));
                }
            }
        }
    }
}
