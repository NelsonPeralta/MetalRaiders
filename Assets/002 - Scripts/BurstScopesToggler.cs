using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstScopesToggler : MonoBehaviour
{

    public BurstScript burstScript;

    private void Start()
    {
        burstScript = gameObject.GetComponent<BurstScript>();
    }

    public void EnableScopes()
    {
        
        if (burstScript.ironSights == true)
        {
            burstScript.ironSightsGO.gameObject.SetActive(true);
        }
        else
        {
            burstScript.ironSightsGO.gameObject.SetActive(false);
        }

        if (burstScript.scope1 == true)
        {
            burstScript.scope1GO.gameObject.SetActive(true);

            /*gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                scope1AimFOV, fovSpeed * Time.deltaTime);*/
        }
        else
        {
            burstScript.scope1GO.gameObject.SetActive(false);
        }

        if (burstScript.scope2 == true)
        {
            burstScript.scope2GO.gameObject.SetActive(true);

            /*gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                scope2AimFOV, fovSpeed * Time.deltaTime);*/
        }
        else
        {
            burstScript.scope2GO.gameObject.SetActive(false);
        }

        if (burstScript.scope3 == true)
        {
            burstScript.scope3GO.gameObject.SetActive(true);

            /*gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                scope3AimFOV, fovSpeed * Time.deltaTime);*/
        }
        else
        {
            burstScript.scope3GO.gameObject.SetActive(false);
        }

        if (burstScript.scope4 == true)
        {
            burstScript.scope4GO.gameObject.SetActive(true);

            /*gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                scope4AimFOV, fovSpeed * Time.deltaTime);*/
        }
        else
        {
            burstScript.scope4GO.gameObject.SetActive(false);
        }

        if (burstScript.silencer == true)
        {
            burstScript.silencerGO.gameObject.SetActive(true);

            /*gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                scope4AimFOV, fovSpeed * Time.deltaTime);*/
        }
        else
        {
            burstScript.silencerGO.gameObject.SetActive(false);
        }


    }
}
