using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DamageIndicatorManager : MonoBehaviour
{
    public Player player;
    public GameObject damageIndicatorPrefab;
    public int damageIndicatorLifeTime;


    List<GameObject> _damageIndicatorList = new List<GameObject>();


    int i = 0;
    private void Start()
    {
        for (i = 0; i < 30; i++)
        {
            _damageIndicatorList.Add(Instantiate(damageIndicatorPrefab, transform));
            _damageIndicatorList[i].gameObject.SetActive(false);
        }
    }


    public void SpawnNewDamageIndicator(int playerWhoShotPID)
    {
        if (playerWhoShotPID == 99) // Guardians
            return;

        if (player.isMine)
        {
            Transform playerWhoDamagedThisPlayer = PhotonView.Find(playerWhoShotPID).transform;
            if (playerWhoDamagedThisPlayer.GetComponent<Player>() == player)
                return;

            i = 0;
            for (i = 0; i < _damageIndicatorList.Count; i++)
            {
                if (!_damageIndicatorList[i].gameObject.activeSelf)
                {
                    _damageIndicatorList[i].GetComponent<DamageIndicator>().player = player;
                    _damageIndicatorList[i].GetComponent<DamageIndicator>().targetTransform = playerWhoDamagedThisPlayer;
                    _damageIndicatorList[i].GetComponent<DamageIndicator>().ttl = 2;
                    _damageIndicatorList[i].gameObject.SetActive(true);

                    break;
                }
            }
        }
    }

    IEnumerator SpawnNewDamageIndicator_Coroutine(int playerWhoShotPID)
    {
        Transform playerWhoDamagedThisPlayer = PhotonView.Find(playerWhoShotPID).transform;

        var ndi = Instantiate(damageIndicatorPrefab, transform);
        ndi.GetComponent<DamageIndicator>().player = player;
        ndi.GetComponent<DamageIndicator>().targetTransform = playerWhoDamagedThisPlayer;


        Vector3 tPos = playerWhoDamagedThisPlayer.position;
        Vector3 direction = player.transform.position - tPos;

        Quaternion tRot = Quaternion.LookRotation(direction);
        tRot.z = -tRot.y;
        tRot.x = 0;
        tRot.y = 0;

        Vector3 northDirection = new Vector3(0, 0, player.transform.eulerAngles.y);
        ndi.transform.localRotation = tRot * Quaternion.Euler(northDirection);

        yield return new WaitForSeconds(damageIndicatorLifeTime);
        Destroy(ndi.gameObject);
    }

    public void HideAllIndicators()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }
}
