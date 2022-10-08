using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerImpactReceiver : MonoBehaviour
{
    [SerializeField] float mass = 60f; // defines the character mass
    Vector3 impact = Vector3.zero;
    CharacterController character = new CharacterController();

    void Start()
    {
        character = GetComponent<CharacterController>();
    }

    // call this function to add an impact force:
    public void AddImpact(Player ogPlayer, Vector3 dir, float force)
    {
        if (!ogPlayer.GetComponent<PhotonView>().IsMine)
            return;

        if (!GetComponent<PhotonView>().IsMine)
            GetComponent<PhotonView>().RPC("AddImpact_RPC", RpcTarget.All, dir, force);
    }

    [PunRPC]
    void AddImpact_RPC(Vector3 dir, float force)
    {
        if (!GetComponent<PhotonView>().IsMine)
            return;

        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground
        impact += dir.normalized * force / mass;
    }

    void Update()
    {
        // apply the impact force:
        if (impact.magnitude > 0.2) character.Move(impact * Time.deltaTime);
        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
    }
}
