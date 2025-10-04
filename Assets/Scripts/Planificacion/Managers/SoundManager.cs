using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum SoundName { UI_CLICK = 0, MOTOR, QUESTION, GOOD_ANSWER, BAD_ANSWER,MUSIC_LEVEL1, MUSIC_LEVEL2, MUSIC_LEVEL3, SOUND_NUMBER };

    // Singleton
    static private SoundManager _instance;
    public static SoundManager Instance { get { return _instance; } }

    AudioSource[] _audioSources = new AudioSource[(int)SoundName.SOUND_NUMBER];

    private void Awake()
    {
        // Si no hay instancia de esta clase ya creada se almacena
        if (_instance == null)
            _instance = this;
        // Si esta creada se destruye porque no necesitamos una mas
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    public void Click()
    {
        _audioSources[(int)SoundName.UI_CLICK].Play();
    }

    // Play del sonido que se indique como parametro
    public void PlaySound(SoundName sound)
    {
        if ((int)sound < _audioSources.Length)
            _audioSources[(int)sound].Play();
        else
            Debug.Log("El sonido indicado esta fuera del indice posible");
    }

    // Registra audio source que se indica como parametro
    public void RegisterSound(SoundName sound, AudioSource audioSource)
    {
        if (sound < SoundName.SOUND_NUMBER)
            _audioSources[(int)sound] = audioSource;
        else
            Debug.Log("El sonido indicado esta fuera del indice posible");
    }

    // Destruye el Sound Manager
    public void DestroySoundManager()
    {
        Destroy(gameObject);
    }
}
