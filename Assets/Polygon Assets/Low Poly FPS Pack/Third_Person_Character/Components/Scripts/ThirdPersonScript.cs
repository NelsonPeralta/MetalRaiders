using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonScript : MonoBehaviour {

	public Animator anim;
    public Movement movement;
    public FPSControllerLPFP.FpsControllerLPFP notMyFPSController;
    public int playerRewiredID;
    public List<SkinnedMeshRenderer> meshes = new List<SkinnedMeshRenderer>();
    public Transform handBone;

	private void Update () 
	{
        ///////////////////////////////////////////////////////////////////////////////////// Controller 
        ///
        anim.SetFloat("Vertical", movement.zDirection, 1f, Time.deltaTime * 10f);
        anim.SetFloat("Horizontal", movement.xDirection, 1f, Time.deltaTime * 10f);
        /*
        if (movement.directionIndicator == 1 && movement.isGrounded)
        {
            anim.SetFloat("Vertical", 0f, 0, Time.deltaTime);
            anim.SetFloat("Horizontal", -1.0f, 0, Time.deltaTime);
        }
        else if (movement.directionIndicator == 2 && movement.isGrounded)
        {
            anim.SetFloat("Vertical", 1.0f, 0, Time.deltaTime);
            anim.SetFloat("Horizontal", -1.0f, 0, Time.deltaTime);
        }
        else if (movement.directionIndicator == 3 && movement.isGrounded)
        {
            anim.SetFloat("Vertical", 1.0f, 0, Time.deltaTime);
            anim.SetFloat("Horizontal", 0f, 0, Time.deltaTime);
        }
        else if (movement.directionIndicator == 4 && movement.isGrounded)
        {
            anim.SetFloat("Vertical", 1.0f, 0, Time.deltaTime);
            anim.SetFloat("Horizontal", 1.0f, 0, Time.deltaTime);
        }
        else if (movement.directionIndicator == 5 && movement.isGrounded)
        {
            anim.SetFloat("Vertical", 0f, 0, Time.deltaTime);
            anim.SetFloat("Horizontal", 1.0f, 0, Time.deltaTime);
        }
        else if (movement.directionIndicator == 6 && movement.isGrounded)
        {
            anim.SetFloat("Vertical", -1.0f, 0, Time.deltaTime);
            anim.SetFloat("Horizontal", 1.0f, 0, Time.deltaTime);
        }
        else if (movement.directionIndicator == 7 && movement.isGrounded)
        {
            anim.SetFloat("Vertical", -1.0f, 0, Time.deltaTime);
            anim.SetFloat("Horizontal", 0f, 0, Time.deltaTime);
        }
        else if (movement.directionIndicator == 8 && movement.isGrounded)
        {
            anim.SetFloat("Vertical", -1.0f, 0, Time.deltaTime);
            anim.SetFloat("Horizontal", -1.0f, 0, Time.deltaTime);
        }
        else
        {
            anim.SetFloat("Vertical", 0f, 0, Time.deltaTime);
            anim.SetFloat("Horizontal", 0.0f, 0, Time.deltaTime);

        }*/
	}

    public void EnableSkinnedMeshes()
    {
        foreach (SkinnedMeshRenderer smr in meshes)
            smr.enabled = true;
    }
    public void DisableSkinnedMeshes()
    {
        foreach (SkinnedMeshRenderer smr in meshes)
            smr.enabled = false;
    }
}