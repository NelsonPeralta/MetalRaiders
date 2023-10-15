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
        _radioLight.SetActive(false);
        found = true;
        GetComponent<AudioSource>().Play();

        foreach (Transform t in _artillerySpawnPoint)
        {
            GameObject a = Instantiate(_artilleryPrefab, t.position, t.rotation);
            a.GetComponent<ExplosiveProjectile>().player = GameManager.instance.pid_player_Dict[(int)pid];
        }
    }












    [Header("Players in Range")]
    public List<Player> playersInRange = new List<Player>();

    [SerializeField] bool _found;
    public bool found
    {
        get { return _found; }
        private set
        {
            bool previousValue = _found;
            _found = value;

            if (value && !previousValue)
            {
                _reset = 3;
            }
        }
    }

    float _reset = 1;




    private void Update()
    {
        if(_reset > 0)
        {
            _reset -= Time.deltaTime;

            if(_reset <= 0)
            {
                _found = false;
                _radioLight.SetActive(true);
            }
        }
    }










    private void OnTriggerStay(Collider other)
    {
        if (found)
            return;

        if (other.GetComponent<Player>() && !other.GetComponent<Player>().isDead && !playersInRange.Contains(other.GetComponent<Player>()))
        {
            playersInRange.Add(other.GetComponent<Player>());

            if (!other.GetComponent<Player>().hasArmor)
            {
                if (other.GetComponent<PlayerSwarmMatchStats>().points >= cost)
                {
                    other.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
                    other.GetComponent<PlayerController>().OnPlayerLongInteract += OnPlayerLongInteract_Delegate;
                    other.GetComponent<PlayerUI>().weaponInformerText.text = $"Buy armor for {cost} points";
                }
                else
                    other.GetComponent<PlayerUI>().weaponInformerText.text = $"Not enough points ({cost})";
            }
            else
                other.GetComponent<PlayerUI>().weaponInformerText.text = $"You already have an armor";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() && playersInRange.Contains(other.GetComponent<Player>()))
        {
            playersInRange.Remove(other.GetComponent<Player>());
            other.GetComponent<PlayerUI>().weaponInformerText.text = $"";
        }
    }

    void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        if (playerController.GetComponent<PlayerSwarmMatchStats>().points >= cost)
        {
            NetworkGameManager.instance.AskHostToTriggerInteractableObject(transform.position, playerController.player.pid);
        }
    }
}
