using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OddballPointGiver : MonoBehaviour
{
    [SerializeField] Player player;

    float _del;

    private void OnEnable()
    {
        _del = 1;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (player.isMine)
            if (_del > 0)
            {
                _del -= Time.deltaTime;

                if (_del <= 0 && (!player.isDead && !player.isRespawning))
                {
                    NetworkGameManager.instance.AddPlayerPoint(player.photonId);

                    _del = 1;
                }
            }

    }
}
