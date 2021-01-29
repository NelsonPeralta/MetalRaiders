using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarPulse : MonoBehaviour
{
    public Transform pulseTransform;
    public PlayerProperties pProperties;
    private float range;
    public float maxRange;
    public float rangeSpeed;

    [Header("Minimap Components")]
    public Camera minimapCamera;
    public RawImage rawImage;

    [Header("Render Textures")]
    public RenderTexture player0RT;
    public RenderTexture player1RT;
    public RenderTexture player2RT;
    public RenderTexture player3RT;

    // Start is called before the first frame update
    void OnEnable()
    {
        AssignCorrectMinimap();
    }

    // Update is called once per frame
    void Update()
    {
        //AssignCorrectMinimap();

        range += rangeSpeed * Time.deltaTime;
        if(range > maxRange)
        {
            range = 0f;
        }

        pulseTransform.localScale = new Vector3(range, range);
    }

    void AssignCorrectMinimap()
    {
        if(pProperties.playerRewiredID == 0)
        {
            Debug.Log("Gave Player 0 good minimap");
            minimapCamera.targetTexture = player0RT;
            rawImage.texture = player0RT;
        }

        if (pProperties.playerRewiredID == 1)
        {
            Debug.Log("Gave Player 1 good minimap");
            minimapCamera.targetTexture = player1RT;
            rawImage.texture = player1RT;
        }

        if (pProperties.playerRewiredID == 2)
        {
            minimapCamera.targetTexture = player2RT;
            rawImage.texture = player2RT;
        }

        if (pProperties.playerRewiredID == 3)
        {
            minimapCamera.targetTexture = player3RT;
            rawImage.texture = player3RT;
        }
    }
}
