using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OddballWorldUiFollowPlayerCamera : MonoBehaviour
{
    public int ridTarget;

    Player _targetPlayer;
    Vector3 _targetPostition;

    // Update is called once per frame
    private void Update()
    {
        if (!_targetPlayer)
        {
            try
            {
                _targetPlayer = GameManager.GetLocalPlayer(ridTarget);
            }
            catch { }
        }
        else
        {
            _targetPostition = new Vector3(_targetPlayer.transform.position.x,
                                            this.transform.position.y,
                                            _targetPlayer.transform.position.z);
            this.transform.LookAt(_targetPostition);
        }
    }
}
