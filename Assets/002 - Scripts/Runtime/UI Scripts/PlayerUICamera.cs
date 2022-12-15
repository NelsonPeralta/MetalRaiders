using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUICamera : MonoBehaviour
{
    Player _player;
    Camera _camera;

    public enum CameraType { Main, Gun, UI, World }
    public CameraType cameraType;

    // Start is called before the first frame update
    void Start()
    {
        _player = transform.root.GetComponent<Player>();
        _camera = GetComponent<Camera>();

        if (!_player.isMine)
        {
            gameObject.SetActive(false);
            return;
        }



        int playerRewiredId = _player.GetComponent<PlayerController>().rid;

        if (playerRewiredId == 0)
            GameManager.instance.EnableCameraMaskLayer(_camera, "Player 1 UI (Motion Tracker)");
        else if (playerRewiredId == 1)
            GameManager.instance.EnableCameraMaskLayer(_camera, "Player 2 UI (Reticule Friction)");
        else if (playerRewiredId == 2)
            GameManager.instance.EnableCameraMaskLayer(_camera, "Player 3 UI (Frag Grenade)");
        else if (playerRewiredId == 3)
            GameManager.instance.EnableCameraMaskLayer(_camera, "Player 4 UI (PlayerMelee)");
    }
}
