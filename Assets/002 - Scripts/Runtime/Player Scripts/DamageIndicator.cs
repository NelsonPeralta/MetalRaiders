using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    public Player player;

    Transform _targetTransform;
    public Transform targetTransform
    {
        get { return _targetTransform; }
        set { _targetTransform = value; _targetPosition = targetTransform.position; }
    }

    Vector3 _targetPosition;

    public float ttl = 2;

    private void Update()
    {
        if (!targetTransform || !player)
            return;

        ttl -= Time.deltaTime;
        if (ttl <= 0)
            gameObject.SetActive(false);

        Vector3 direction = player.transform.position - _targetPosition;

        Quaternion tRot = Quaternion.LookRotation(direction);
        tRot.z = -tRot.y;
        tRot.x = 0;
        tRot.y = 0;

        Vector3 northDirection = new Vector3(0, 0, player.transform.eulerAngles.y);
        transform.localRotation = tRot * Quaternion.Euler(northDirection);

    }
}
