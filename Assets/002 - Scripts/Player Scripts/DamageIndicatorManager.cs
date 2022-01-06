using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DamageIndicatorManager : MonoBehaviour
{
    public PlayerProperties player;
    public GameObject damageIndicatorPrefab;
    public int damageIndicatorLifeTime;
    public void SpawnNewDamageIndicator(int playerWhoShotPID)
    {
        if (playerWhoShotPID == 99) // Guardians
            return;
        StartCoroutine(SpawnNewDamageIndicator_Coroutine(playerWhoShotPID));
    }

    IEnumerator SpawnNewDamageIndicator_Coroutine(int playerWhoShotPID)
    {
        var ndi = Instantiate(damageIndicatorPrefab, transform);

        Transform playerWhoDamagedThisPlayer = PhotonView.Find(playerWhoShotPID).transform;

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
}
