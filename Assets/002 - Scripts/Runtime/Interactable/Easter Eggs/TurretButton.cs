using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretButton : InteractableObject
{
    public List<Player> collidingPlayers { get { return _playerDetector.collidingPlayers; } }
    public bool consumed { get { return _turret.activeInHierarchy; } }

    [SerializeField] int _cost;
    [SerializeField] PlayerDetector _playerDetector;
    [SerializeField] GameObject _turret, _greenBtn, _redBtn;


    // Update is called once per frame
    void Update()
    {
        foreach (Player player in collidingPlayers)
        {
            if (player.GetComponent<PlayerSwarmMatchStats>().points >= _cost && !consumed)
            {
                player.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
                player.GetComponent<PlayerController>().OnPlayerLongInteract += OnPlayerLongInteract_Delegate;
                player.GetComponent<PlayerUI>().weaponInformerText.text = $"Mortar Strike ({_cost})";
            }
            else
            {
                if (player.GetComponent<PlayerSwarmMatchStats>().points < _cost)
                    player.GetComponent<PlayerUI>().weaponInformerText.text = $"Not enough points ({_cost})";
                else if(consumed)
                    player.GetComponent<PlayerUI>().weaponInformerText.text = $"Recharging...";
            }
        }
    }

    public override void Trigger(int? pid)
    {
        _greenBtn.SetActive(false);
        _redBtn.SetActive(true);
        _turret.GetComponent<TurretHead>().player = GameManager.instance.pid_player_Dict[(int)pid];
        _turret.SetActive(true);
    }

    void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        if (playerController.GetComponent<PlayerSwarmMatchStats>().points >= _cost)
        {
            NetworkGameManager.instance.AskHostToTriggerInteractableObject(transform.position, playerController.player.photonId);
        }
    }
}
