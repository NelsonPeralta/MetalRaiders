using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionTrackerDot : MonoBehaviour
{
    public Biped biped
    {
        get { return _biped; }
        set
        {
            _biped = value;

            if (_biped.GetComponent<Player>()) targetPlayerController = _biped.GetComponent<PlayerController>();

            try { _actor = _biped.GetComponent<Actor>(); } catch { }
        }
    }


    public PlayerController targetPlayerController
    {
        get { return _targetPlayerController; }
        private set
        {
            _targetPlayerController = value;

            if (GameManager.instance.gameType != GameManager.GameType.Pro &&
                GameManager.instance.gameType != GameManager.GameType.Swat &&
                GameManager.instance.gameType != GameManager.GameType.Snipers &&
                 GameManager.instance.gameType != GameManager.GameType.Retro)
            {
                if (_targetPlayerController.player == transform.root.GetComponent<Player>())
                {
                    _spriteRenderer.color = Color.green;
                }
                else
                {
                    if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                    {
                        _spriteRenderer.color = Color.red;

                    }
                    else
                    {
                        if (_targetPlayerController.player.team == transform.root.GetComponent<Player>().team)
                        {
                            _spriteRenderer.color = Color.green;

                        }
                        else
                        {
                            _spriteRenderer.color = Color.red;
                        }
                    }
                }


                _dotHolder.SetActive(true);
            }
        }
    }


    [SerializeField] Biped _biped;
    [SerializeField] PlayerController _targetPlayerController;


    [SerializeField] GameObject _dotHolder;
    [SerializeField] Transform _dotRotation, _dotDistance;

    [SerializeField] float _tarDistance, _tarAngle, _divider;

    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] Vector3 _dirToTar;

    Vector3 _tp, _tpc, _tarpc, _tarRot, _tarDisV;

    GameManager.Team _rootPlayerTeam;
    Actor _actor;





    private void Awake()
    {
        if (_divider == 0) _divider = 1;
        _dotHolder.SetActive(false);
        _rootPlayerTeam = transform.root.GetComponent<Player>().team;
    }



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (targetPlayerController != null)
        {
            _tpc = transform.position; _tpc.y = 0;
            _tarpc = targetPlayerController.transform.position; _tarpc.y = 0;

            _tarDistance = Vector3.Distance(_tpc, _tarpc);

            if (_tarDistance <= 20)
            {
                _dirToTar = _tarpc - _tpc;

                _dotHolder.SetActive(true);
                _tarAngle = Vector3.SignedAngle(transform.forward, _dirToTar, Vector3.up);
                _tarRot = new Vector3(0, _tarAngle, 0);
                _tarDisV = new Vector3(0, 0, _tarDistance);


                _dotRotation.localRotation = Quaternion.Euler(0, _tarAngle, 0);
                _dotDistance.localPosition = _tarDisV / _divider;


                _tp = targetPlayerController.transform.position / _divider; _tp.y = 0;

                //_dotHolder.transform.localPosition = _tp;

                if (targetPlayerController.player.movement.isMoving)
                {
                    _dotHolder.SetActive(targetPlayerController.player.movement.isMoving);
                    _dotHolder.SetActive(!targetPlayerController.isCrouching);

                }
                else
                {
                    _dotHolder.SetActive(false);
                }

                if (!targetPlayerController.player.movement.isGrounded) _dotHolder.SetActive(true);
                if (targetPlayerController.isCurrentlyShootingForMotionTracker) _dotHolder.SetActive(true);

                //if (targetPlayerController.player.team == _rootPlayerTeam) _dotHolder.SetActive(true);
            }
            else
            {
                _dotHolder.SetActive(false);

            }


            if (targetPlayerController.player.isDead || targetPlayerController.player.isRespawning) _dotHolder.SetActive(false);

            //_dotHolder.SetActive(true);

        }
        else
        {
            if (GameManager.instance.gameMode == GameManager.GameMode.Coop && _actor)
            {
                _tpc = transform.position; _tpc.y = 0;
                _tarpc = _actor.transform.position; _tarpc.y = 0;

                _tarDistance = Vector3.Distance(_tpc, _tarpc);

                if (_tarDistance <= 15)
                {
                    _dirToTar = _tarpc - _tpc;

                    _tarAngle = Vector3.SignedAngle(transform.forward, _dirToTar, Vector3.up);
                    _tarRot = new Vector3(0, _tarAngle, 0);
                    _tarDisV = new Vector3(0, 0, _tarDistance);


                    _dotRotation.localRotation = Quaternion.Euler(0, _tarAngle, 0);
                    _dotDistance.localPosition = _tarDisV / _divider;

                    _dotHolder.SetActive(true);

                    _dotHolder.SetActive(_actor.hitPoints > 0);
                }
                else
                {
                    _dotHolder.SetActive(false);

                }
            }
        }
    }
}