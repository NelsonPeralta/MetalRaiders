using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.ProBuilder.Shapes;

public class ArmorSeller : MonoBehaviour
{
    public GameObject armorModel;

    [Header("Seller Info")]
    public int cost = -1;

    [Header("Players in Range")]
    public List<Player> playersInRange = new List<Player>();

    [SerializeField] List<FindableObject> _findableObjects = new List<FindableObject>();

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
            cost = 100;
        armorModel.SetActive(false);

        foreach (FindableObject fo in _findableObjects)
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
            Debug.Log($"Player {playerController.GetComponent<PhotonView>().Owner.NickName} bought armor");
            playerController.player.maxHitPoints = 250;
            playerController.player.maxShieldPoints = 150;
            playerController.player.maxHealthPoints = 100;
            playerController.player.hitPoints = 250;

            playerController.player.playerArmorManager.HardReloadArmor();

            playerController.GetComponent<Player>().hasArmor = true;

            if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                foreach (PlayerArmorPiece pap in playerController.player.playerInventory.activeWeapon.GetComponentsInChildren<PlayerArmorPiece>(true))
                    pap.gameObject.SetActive(playerController.player.playerArmorManager.armorDataString.Contains(pap.entity));

            playersInRange.Remove(playerController.GetComponent<Player>());
            playerController.GetComponent<PlayerUI>().weaponInformerText.text = $"";
        }
    }

    void OnFindableObjectFound(FindableObject findableObject)
    {
        Debug.Log($"OnFindableObjectFound");
        bool _allFound = true;
        foreach (FindableObject fo in _findableObjects)
            if (!fo.found)
                _allFound = false;

        if (!_allFound)
        {
            GetComponent<AudioSource>().clip = _findableObjectsFoundClip;
            GetComponent<AudioSource>().Play();
        }

        allFindableObjectsFound = _allFound;
    }
}
