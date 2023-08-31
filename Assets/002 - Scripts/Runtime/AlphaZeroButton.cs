using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AlphaZeroButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            GameObject rag = Instantiate(GameManager.GetRootPlayer().thirdPersonModels, position: new Vector3(0, 0, 0), Quaternion.identity, null);

            var components = rag.GetComponents<Component>().Concat(rag.GetComponentsInChildren<Component>()).ToArray();
            foreach (var t in components)
            {
                if (t is Transform || t is Renderer || t is MeshFilter || t is MeshRenderer || t is SkinnedMeshRenderer ||
                    t is UnityEngine.ProBuilder.ProBuilderMesh)
                    continue;
                Destroy(t);

                try
                {
                    t.GetComponent<Renderer>().material.SetTexture("_MainTex", t.GetComponent<Renderer>().material.mainTexture);
                }
                catch { }
            }
        }
    }
}
