using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationDebug : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent agentToDebug;

    private GameObject[] allAgentsGO;
    public NavMeshAgent[] allAgents;

    private LineRenderer lineRenderer;

    private ChildManager cManager;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        cManager = GameObject.FindGameObjectWithTag("Scene Manager").GetComponent<ChildManager>();
    }

    private void Update()
    {

        if(agentToDebug.hasPath)
        {
            lineRenderer.positionCount = agentToDebug.path.corners.Length;
            lineRenderer.SetPositions(agentToDebug.path.corners);
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }


    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////Unused
    /// </summary>
    void GetAllAgents()
    {
        int counter = 0;

        foreach (GameObject agentGO in cManager.allChildren)
        {
            if (agentGO.GetComponent<Tags>() != null)
            {
                if(agentGO.GetComponent<Tags>().tags[0] == "Zombie")
                {
                    allAgents[counter] = agentGO.GetComponent<NavMeshAgent>();
                    counter = counter + 1;
                }
                
            }
        }
    }
}
