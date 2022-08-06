using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    public Player player;
    public Transform targetTransform;

    [SerializeField] float ttl = 2;

    private void Update()
    {
        if (!targetTransform || !player)
            return;

        ttl -= Time.deltaTime;
        if (ttl <= 0)
            Destroy(gameObject);

        Vector3 tPos = targetTransform.position;
        Vector3 direction = player.transform.position - tPos;

        Quaternion tRot = Quaternion.LookRotation(direction);
        tRot.z = -tRot.y;
        tRot.x = 0;
        tRot.y = 0;

        Vector3 northDirection = new Vector3(0, 0, player.transform.eulerAngles.y);
        transform.localRotation = tRot * Quaternion.Euler(northDirection);

    }
}
