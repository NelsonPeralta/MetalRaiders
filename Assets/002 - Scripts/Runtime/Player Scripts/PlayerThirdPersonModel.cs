using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerThirdPersonModel : MonoBehaviour
{
    private void Awake()
    {
        return;
        var components = GetComponents<Component>().Concat(GetComponentsInChildren<Component>()).ToArray();
        foreach (var t in components)
        {
            if (t is Rigidbody || t is Renderer || t is MeshFilter || t is MeshRenderer || t is SkinnedMeshRenderer ||
                t is UnityEngine.ProBuilder.ProBuilderMesh)
                continue;
            Destroy(t);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
