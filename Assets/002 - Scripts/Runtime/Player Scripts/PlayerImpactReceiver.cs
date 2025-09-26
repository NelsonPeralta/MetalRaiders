using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerImpactReceiver : MonoBehaviour
{

    [SerializeField] float mass = 60f; // defines the character mass
    public Vector3 impact = Vector3.zero;
    CharacterController character = new CharacterController();


    [SerializeField] Player _player;
    [SerializeField] GroundCheck groundCheck;

    float _groundCheckCountdown;

    void Start()
    {
        character = GetComponent<CharacterController>();
    }

    // call this function to add an impact force:
    public void AddImpact(Player ogPlayer, Vector3 dir, float force)
    {
        if (!ogPlayer.isMine)
            return;

        if (!_player.isMine)
            _player.PV.RPC("AddImpact_RPC", RpcTarget.All, dir, force);
        else
            Impact(dir, force);
    }

    public void AddImpact(Vector3 dir, float force)
    {
        if (_player.isMine)
        {
            _groundCheckCountdown = 1f;
            Impact(dir, force);
        }
    }

    [PunRPC]
    void AddImpact_RPC(Vector3 dir, float force)
    {
        if (!_player.isMine)
            return;

        Log.Print(() =>"AddImpact_RPC");
        Impact(dir, force);
    }

    void Impact(Vector3 dir, float force)
    {
        dir.Normalize();
        //if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground
        impact += dir.normalized * force / mass;
    }

    void Update()
    {
        if (_player.isDead || _player.isRespawning)
            impact = Vector3.zero;

        // apply the impact force:
        if (impact.magnitude > 0.2) character.Move(impact * Time.deltaTime);
        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, 1 * Time.deltaTime);

        if (_groundCheckCountdown > 0)
            _groundCheckCountdown -= Time.deltaTime;

        //if (_groundCheckCountdown <= 0)
        //{
        //    if (groundCheck.isGrounded && impact.magnitude > 0)
        //    {
        //        impact *= 0;
        //    }
        //}
    }

    public void OnGrounded_Event(GroundCheck gc)
    {
        if (_groundCheckCountdown <= 0)
            impact *= 0;
    }
}
