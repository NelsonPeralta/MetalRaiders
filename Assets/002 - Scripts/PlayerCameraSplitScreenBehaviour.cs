using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraSplitScreenBehaviour : MonoBehaviour
{
    Player player;
    Camera camera;

    public enum CameraType { Main, Gun, UI, World }
    public CameraType cameraType;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.root.GetComponent<Player>();
        camera = GetComponent<Camera>();

        int playerRewiredId = player.GetComponent<PlayerController>().rid;

        if (cameraType == CameraType.Main)
        {
            if (playerRewiredId == 0)
                GameManager.instance.DisableCameraMaskLayer(camera, "Player 1 TPS Models (LOS Ray)");
            else if (playerRewiredId == 1)
                GameManager.instance.DisableCameraMaskLayer(camera, "Player 2 TPS Models");
        }
        else if (cameraType == CameraType.Gun)
        {
            if (playerRewiredId == 0)
                GameManager.instance.EnableCameraMaskLayer(camera, "Player 1 FPS Models (Physical Loot)");
            else if (playerRewiredId == 1)
                GameManager.instance.EnableCameraMaskLayer(camera, "Player 2 FPS Models");
        }

        if (GameManager.instance.NbPlayers > 1)
        {
            camera.rect = new Rect(0, 0, 1, 0.5f);

            if (playerRewiredId == 0)
                camera.rect = new Rect(0, 0.5f, camera.rect.width, camera.rect.height);

            if (playerRewiredId == 1)
            {
                camera.rect = new Rect(0, 0f, camera.rect.width, camera.rect.height);
                try { Destroy(GetComponent<AudioListener>()); } catch { }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
