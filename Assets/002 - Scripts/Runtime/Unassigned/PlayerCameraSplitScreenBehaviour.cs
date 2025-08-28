using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraSplitScreenBehaviour : MonoBehaviour
{
    public enum CameraType { Main, Gun, UI, World }
    public CameraType cameraType;

    public Player player { get { return _player; } }
    public Transform orignalParent { get { return _originalParent; } }

    [SerializeField] Player _player;
    Camera _camera;

    [SerializeField] Transform _originalParent;
    [SerializeField] AudioListener _audioListener;

    private void Awake()
    {
        _originalParent = transform.parent;
    }

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();


        if (_player.isMine)
        {
            _player.OnPlayerIdAssigned -= OnPlayerIdAndRewiredIdAssigned_Delegate;
            _player.OnPlayerIdAssigned += OnPlayerIdAndRewiredIdAssigned_Delegate;
        }
    }


    void OnPlayerIdAndRewiredIdAssigned_Delegate(Player p)
    {
        SetupCameraLayerMask_FirstPerson(p.rid);
        int playerRewiredId = p.rid;


        if (GameManager.instance.nbLocalPlayersPreset > 1)
        {
            if (GameManager.instance.nbLocalPlayersPreset == 2)
            {
                _camera.rect = new Rect(0, 0, 1, 0.5f);

                if (playerRewiredId == 0)
                    _camera.rect = new Rect(0, 0.5f, _camera.rect.width, _camera.rect.height);

                if (playerRewiredId == 1)
                {
                    _camera.rect = new Rect(0, 0f, _camera.rect.width, _camera.rect.height);
                    try { _audioListener.gameObject.SetActive(false); } catch { }
                }
            }





            if (GameManager.instance.nbLocalPlayersPreset == 3)
            {
                if (playerRewiredId == 0)
                    _camera.rect = new Rect(0, 0, 1, 0.5f);
                else
                    _camera.rect = new Rect(0, 0, 0.5f, 0.5f);

                if (playerRewiredId == 0)
                    _camera.rect = new Rect(0, 0.5f, _camera.rect.width, _camera.rect.height);
                else
                    try { _audioListener.gameObject.SetActive(false); } catch { }

                if (playerRewiredId == 1)
                    _camera.rect = new Rect(0, 0, _camera.rect.width, _camera.rect.height);

                if (playerRewiredId == 2)
                    _camera.rect = new Rect(0.5f, 0f, _camera.rect.width, _camera.rect.height);
            }






            if (GameManager.instance.nbLocalPlayersPreset == 4)
            {
                _camera.rect = new Rect(0, 0, 0.5f, 0.5f);

                if (playerRewiredId == 0)
                    _camera.rect = new Rect(0, 0.5f, _camera.rect.width, _camera.rect.height);
                else
                    try { _audioListener.gameObject.SetActive(false); } catch { }

                if (playerRewiredId == 1)
                    _camera.rect = new Rect(0.5f, 0.5f, _camera.rect.width, _camera.rect.height);

                if (playerRewiredId == 2)
                    _camera.rect = new Rect(0f, 0f, _camera.rect.width, _camera.rect.height);

                if (playerRewiredId == 3)
                    _camera.rect = new Rect(0.5f, 0f, _camera.rect.width, _camera.rect.height);
            }
        }
    }



    public void SetupCameraLayerMask_FirstPerson(int playerRewiredId)
    {
        if (GameManager.instance.thirdPersonMode != GameManager.ThirdPersonMode.On)
        {
            if (cameraType == CameraType.Main)
            {
                GameManager.instance.EnableCameraMaskLayer(_camera, "Player 1 TPS Models (LOS Ray)");
                GameManager.instance.EnableCameraMaskLayer(_camera, "Player 2 TPS Models");
                GameManager.instance.EnableCameraMaskLayer(_camera, "Player 3 TPS Models");
                GameManager.instance.EnableCameraMaskLayer(_camera, "Player 4 TPS Models");



                if (playerRewiredId == 0)
                    GameManager.instance.DisableCameraMaskLayer(_camera, "Player 1 TPS Models (LOS Ray)");
                else if (playerRewiredId == 1)
                    GameManager.instance.DisableCameraMaskLayer(_camera, "Player 2 TPS Models");
                else if (playerRewiredId == 2)
                    GameManager.instance.DisableCameraMaskLayer(_camera, "Player 3 TPS Models");
                else if (playerRewiredId == 3)
                    GameManager.instance.DisableCameraMaskLayer(_camera, "Player 4 TPS Models");
            }
            else if (cameraType == CameraType.Gun)
            {
                if (playerRewiredId == 0)
                    GameManager.instance.EnableCameraMaskLayer(_camera, "Player 1 FPS Models (Physical Loot)");
                else if (playerRewiredId == 1)
                    GameManager.instance.EnableCameraMaskLayer(_camera, "Player 2 FPS Models");
                else if (playerRewiredId == 2)
                    GameManager.instance.EnableCameraMaskLayer(_camera, "Player 3 FPS Models");
                else if (playerRewiredId == 3)
                    GameManager.instance.EnableCameraMaskLayer(_camera, "Player 4 FPS Models");
            }
            else if (cameraType == CameraType.UI)
            {
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
    }
}
