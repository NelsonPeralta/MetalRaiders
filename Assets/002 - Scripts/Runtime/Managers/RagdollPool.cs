using MathNet.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RagdollPool : MonoBehaviour
{
    public static RagdollPool instance;


    public GameObject ragdollPrefab;
    public List<PlayerRagdoll> ragdollPoolList = new List<PlayerRagdoll>();


    static int RAGDOLLS_PER_PLAYER = 3;
    static int RAGDOLL_RESET_TIME = 30;


    private void Awake()
    {
        instance = this;

        for (int j = 0; j < CurrentRoomManager.instance.expectedNbPlayers * RAGDOLLS_PER_PLAYER; j++)
        {
            GameObject obj = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            obj.GetComponent<PlayerRagdoll>().ToggleAllRigidbodiesToKinetmatic(true);
            //obj.SetActive(false);
            ragdollPoolList.Add(obj.GetComponent<PlayerRagdoll>());
            obj.transform.parent = gameObject.transform;
            obj.name += obj.name + $" {j}";
            obj.transform.position = GameManager.RAGDOLL_HIDING_POSITION + (Vector3.right * (j + 1) * 3);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentRoomManager.instance.OnGameIsReady -= OnGameIsReady_Delegate;
        CurrentRoomManager.instance.OnGameIsReady += OnGameIsReady_Delegate;
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {

    }

    void OnGameIsReady_Delegate(CurrentRoomManager gme)
    {
        Debug.Log("OnGameIsReady_Delegate");


        int _currentlyCheckingThisPhotonRoomIndex = 1, _counter = 0;


        for (int i = 0; i < ragdollPoolList.Count; i++)
        {
            ragdollPoolList[i].GetComponent<PlayerArmorManager>().player = GameManager.GetPlayerWithPhotonRoomIndex(_currentlyCheckingThisPhotonRoomIndex);
            ragdollPoolList[i].GetComponent<PlayerArmorManager>().playerDataCell = CurrentRoomManager.GetDataCellWithPhotonRoomIndex(_currentlyCheckingThisPhotonRoomIndex);


            _counter++; if (_counter == RAGDOLLS_PER_PLAYER) { _counter = 0; _currentlyCheckingThisPhotonRoomIndex++; }

            ragdollPoolList[i].GetComponent<Animator>().enabled = true;
        }
    }


    Vector3 _ragdollSpawnOffset = new Vector3(0.002251625f, -1.296722f, -0.2667971f);
    public GameObject GetPooledPlayerRagdoll(int photonRoomIndex, bool isMine, Transform spawnPoint)
    {
        GameObject obj = null;



        int _ragdollsNotInUse = 0; int _ragInd = -1;
        for (int i = (photonRoomIndex - 1) * RAGDOLLS_PER_PLAYER; i < (photonRoomIndex * RAGDOLLS_PER_PLAYER); i++) if (!ragdollPoolList[i].inUse) { _ragdollsNotInUse++; }

        for (int i = (photonRoomIndex - 1) * RAGDOLLS_PER_PLAYER; i < (photonRoomIndex * RAGDOLLS_PER_PLAYER); i++)
        {
            Log.Print($"GetPooledPlayerRagdoll {i} is active:{ragdollPoolList[i].gameObject.activeSelf} {ragdollPoolList[i].name} {ragdollPoolList[i].inUse}");

            if (!ragdollPoolList[i].inUse)
            {
                Log.Print($"GetPooledPlayerRagdoll returnin {i}");
                ragdollPoolList[i].inUse = true;
                obj = ragdollPoolList[i].gameObject;
                if (_ragdollsNotInUse == 1) _ragInd = i;
                break;
            }
        }


        // preparing a ragdoll when the last one ready has been taken
        if (_ragdollsNotInUse == 1)
        {
            Log.Print($"Pre-Preparing a ragdoll");
            for (int i = (photonRoomIndex - 1) * RAGDOLLS_PER_PLAYER; i < (photonRoomIndex * RAGDOLLS_PER_PLAYER); i++)
            {

                if (ragdollPoolList[i].inUse && _ragInd != i)
                {
                    Log.Print($"Pre-Preparing a ragdoll {i}");
                    if (ragdollPoolList[i].changeLayerCoroutine != null) StopCoroutine(ragdollPoolList[i].changeLayerCoroutine);
                    if (ragdollPoolList[i].resetRagdollCoroutine != null) StopCoroutine(ragdollPoolList[i].resetRagdollCoroutine);
                    if (ragdollPoolList[i].resetInUseCoroutine != null) StopCoroutine(ragdollPoolList[i].resetInUseCoroutine);

                    ragdollPoolList[i].changeLayerCoroutine = StartCoroutine(ChangeLayer_Coroutine(ragdollPoolList[i].gameObject, 0.25f));
                    ragdollPoolList[i].resetRagdollCoroutine = StartCoroutine(ResetRagdoll_Coroutine(ragdollPoolList[i].gameObject, 0.5f));
                    ragdollPoolList[i].resetInUseCoroutine = StartCoroutine(ResetInUse_Coroutine(ragdollPoolList.IndexOf(ragdollPoolList[i]), 1));

                    break;
                }
            }
        }


        obj.transform.rotation = spawnPoint.rotation; obj.transform.position = spawnPoint.position;
        Vector3 worldOffset = obj.transform.TransformDirection(_ragdollSpawnOffset);
        obj.transform.position += worldOffset;


        if (obj.GetComponent<PlayerRagdoll>().changeLayerCoroutine != null) StopCoroutine(obj.GetComponent<PlayerRagdoll>().changeLayerCoroutine);
        if (obj.GetComponent<PlayerRagdoll>().resetRagdollCoroutine != null) StopCoroutine(obj.GetComponent<PlayerRagdoll>().resetRagdollCoroutine);
        if (obj.GetComponent<PlayerRagdoll>().resetInUseCoroutine != null) StopCoroutine(obj.GetComponent<PlayerRagdoll>().resetInUseCoroutine);

        obj.transform.parent = null;
        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene()); // Undos DontDestroyOnLoad

        obj.GetComponent<PlayerRagdoll>().changeLayerCoroutine = StartCoroutine(ChangeLayer_Coroutine(obj, RAGDOLL_RESET_TIME));
        obj.GetComponent<PlayerRagdoll>().resetRagdollCoroutine = StartCoroutine(ResetRagdoll_Coroutine(obj, RAGDOLL_RESET_TIME + 1));
        obj.GetComponent<PlayerRagdoll>().resetInUseCoroutine = StartCoroutine(ResetInUse_Coroutine(ragdollPoolList.IndexOf(obj.GetComponent<PlayerRagdoll>()), RAGDOLL_RESET_TIME + 2));

        obj.GetComponent<PlayerRagdoll>().isMine = isMine;
        obj.GetComponent<PlayerRagdoll>().TriggerOnEnable();

        return obj;
    }


    public GameObject SpawnPooledPlayerRagdoll(bool isMine)
    {
        foreach (PlayerRagdoll obj in ragdollPoolList)
            if (obj.gameObject.activeInHierarchy)
            {
                obj.transform.parent = null;
                SceneManager.MoveGameObjectToScene(obj.gameObject, SceneManager.GetActiveScene()); // Undos DontDestroyOnLoad
                ragdollPoolList.Remove(obj);
                StartCoroutine(DisableObjectAfterTime(obj.gameObject, 20));
                obj.GetComponent<PlayerRagdoll>().isMine = isMine;
                return obj.gameObject;
            }
        return null;
    }

    IEnumerator DisableObjectAfterTime(GameObject obj, int time = 1)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }



    IEnumerator ChangeLayer_Coroutine(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

        GameManager.SetLayerRecursively(obj.transform.GetChild(0).gameObject, 3);
        obj.transform.GetChild(1).gameObject.layer = 3;
    }

    IEnumerator ResetRagdoll_Coroutine(GameObject obj, float time = 1)
    {
        yield return new WaitForSeconds(time);
        obj.transform.parent = transform;

        obj.GetComponent<PlayerRagdoll>().ToggleAllRigidbodiesToKinetmatic(true);
        obj.GetComponent<Animator>().enabled = true;

        GameManager.SetLayerRecursively(obj.transform.GetChild(0).gameObject, 0);
        obj.transform.GetChild(1).gameObject.layer = 0;


        obj.transform.position = GameManager.RAGDOLL_HIDING_POSITION + (Vector3.right * (ragdollPoolList.IndexOf(obj.GetComponent<PlayerRagdoll>()) + 1) * 3);
        obj.transform.rotation = Quaternion.identity;
    }


    IEnumerator DisableRagdoll(GameObject obj)
    {
        yield return new WaitForEndOfFrame();

        obj.SetActive(false);
        GameManager.SetLayerRecursively(obj.transform.GetChild(0).gameObject, 10);
        obj.transform.GetChild(1).gameObject.layer = 0;
    }

    IEnumerator ResetInUse_Coroutine(int iii, int time)
    {
        yield return new WaitForSeconds(time);

        ragdollPoolList[iii].inUse = false;
    }
}
