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

            //Log.Print(() =>GameManager.GetRootPlayer().playerThirdPersonModel.transform.GetChild(1).name);
           // GameManager.SetupMaterialWithBlendMode(GameManager.GetRootPlayer().playerThirdPersonModel.transform.GetChild(1).GetComponent<Renderer>().material, GameManager.MaterialBlendMode.Transparent, true);
           // GameObject ar = GameManager.GetRootPlayer().playerThirdPersonModel.transform.GetChild(1).gameObject;

           //ar.GetComponent<Renderer>().material.color = new Color(ar.GetComponent<Renderer>().material.color.r, 
           //    ar.GetComponent<Renderer>().material.color.g, ar.GetComponent<Renderer>().material.color.b, 0.1f);



            return;
            GameObject rag = Instantiate(GameManager.GetRootPlayer().playerThirdPersonModel.gameObject, position: new Vector3(0, 0, 0), Quaternion.identity, null);

            var components = rag.GetComponents<Component>().Concat(rag.GetComponentsInChildren<Component>()).ToArray();
            foreach (var t in components)
            {
                if (t is Transform || t is Renderer || t is MeshFilter || t is MeshRenderer || t is SkinnedMeshRenderer ||
                    t is UnityEngine.ProBuilder.ProBuilderMesh)
                    continue;
                Destroy(t);
            }
        }
    }
}
