using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorMaterialProperties : MonoBehaviour
{
    private float value = 0.01f;
    private float TimeRate = 0;
    public float Timer = 2f;
    private bool Undo;
    public bool opacity = false;
    private Material mat;

    void Start()
    {
        Undo = false;
        var proj = GetComponent<Projector>();
        if (!proj.material.name.EndsWith("(Instance)"))
            proj.material = new Material(proj.material) { name = proj.material.name + " (Instance)" };
        mat = proj.material;
    }
    void Update ()
    {
        if (opacity == true && TimeRate <= Timer && Undo == false)
        {
            TimeRate += Time.deltaTime;
            value = Mathf.Lerp(1f, 0f, TimeRate / Timer);
            mat.SetFloat("_Opacity", value);
        }

        if (opacity == false)
        {
            if (TimeRate <= Timer && Undo == false)
            {
                TimeRate += Time.deltaTime;
                value = Mathf.Lerp(0.01f, 4f, TimeRate / Timer);
                mat.SetFloat("_MoveCirle", value);
            }

            if (TimeRate >= Timer && Undo == false)
            {
                check();
            }

            if (Undo == true && TimeRate <= Timer)
            {
                TimeRate += Time.deltaTime;
                value = Mathf.Lerp(4f, 0.01f, TimeRate / Timer);
                mat.SetFloat("_MoveCirle", value);
            }
        }
    }

    void check()
    {
        Undo = true;
        TimeRate = 0;
    }
}
