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

    private void OnEnable()
    {

    }

    private void Awake()
    {
        OnModelAssigned -= GetComponent<PlayerHitboxes>().OnModelAssigned;
        OnModelAssigned += GetComponent<PlayerHitboxes>().OnModelAssigned;
        playerInventory.OnActiveWeaponChanged -= OnActiveWeaponChanged_Delegate;
        playerInventory.OnActiveWeaponChanged += OnActiveWeaponChanged_Delegate;

        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            thirdPersonScript = spartanModel;
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            thirdPersonScript = humanModel;
    }
    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            try
            {

                Debug.Log($"PlayerThirdPersonModelManager game mode: {GameManager.instance.gameMode}");
                if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                {
                    humanModel.gameObject.SetActive(false);
                    spartanModel.gameObject.SetActive(true);

                    spartanModel.EnableSkinnedMeshes();
                    humanModel.DisableSkinnedMeshes();
                }
                else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                {
                    humanModel.gameObject.SetActive(true);
                    spartanModel.gameObject.SetActive(false);

                    spartanModel.DisableSkinnedMeshes();
                    humanModel.EnableSkinnedMeshes();
                }

                OnModelAssigned?.Invoke(this);

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

                            Debug.Log($"PlayerThirdPersonModelManager {l}");

                            GameManager.SetLayerRecursively(model, l, ignoreList);
                        }
                        else
                        {
                            GameManager.SetLayerRecursively(model, 0, ignoreList);
                        }
                    }

            }
            catch (System.Exception e) { Debug.LogException(e); }
        }
    }

    void OnActiveWeaponChanged_Delegate(PlayerInventory playerInventory)
    {
        thirdPersonScript.GetComponent<Animator>().SetBool($"Idle Rifle", false);
        thirdPersonScript.GetComponent<Animator>().SetBool($"Idle Pistol", false);

        thirdPersonScript.GetComponent<Animator>().SetBool($"Idle {playerInventory.activeWeapon.idleHandlingAnimationType}", true);
    }
}
