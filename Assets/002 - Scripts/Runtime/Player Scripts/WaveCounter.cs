using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveCounter : MonoBehaviour
{
    public Text waveText;

    public IEnumerator UpdateWaveNumber(int waveNumber)
    {
        yield return new WaitForSeconds(3.5f);

        if (waveText != null)
            waveText.gameObject.SetActive(false);

        if (waveText != null)
            waveText.gameObject.GetComponent<Text>().text = "Wave " + waveNumber;
        yield return new WaitForSeconds(.25f);

        if (waveText != null)
            waveText.gameObject.SetActive(true);
        yield return new WaitForSeconds(.25f);

        if (waveText != null)
            waveText.gameObject.SetActive(false);
        yield return new WaitForSeconds(.25f);

        if (waveText != null)
            waveText.gameObject.SetActive(true);
        yield return new WaitForSeconds(.25f);

        if (waveText != null)
            waveText.gameObject.SetActive(false);
        yield return new WaitForSeconds(.25f);

        if (waveText != null)
            waveText.gameObject.SetActive(true);
        yield return new WaitForSeconds(.25f);
    }
}
