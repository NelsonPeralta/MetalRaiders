using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RagdollPool : MonoBehaviour
{
    public static RagdollPool instance;


    public GameObject ragdollPrefab;
    public List<GameObject> ragdollPoolList = new List<GameObject>();


    private void Awake()
    {
        instance = this;

        for (int j = 0; j < CurrentRoomManager.instance.expectedNbPlayers; j++)
        {
            GameObject obj = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            ragdollPoolList.Add(obj);
            obj.transform.parent = gameObject.transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }


    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        //Debug.Log("RagdollPool");
        //if (scene.buildIndex == 0)
        //{
        //    foreach (GameObject rd in ragdolls) Destroy(rd);
        //}
        //else
        //{
        //    GameObject obj = Instantiate(ragdollPrefab, transform.position, transform.rotation);
        //    obj.SetActive(false);
        //    ragdolls.Add(obj);
        //    obj.transform.parent = gameObject.transform;
        //}
    }


    public GameObject GetPooledPlayerRagdoll(int pDataCellInd, bool isMine)
    {
        GameObject obj = ragdollPoolList[pDataCellInd];

        obj.transform.parent = null;
        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene()); // Undos DontDestroyOnLoad
        StartCoroutine(ChangeRagdollLayer(obj, Player.RESPAWN_TIME * 0.95f));
        StartCoroutine(DisableRagdollAfterTime(obj, Player.RESPAWN_TIME));
        obj.GetComponent<PlayerRagdoll>().isMine = isMine;
        return obj;



        //foreach (GameObject obj in ragdollPoolList)
        //    if (!obj.activeInHierarchy)
        //        if (!obj.activeInHierarchy)
        //        {
        //            obj.transform.parent = null;
        //            SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene()); // Undos DontDestroyOnLoad
        //            ragdollPoolList.Remove(obj);
        //            StartCoroutine(DisableObjectAfterTime(obj, 20));
        //            obj.GetComponent<PlayerRagdoll>().isMine = isMine;
        //            return obj;
        //        }
        //return null;
    }


    public GameObject SpawnPooledPlayerRagdoll(bool isMine)
    {
        foreach (GameObject obj in ragdollPoolList)
            if (!obj.activeInHierarchy)
                if (!obj.activeInHierarchy)
                {
                    obj.transform.parent = null;
                    SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene()); // Undos DontDestroyOnLoad
                    ragdollPoolList.Remove(obj);
                    StartCoroutine(DisableObjectAfterTime(obj, 20));
                    obj.GetComponent<PlayerRagdoll>().isMine = isMine;
                    return obj;
                }
        return null;
    }

    IEnumerator DisableObjectAfterTime(GameObject obj, int time = 1)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }



    IEnumerator ChangeRagdollLayer(GameObject obj, float time = 1)
    {
        yield return new WaitForSeconds(time);
        GameManager.SetLayerRecursively(obj.transform.GetChild(0).gameObject, 3);
        obj.transform.GetChild(1).gameObject.layer = 3;
    }

    IEnumerator DisableRagdollAfterTime(GameObject obj, int time = 1)
    {
        yield return new WaitForSeconds(time);
        obj.GetComponent<PlayerRagdoll>().ToggleAllRigidbodiesToKinetmatic(true);
        //transform.position = Vector3.one * -1000; // does not work
        //transform.SetPositionAndRotation(Vector3.one * -222, Quaternion.identity); // does not work
        obj.GetComponent<Animator>().enabled = true;
        StartCoroutine(DisableRagdoll(obj));
    }

    IEnumerator DisableRagdoll(GameObject obj)
    {
        yield return new WaitForEndOfFrame();

        obj.SetActive(false);
        GameManager.SetLayerRecursively(obj.transform.GetChild(0).gameObject, 10);
        obj.transform.GetChild(1).gameObject.layer = 0;
    }
}
