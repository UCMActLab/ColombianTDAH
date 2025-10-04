using UnityEngine;

public class AudioRegister : MonoBehaviour
{
    [SerializeField]
    int _audioIndex;

    AudioSource _audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        // Registra en el manager de sonido el audio deseado
        SoundManager.Instance.RegisterSound((SoundManager.SoundName)_audioIndex, _audioSource);
    }
}
