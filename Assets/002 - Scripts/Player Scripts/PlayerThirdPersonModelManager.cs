using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerThirdPersonModelManager : MonoBehaviour
{
    public delegate void PlayerModelManagerEvent(PlayerThirdPersonModelManager playerThirdPersonModelManager);
    public PlayerModelManagerEvent OnModelAssigned;

    public Player player;
    public PlayerInventory playerInventory;
    public ThirdPersonScript thirdPersonScript
    {
        get {
            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                return spartanModel;
            if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                return humanModel;
            return null;
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
        playerInventory.OnActiveWeaponChanged -= OnActiveWeaponChanged_Delegate;
        playerInventory.OnActiveWeaponChanged += OnActiveWeaponChanged_Delegate;
    }
    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            try
            {
                OnModelAssigned += GetComponent<PlayerHitboxes>().OnModelAssigned;

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

                if (!player.photonView.IsMine)
                    return;

                Debug.Log("PlayerArmorManager OnSceneLoaded");

                foreach (GameObject model in models)
                    if (!feet.Contains(model))
                        GameManager.SetLayerRecursively(model, 31);

                OnModelAssigned?.Invoke(this);

            }
            catch (System.Exception e) { Debug.Log(e); }
        }
    }

    void OnActiveWeaponChanged_Delegate(PlayerInventory playerInventory)
    {
        thirdPersonScript.GetComponent<Animator>().SetBool($"Idle Rifle", false);
        thirdPersonScript.GetComponent<Animator>().SetBool($"Idle Pistol", false);

        thirdPersonScript.GetComponent<Animator>().SetBool($"Idle {playerInventory.activeWeapon.idleHandlingAnimationType}", true);
    }
}
