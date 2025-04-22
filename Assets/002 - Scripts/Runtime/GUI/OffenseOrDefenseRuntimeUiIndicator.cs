using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffenseOrDefenseRuntimeUiIndicator : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] GameObject _offense, _defense;





    private void Awake()
    {
        _offense.SetActive(false);
        _defense.SetActive(false);
    }


    public void Trigger()
    {
        if (GameManager.instance.oneObjMode == GameManager.OneObjMode.On && _player)
        {
            if (_player.team == GameManager.instance.teamAttackingThisRound)
            {
                _offense.SetActive(true);
            }
            else
            {
                _defense.SetActive(true);
            }
        }
    }
}
