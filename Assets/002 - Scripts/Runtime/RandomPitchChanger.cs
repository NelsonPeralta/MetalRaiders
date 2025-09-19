using Mono.Cecil;
using UnityEngine;

public class RandomPitchChanger : MonoBehaviour
{
    [Range(0.01f, 10f)]
    public float pitchRandomMultiplier = 1f;


    [SerializeField] AudioSource _audioSource;

    private void OnEnable()
    {
        //Multiply pitch
        if (pitchRandomMultiplier != 1)
        {
            if (Random.value < .5)
                _audioSource.pitch *= Random.Range(1 / pitchRandomMultiplier, 1);
            else
                _audioSource.pitch *= Random.Range(1, pitchRandomMultiplier);
        }
    }
}
