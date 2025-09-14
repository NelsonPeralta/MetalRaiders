using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionTrackerCamera : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }


    public void ChooseRenderTexture(int rewiredId)
    {
        if (transform.root.GetComponent<Player>().isMine)
        {
            gameObject.SetActive(true);
            //GetComponent<Camera>().targetTexture = GameManager.instance.minimapRenderTextures[rewiredId];

            GetComponent<Camera>().targetTexture = Resources.Load<RenderTexture>($"MinimapRenderTextures/Minimap Render TexturePlayer{rewiredId}");
        }

    }
}
