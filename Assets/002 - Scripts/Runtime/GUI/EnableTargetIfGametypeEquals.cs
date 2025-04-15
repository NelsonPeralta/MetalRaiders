using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableTargetIfGametypeEquals : MonoBehaviour
{
    [SerializeField] GameManager.GameType _gameType;
    [SerializeField] GameObject _target;


    private void OnEnable()
    {
        if(_gameType != GameManager.GameType.Unassgined && _target)
        {
            _target.SetActive(GameManager.instance.gameType == _gameType);
        }
    }
}
