using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerThirdPersonModelManager : MonoBehaviour
{
    public delegate void PlayerModelManagerEvent(PlayerThirdPersonModelManager playerThirdPersonModelManager);
    public PlayerModelManagerEvent OnModelAssigned;

    [SerializeField] ThirdPersonScript _thidPersonScript;

    public Player player;
    public PlayerInventory playerInventory;
    public ThirdPersonScript thirdPersonScript
    {
        set { _thidPersonScript = value; }
        get
        {
            return _thidPersonScript;
        }
    }
    public ThirdPersonScript humanModel;
    public ThirdPersonScript spartanModel;
    public List<GameObject> models = new List<GameObject>();
    public List<GameObject> feet = new List<GameObject>();

    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] GameObject undersuitMesh;
    [SerializeField] GameObject soldierMeshObj;

    private void OnEnable()
    {

    }

    private void Awake()
    {
        print($"PlayerThirdPersonModelManager Awake {transform.root.name}");
        undersuitMesh.layer = 0;

        OnModelAssigned -= GetComponent<PlayerHitboxes>().OnModelAssigned;
        OnModelAssigned += GetComponent<PlayerHitboxes>().OnModelAssigned;
        playerInventory.OnActiveWeaponChanged -= OnActiveWeaponChanged_PlayTPSAnimations_Delegate;
        playerInventory.OnActiveWeaponChanged += OnActiveWeaponChanged_PlayTPSAnimations_Delegate;

        transform.root.GetComponent<Player>().OnPlayerIdAssigned -= OnPlayerIdAndRewiredIdAssigned_Delegate;
        transform.root.GetComponent<Player>().OnPlayerIdAssigned += OnPlayerIdAndRewiredIdAssigned_Delegate;

        //if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        thirdPersonScript = spartanModel;
        //if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        //    thirdPersonScript = humanModel;
    }
    private void Start()
    {
        print($"PlayerThirdPersonModelManager Start {transform.root.name}");
    }

    public void OnActiveWeaponChanged_PlayTPSAnimations_Delegate(PlayerInventory playerInventory)
    {
        print($"Chaging TPS model stance {playerInventory.activeWeapon.killFeedOutput} {playerInventory.activeWeapon.idleHandlingAnimationType}");


        if (!playerInventory.isDualWielding && !playerInventory.isHoldingHeavy)
        {
            thirdPersonScript.GetComponent<Animator>().SetBool($"Idle Rifle", false);
            thirdPersonScript.GetComponent<Animator>().SetBool($"Idle Pistol", false);
            thirdPersonScript.GetComponent<Animator>().SetBool($"sword idle", false);
            thirdPersonScript.GetComponent<Animator>().SetBool($"heavy idle", false);

            if (playerInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sword || playerInventory.hasEnnemyFlag)
            {
                thirdPersonScript.GetComponent<Animator>().SetBool($"sword idle", true);
                thirdPersonScript.GetComponent<Animator>().Play("sword draw");
            }
            else
            {
                thirdPersonScript.GetComponent<Animator>().SetBool($"Idle {playerInventory.activeWeapon.idleHandlingAnimationType}", true);
                thirdPersonScript.GetComponent<Animator>().SetBool($"sword idle", false);
                thirdPersonScript.GetComponent<Animator>().Play("Draw");
            }
        }
    }

    void OnPlayerIdAndRewiredIdAssigned_Delegate(Player p)
    {
        SetupThirdPersonModelLayers();
    }


    public void SetupThirdPersonModelLayers(bool forceFpsMode = false)
    {
        if (forceFpsMode)
        {
            SetupThirdPersonModelLayers_FPS();
        }
        else if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On ||
           (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.Off && player.playerInventory.activeWeapon.weaponType == WeaponProperties.WeaponType.Heavy))
        {
            print($"OnPlayerIdAndRewiredIdAssigned_Delegate {transform.root.name} OnPlayerIdAssigned THIRPERSON MODE ON");

            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.buildIndex > 0) // We are not in the menu
            {
                List<int> ignoreList = new List<int>();
                ignoreList.Add(7); // 7 = Player Hitbox
                foreach (GameObject model in models)
                    if (!feet.Contains(model))
                    {
                        GameManager.SetLayerRecursively(model, 0, ignoreList);
                    }

                undersuitMesh.layer = 0;

                OnModelAssigned?.Invoke(this);

                foreach (LootableWeapon lw in spartanModel.transform.GetComponentsInChildren<LootableWeapon>(true))
                {
                    lw.enabled = false;
                    lw.ttl = 99999;
                }
            }
        }
        else if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.Off)
        {
            SetupThirdPersonModelLayers_FPS();
        }
    }


    void SetupThirdPersonModelLayers_FPS()
    {
        print($"OnPlayerIdAndRewiredIdAssigned_Delegate {transform.root.name} OnPlayerIdAssigned");

        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            List<int> ignoreList = new List<int>();
            ignoreList.Add(7); // 7 = Player Hitbox
            foreach (GameObject model in models)
                if (!feet.Contains(model))
                {
                    if (player.PV.IsMine)
                    {
                        int l = 0;

                        if (player.rid == 0)
                            l = 25;
                        else if (player.rid == 1)
                            l = 27;
                        else if (player.rid == 2)
                            l = 29;
                        else if (player.rid == 3)
                            l = 31;

                        GameManager.SetLayerRecursively(model, l, ignoreList);
                    }
                    else
                    {
                        GameManager.SetLayerRecursively(model, 0, ignoreList);
                    }
                }


            undersuitMesh.layer = 0;

            if (player.PV.IsMine)
            {
                int l = 0;

                if (player.rid == 0)
                    l = 25;
                else if (player.rid == 1)
                    l = 27;
                else if (player.rid == 2)
                    l = 29;
                else if (player.rid == 3)
                    l = 31;
                undersuitMesh.layer = l;
            }


            OnModelAssigned?.Invoke(this);


            foreach (LootableWeapon lw in spartanModel.transform.GetComponentsInChildren<LootableWeapon>(true))
            {
                lw.enabled = false;
                lw.ttl = 99999;
            }
        }
    }
}
