using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasterEggFindThreeObjects : MonoBehaviour
{
    [Header("Sound FX")]
    public AudioSource audioSource;
    public AudioClip activateNoise;
    public AudioClip completedNoise;

    [Header("GameObjects")]
    public GameObject[] objects;

    [Header("Reward")]
    public GameObject reward;
        
    public bool allobjectsHaveBeenActivated;
    

    public void updateActivatedObjects(FindableObject fo)
    {
        foreach(GameObject go in objects)
        {
            if(go == fo.gameObject)
            {
                //fo.isActivated = true;
            }            
        }

        int numberOfActiavtedObjects = 0, totalObjects = objects.Length;        

        for (int i = 0; i < objects.Length; i++)
        {
            //if(objects[i].GetComponent<FindableObject>().isActivated)
            //{
            //    numberOfActiavtedObjects = numberOfActiavtedObjects + 1;
            //    Debug.Log("Number of Activated objects = " + numberOfActiavtedObjects + " and Total Objects = " + totalObjects);
            //}
        }

        if(numberOfActiavtedObjects == totalObjects)
        {
            allobjectsHaveBeenActivated = true;
            audioSource.clip = completedNoise;
            audioSource.Play();
        }
        else
        {
            audioSource.clip = activateNoise;
            audioSource.Play();
        }

        if(allobjectsHaveBeenActivated)
        {
            GameObject rewardGO = Instantiate(reward, transform.position, transform.rotation);            
        }        
    }
}
