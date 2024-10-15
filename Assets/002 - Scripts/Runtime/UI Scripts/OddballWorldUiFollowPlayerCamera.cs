using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static PlayerProgressionManager;

public class OddballWorldUiFollowPlayerCamera : MonoBehaviour
{
    public int ridTarget;

    Player _targetPlayer;
    Vector3 _targetPostition;
    Image _im;


    [SerializeField] Flag _flagScript;



    private void Awake()
    {
        try { _im = GetComponent<Image>(); } catch { }
    }

    private void Start()
    {
        if (GameManager.instance.gameType == GameManager.GameType.CTF && _flagScript)
        {
            _im.color = Color.white;

            if (_flagScript.spawnPoint.team == GameManager.Team.Red)
                _im.color = Color.red;

            if (_flagScript.spawnPoint.team == GameManager.Team.Blue)
            {
                Color _tCol = new Color();
                ColorUtility.TryParseHtmlString(GameManager.colorDict["lightblue"], out _tCol);
                _im.color = _tCol;
            }
        }
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
