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
        int amountOfWeaponsToPool = 100;
        instance = this;



        for (int j = 0; j < amountOfWeaponsToPool; j++)
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
}
