using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DamageIndicatorManager : MonoBehaviour
{
    public Player player;
    public GameObject damageIndicatorPrefab;
    public int damageIndicatorLifeTime;
    public void SpawnNewDamageIndicator(int playerWhoShotPID)
    {
        if (playerWhoShotPID == 99) // Guardians
            return;
        try
        {
            Transform playerWhoDamagedThisPlayer = PhotonView.Find(playerWhoShotPID).transform;
            if (playerWhoDamagedThisPlayer.GetComponent<Player>() == player)
                return;

            var ndi = Instantiate(damageIndicatorPrefab, transform);
            ndi.GetComponent<DamageIndicator>().player = player;
            ndi.GetComponent<DamageIndicator>().targetTransform = playerWhoDamagedThisPlayer;

            //StartCoroutine(SpawnNewDamageIndicator_Coroutine(playerWhoShotPID));
        }
        catch (System.Exception e)
        {

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
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }
    }
}
