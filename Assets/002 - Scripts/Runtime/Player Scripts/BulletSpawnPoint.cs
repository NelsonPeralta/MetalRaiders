using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletSpawnPoint : MonoBehaviourPun
{
    //Vector3 realPosition;
    //Quaternion realRotation;

    //private void Update()
    //{
    //    if (!photonView.IsMine)
    //    {
    //        transform.position = Vector3.Lerp(transform.position, realPosition, .1f);
    //        transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, .1f);
    //    }
    //}
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        stream.SendNext(transform.position);
    //        stream.SendNext(transform.rotation);
    //    }
    //    else
    //    {
    //        realPosition = (Vector3)stream.ReceiveNext();
    //        realRotation = (Quaternion)stream.ReceiveNext();
    //    }
    //}

    //public Vector3 GetRealPosition()
    //{
    //    return realPosition;
    //}

    //public Quaternion GetRealRotation()
    //{
    //    return realRotation;
    //}
}
