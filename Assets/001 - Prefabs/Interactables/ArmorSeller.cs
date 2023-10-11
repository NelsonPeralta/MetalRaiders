using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.ProBuilder.Shapes;

public class ArmorSeller : InteractableObject
{
    public GameObject armorModel;

    [Header("Seller Info")]
    public int cost = -1;

    [Header("Players in Range")]
    public List<Player> playersInRange = new List<Player>();

    [SerializeField] List<EasterEggTreasure> _findableObjects = new List<EasterEggTreasure>();

    [SerializeField] bool _allFindableObjectsFound;
    [SerializeField] AudioClip _findableObjectsFoundClip;
    [SerializeField] AudioClip _allFindableObjectsFoundClip;
    public bool allFindableObjectsFound
    {
        get { return _allFindableObjectsFound; }
        private set
        {
            _allFindableObjectsFound = value;

            if (value)
            {
                armorModel.SetActive(true);
                GetComponent<AudioSource>().clip = _allFindableObjectsFoundClip;
                GetComponent<AudioSource>().Play();
            }
        }
    }
    private void Start()
    {
        if (SwarmManager.instance.editMode)
            cost = 0;
        armorModel.SetActive(false);

        foreach (EasterEggTreasure fo in _findableObjects)
            fo.OnFound += OnFindableObjectFound;
    }
    private void OnTriggerStay(Collider other)
    {
        if (!allFindableObjectsFound)
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

    void OnFindableObjectFound(EasterEggTreasure findableObject)
    {
        Debug.Log($"OnFindableObjectFound");
        bool _allFound = true;
        foreach (EasterEggTreasure fo in _findableObjects)
            if (!fo.found)
                _allFound = false;

        if (!_allFound)
        {
            GetComponent<AudioSource>().clip = _findableObjectsFoundClip;
            GetComponent<AudioSource>().Play();
        }

        allFindableObjectsFound = _allFound;
    }

    public override void Trigger(int? pid)
    {
        Debug.Log($"Player {pid} bought armor");
        Player p = GameManager.instance.pid_player_Dict[(int)pid];
        Debug.Log($"Player {p.nickName} bought armor");

        p.maxHitPoints = 250;
        p.maxShieldPoints = 150;
        p.maxHealthPoints = 100;
        p.hitPoints = 250;

        p.hasArmor = true;


        p.playerArmorManager.HardReloadArmor(true);
        p.playerArmorManager.ReloadFpsArmor();



        playersInRange.Remove(p);
        p.GetComponent<PlayerUI>().weaponInformerText.text = $"";
    }
}
