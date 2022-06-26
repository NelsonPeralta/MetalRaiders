using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerThirdPersonModelManager : MonoBehaviour
{
    public delegate void PlayerModelManagerEvent(PlayerThirdPersonModelManager playerThirdPersonModelManager);
    public PlayerModelManagerEvent OnModelAssigned;

    public Player player;
    public ThirdPersonScript humanModel;
    public ThirdPersonScript spartanModel;
    public List<GameObject> models = new List<GameObject>();
    public List<GameObject> feet = new List<GameObject>();

    private void OnEnable()
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

        OnModelAssigned?.Invoke(this);
    }

    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            try
            {
                if (!player.photonView.IsMine)
                    return;

                Debug.Log("PlayerArmorManager OnSceneLoaded");

                foreach (GameObject model in models)
                    if(!feet.Contains(model))
                        GameManager.SetLayerRecursively(model, 31);
            }
            catch (System.Exception e) { Debug.Log(e); }
        }
    }
}
