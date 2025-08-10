using System.Collections.Generic;
using UnityEngine;

public enum ObjetosSound
{
    Tabla_De_Picar,
    Mezcladora,
    Licuadora,
    Horno,
    Olla,
    Olla_A_Presion,
    Sarten
}

/// <summary>
/// SoundManager singleton para gestionar los sonidos del juego.
/// </summary>
public class SoundManager : MonoBehaviour
{
    /// <summary>Instancia publica de SoundManager.</summary>
    private static SoundManager _instance = null;

    static public SoundManager Instance { get { return _instance; } }

    private Dictionary<ObjetosSound, AudioSource> sources = new Dictionary<ObjetosSound, AudioSource>();
    private Dictionary<ObjetosSound, Dictionary<string, AudioClip>> clips = new Dictionary<ObjetosSound, Dictionary<string, AudioClip>>();
    private Dictionary<ObjetosSound, Queue<AudioClip>>  audioQueues = new Dictionary<ObjetosSound, Queue<AudioClip>>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        foreach (ObjetosSound tipo in System.Enum.GetValues(typeof(ObjetosSound)))
        {
            clips[tipo] = new Dictionary<string, AudioClip>();
            audioQueues[tipo] = new Queue<AudioClip>();
        }

    }

    void Update()
    {
        foreach (KeyValuePair<ObjetosSound, Queue<AudioClip>> par in audioQueues)
        {
            if (sources.ContainsKey(par.Key) && audioQueues.ContainsKey(par.Key))
            {
                if (!sources[par.Key].isPlaying && par.Value.Count > 0)
                {
                    AudioClip nextClip = par.Value.Dequeue();
                    sources[par.Key].PlayOneShot(nextClip);
                }
            }
        }
    }


    /// <summary>
    /// Reproduce uno de los clips del viejete seg�n el argumento recibido.
    /// </summary>
    /// <param name="clip">El clip a reproducir.</param>
    public void PlaySound(ObjetosSound ob, string clipName) 
    {
        if (clips.ContainsKey(ob) && clips[ob].ContainsKey(clipName))
        {
            AudioClip toPlay = clips[ob][clipName];
            if (toPlay != null)
                audioQueues[ob].Enqueue(toPlay);
        }
        else
        {
            Debug.LogWarning("Clip (" + clipName + ") no existente en este objeto: " + ob.ToString());
        }
    }

    public void PlayLoop(ObjetosSound ob, string clipName)
    {
        if (!sources.ContainsKey(ob))
        {
            Debug.LogWarning($"[SoundManager] No hay AudioSource registrado para {ob}");
            return;
        }

        if (!clips.ContainsKey(ob) || !clips[ob].ContainsKey(clipName))
        {
            Debug.LogWarning($"[SoundManager] Clip '{clipName}' no encontrado para {ob}");
            return;
        }

        var src = sources[ob];
        src.loop = true;
        src.clip = clips[ob][clipName];
        src.Play();
    }

    public void StopLoop(ObjetosSound ob)
    {
        if (!sources.ContainsKey(ob))
            return;

        var src = sources[ob];
        src.loop = false;
        src.Stop();
        src.clip = null;
    }



    public void SetAudioSource(ObjetosSound ob, AudioSource source)
    {
        sources[ob] = source;
    }

    public void SetClip(ObjetosSound ob, AudioClip clip)
    {
        clips[ob][clip.name] = clip;
    }
}
