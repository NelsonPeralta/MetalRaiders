using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarPulse : MonoBehaviour
{
    public Transform pulseTransform;
    public Player pProperties;
    private float range;
    public float maxRange;
    public float rangeSpeed;

    [Header("Minimap Components")]
    public Camera minimapCamera;
    public RawImage rawImage;


    float wait;

    // Start is called before the first frame update
    void OnEnable()
    {
        AssignCorrectMinimap();
    }

    private void Start()
    {
        AssignCorrectMinimap();
    }

    // Update is called once per frame
    void Update()
    {
        //AssignCorrectMinimap();


        if (range < maxRange && wait <= 0)
        {
            range += rangeSpeed * Time.deltaTime;

            if (range >= maxRange)
            {
                range = 0f;
                wait = 1f;
            }
        }

        if (wait > 0) { wait -= Time.deltaTime; }

        pulseTransform.localScale = new Vector3(range, range);
    }

    void AssignCorrectMinimap()
    {
        if (!pProperties.isMine)
            return;

        if (pProperties.controllerId == 0)
        {
            //Log.Print(() =>"Gave Player 0 good minimap");
            minimapCamera.targetTexture = Resources.Load<RenderTexture>($"MinimapRenderTextures/Minimap Render TexturePlayer0");
            rawImage.texture = Resources.Load<RenderTexture>($"MinimapRenderTextures/Minimap Render TexturePlayer0");
        }

        if (pProperties.controllerId == 1)
        {
            //Log.Print(() =>"Gave Player 1 good minimap");
            minimapCamera.targetTexture = Resources.Load<RenderTexture>($"MinimapRenderTextures/Minimap Render TexturePlayer1");
            rawImage.texture = Resources.Load<RenderTexture>($"MinimapRenderTextures/Minimap Render TexturePlayer1");
        }

        if (pProperties.controllerId == 2)
        {
            minimapCamera.targetTexture = Resources.Load<RenderTexture>($"MinimapRenderTextures/Minimap Render TexturePlayer2");
            rawImage.texture = Resources.Load<RenderTexture>($"MinimapRenderTextures/Minimap Render TexturePlayer2");
        }

        if (pProperties.controllerId == 3)
        {
            minimapCamera.targetTexture = Resources.Load<RenderTexture>($"MinimapRenderTextures/Minimap Render TexturePlayer3");
            rawImage.texture = Resources.Load<RenderTexture>($"MinimapRenderTextures/Minimap Render TexturePlayer3");
        }
    }
}
