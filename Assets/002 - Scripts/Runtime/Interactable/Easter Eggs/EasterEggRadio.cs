using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasterEggRadio : InteractableObject
{
    public int cost;
    [SerializeField] GameObject _radioLight;
    [SerializeField] GameObject _artilleryPrefab;
    [SerializeField] List<Transform> _artillerySpawnPoint = new List<Transform>();

    public override void Trigger(int? pid)
    {
        Player mp = GameManager.GetPlayerWithPhotonView((int)pid);
        _radioLight.SetActive(false);
        consumed = true;
        GetComponent<AudioSource>().Play();

        foreach (Transform t in _artillerySpawnPoint)
        {
            GameObject a = Instantiate(_artilleryPrefab, t.position, t.rotation);
            a.GetComponent<ExplosiveProjectile>().player = GameManager.GetPlayerWithPhotonView((int)pid);
        }

        mp.GetComponent<PlayerSwarmMatchStats>().RemovePoints(cost);
    }












    [Header("Players in Range")]
    public List<Player> playersInRange = new List<Player>();

    [SerializeField] bool _consumed;
    public bool consumed
    {
        get { return _consumed; }
        private set
        {
            bool previousValue = _consumed;
            _consumed = value;

            if (value && !previousValue)
            {
                _reset = 30;

                if (SwarmManager.instance.editMode) _reset = 5;
            }
        }
    }

    float _reset = 1;




    private void Update()
    {
        if (_reset > 0)
        {
            _reset -= Time.deltaTime;

            if (_reset <= 0)
            {
                _consumed = false;
                _radioLight.SetActive(true);
            }
        }
    }










    private void OnTriggerStay(Collider other)
    {
        if (consumed)
            return;

        if (other.GetComponent<Player>() && !other.GetComponent<Player>().isDead && !playersInRange.Contains(other.GetComponent<Player>()))
        {
            playersInRange.Add(other.GetComponent<Player>());

            if (other.GetComponent<PlayerSwarmMatchStats>().points >= cost && !consumed)
            {
                other.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
                other.GetComponent<PlayerController>().OnPlayerLongInteract += OnPlayerLongInteract_Delegate;
                //other.GetComponent<PlayerUI>()._informerText.text = $"Mortar Strike ({cost})";
            }
            else
            {
                //if (other.GetComponent<PlayerSwarmMatchStats>().points < cost)
                //    other.GetComponent<PlayerUI>()._informerText.text = $"Not enough points ({cost})";
                //else
                //    other.GetComponent<PlayerUI>()._informerText.text = $"Recharging...";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() && playersInRange.Contains(other.GetComponent<Player>()))
        {
            playersInRange.Remove(other.GetComponent<Player>());
            //other.GetComponent<PlayerUI>()._informerText.text = $"";
        }
    }

    void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        if (playerController.GetComponent<PlayerSwarmMatchStats>().points >= cost)
        {
            NetworkGameManager.instance.AskHostToTriggerInteractableObject(transform.position, playerController.player.photonId);
        }
    }
}
