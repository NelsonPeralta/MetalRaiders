using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OddballWorldUiFollowPlayerCamera : MonoBehaviour
{
    public int ridTarget;

    Player _targetPlayer;
    Vector3 _targetPostition;
    Image _im;



    private void Awake()
    {
        try { _im = GetComponent<Image>(); } catch { }
    }

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
            try { _im.enabled = _targetPlayer.isAlive; } catch { }
            _targetPostition = new Vector3(_targetPlayer.transform.position.x,
                                            this.transform.position.y,
                                            _targetPlayer.transform.position.z);
            this.transform.LookAt(_targetPostition);
        }
    }
}
