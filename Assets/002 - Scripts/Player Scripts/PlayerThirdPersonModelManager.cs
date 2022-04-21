using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerThirdPersonModelManager : MonoBehaviour
{
    public Player player;
    public List<GameObject> models = new List<GameObject>();
    public List<GameObject> feet = new List<GameObject>();

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
