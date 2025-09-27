using System.Collections;
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
    Sarten, 
    Esponja,
    RecetaEntregada,
    HandGrab,
    HandDrop,
    AmbienceMorning,
    AmbienceAfternoon,
    AmbienceNight,
    Win,
    GameOver

}

/// <summary>
/// SoundManager singleton para gestionar los sonidos del juego.
/// </summary>
public class KitchenSoundManager : MonoBehaviour
{
    /// <summary>Instancia publica de SoundManager.</summary>
    private static KitchenSoundManager _instance = null;

    static public KitchenSoundManager Instance { get { return _instance; } }

    [Header("Fades")]
    [SerializeField] private bool queuePlaysWithFadesScheduled = true;
    [SerializeField] private float defaultSfxFadeIn = 0.06f;
    [SerializeField] private float defaultSfxFadeOut = 0.06f;
    [SerializeField] private float defaultLoopFadeIn = 0.3f;  
    [SerializeField] private float defaultLoopFadeOut = 0.3f;  

    private Dictionary<ObjetosSound, AudioSource> sources = new Dictionary<ObjetosSound, AudioSource>();
    private Dictionary<ObjetosSound, Dictionary<string, AudioClip>> clips = new Dictionary<ObjetosSound, Dictionary<string, AudioClip>>();
    private Dictionary<ObjetosSound, Queue<AudioClip>>  audioQueues = new Dictionary<ObjetosSound, Queue<AudioClip>>();
    private readonly Dictionary<ObjetosSound, Coroutine> runningRoutines = new();
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
                var src = sources[par.Key];
                if (src != null)
                {
                    if (!src.isPlaying && par.Value.Count > 0)
                    {
                        AudioClip nextClip = par.Value.Dequeue();

                        if (queuePlaysWithFadesScheduled)
                        {
                            StartCoroutine(PlayOneShotWithFadesScheduled(src, nextClip, defaultSfxFadeIn, defaultSfxFadeOut, src.volume));
                        }
                        else
                        {    
                            src.PlayOneShot(nextClip);
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// Reproduce uno de los clips segun el argumento recibido.
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

    /// <summary>
    /// Reproduce inmediatamente un SFX con fade-in/out sin pasar por la cola.
    /// Mantiene exactamente la duración del clip.
    /// </summary>
    public void PlaySoundFaded(ObjetosSound ob, string clipName, float fadeIn, float fadeOut, float targetVolume = -1f)
    {
        if (!sources.ContainsKey(ob) || sources[ob] == null)
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
        var clip = clips[ob][clipName];
        float tgt = (targetVolume >= 0f) ? targetVolume : src.volume;

        StartCoroutine(PlayOneShotWithFadesScheduled(src, clip, fadeIn, fadeOut, tgt));
    }

    public void PlayOneShotRaw(ObjetosSound ob, string clipName, float volumeScale = 1f)
    {
        if (!sources.ContainsKey(ob) || sources[ob] == null)
        {
            Debug.LogWarning($"[SoundManager] No hay AudioSource registrado para {ob} (PlayOneShotRaw)");
            return;
        }
        if (!clips.ContainsKey(ob) || !clips[ob].ContainsKey(clipName))
        {
            Debug.LogWarning($"[SoundManager] Clip '{clipName}' no encontrado para {ob} (PlayOneShotRaw)");
            return;
        }

        var src = sources[ob];
        var clip = clips[ob][clipName];

        src.PlayOneShot(clip, volumeScale);
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

    /// <summary>Arranca un loop con fade-in, respetando la sincronía (DSP-safe).</summary>
    public void PlayLoopFaded(ObjetosSound ob, string clipName, float fadeIn = -1f, float targetVolume = -1f)
    {
        if (!sources.ContainsKey(ob) || sources[ob] == null)
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
        var clip = clips[ob][clipName];
        float fin = (fadeIn >= 0f) ? fadeIn : defaultLoopFadeIn;
        float tgt = (targetVolume >= 0f) ? targetVolume : src.volume;

        StartOrSwapRoutine(ob, PlayLoopWithFadeInScheduled(src, clip, fin, tgt));
    }

    /// <summary>Detiene un loop con fade-out, sin “alargar” ni cortar bruscamente (DSP-safe).</summary>
    public void StopLoopFaded(ObjetosSound ob, float fadeOut = -1f)
    {
        if (!sources.ContainsKey(ob) || sources[ob] == null) return;

        var src = sources[ob];
        float fout = (fadeOut >= 0f) ? fadeOut : defaultLoopFadeOut;

        StartOrSwapRoutine(ob, FadeOutAndStopLoopScheduled(src, fout));
    }

    public void SetAudioSource(ObjetosSound ob, AudioSource source)
    {
        sources[ob] = source;
    }

    public void SetClip(ObjetosSound ob, AudioClip clip)
    {
        clips[ob][clip.name] = clip;
    }

    // =========================================================
    // Reproducción programada (DSP) con fades in/out
    // Mantiene exactamente la duración del clip.
    // =========================================================
    private IEnumerator PlayOneShotWithFadesScheduled(AudioSource src, AudioClip clip, float fadeIn, float fadeOut, float targetVol = -1f)
    {
        if (src == null || clip == null) yield break;

        // Margen de seguridad para programar (evita clicks si llamas el mismo frame)
        const double safetyLead = 0.02; // 20 ms
        double t0 = AudioSettings.dspTime + safetyLead;
        double tEnd = t0 + clip.length;

        // Volumen objetivo
        float baseVol = (targetVol >= 0f) ? targetVol : src.volume;

        // Configuramos el source para reproducir el clip completo exactamente
        src.clip = clip;
        src.loop = false;
        src.volume = 0f;

        src.PlayScheduled(t0);
        src.SetScheduledEndTime(tEnd);

        // Esperamos al comienzo real en reloj DSP
        yield return new WaitUntil(() => AudioSettings.dspTime >= t0);

        // Fade-in dentro del clip
        float inDur = Mathf.Max(0f, fadeIn);
        if (inDur > 0f)
        {
            double inEnd = t0 + inDur;
            while (AudioSettings.dspTime < inEnd)
            {
                double k = (AudioSettings.dspTime - t0) / inDur;
                src.volume = Mathf.Lerp(0f, baseVol, (float)k);
                yield return null;
            }
        }
        src.volume = baseVol;

        // Esperamos hasta el punto donde debe iniciar el fade-out (dentro del clip)
        float outDur = Mathf.Max(0f, fadeOut);
        double outStart = tEnd - outDur;

        while (AudioSettings.dspTime < outStart)
            yield return null;

        // Fade-out hasta el final programado (sin alargar)
        if (outDur > 0f)
        {
            while (AudioSettings.dspTime < tEnd)
            {
                double k = (AudioSettings.dspTime - outStart) / outDur;
                src.volume = Mathf.Lerp(baseVol, 0f, (float)k);
                yield return null;
            }
        }
  
        src.volume = baseVol; // Restauramos para futuros usos
        src.clip = null;
    }

    private IEnumerator PlayLoopWithFadeInScheduled(AudioSource src, AudioClip clip, float fadeIn, float targetVol)
    {
        if (src == null || clip == null) yield break;

        const double safetyLead = 0.02; // 20 ms para planificar sin clicks
        double t0 = AudioSettings.dspTime + safetyLead;

        // Preparamos source
        src.clip = clip;
        src.loop = true;
        float prevVol = src.volume; 
        src.volume = 0f;

        // Programamos inicio exacto
        src.PlayScheduled(t0);

        // Esperamos a que empiece realmente
        yield return new WaitUntil(() => AudioSettings.dspTime >= t0);

        // Fade-in dentro del propio loop (no afecta duración)
        float fin = Mathf.Max(0f, fadeIn);
        if (fin > 0f)
        {
            double tEnd = t0 + fin;
            while (AudioSettings.dspTime < tEnd)
            {
                double k = (AudioSettings.dspTime - t0) / fin;
                src.volume = Mathf.Lerp(0f, targetVol, (float)k);
                yield return null;
            }
        }
        src.volume = targetVol;
    }

    private IEnumerator FadeOutAndStopLoopScheduled(AudioSource src, float fadeOut)
    {
        if (src == null || src.clip == null) yield break;

        // Programamos un final exacto del loop dentro de fadeOut segundos
        float fout = Mathf.Max(0f, fadeOut);
        const double safetyLead = 0.0; 
        double now = AudioSettings.dspTime;
        double tEnd = now + fout + safetyLead;

        // Guardamos volumen para restaurarlo después
        float startVol = src.volume;

        // Programamos fin exacto del audio
        src.SetScheduledEndTime(tEnd);

        // Fade-out hasta tEnd 
        if (fout > 0f)
        {
            double t0 = now;
            while (AudioSettings.dspTime < tEnd)
            {
                double k = (AudioSettings.dspTime - t0) / fout;
                src.volume = Mathf.Lerp(startVol, 0f, (float)k);
                yield return null;
            }
        }

        src.loop = false;
        src.clip = null;
        src.volume = startVol;
    }


    public void PlayLoopForDurationFaded(ObjetosSound ob, string clipName, float totalDuration, float fadeIn = -1f, float fadeOut = -1f, float targetVolume = -1f)
    {
        if (!sources.ContainsKey(ob) || sources[ob] == null)
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
        var clip = clips[ob][clipName];

        float fin = (fadeIn >= 0f) ? fadeIn : defaultLoopFadeIn;
        float fout = (fadeOut >= 0f) ? fadeOut : defaultLoopFadeOut;
        float tgt = (targetVolume >= 0f) ? targetVolume : src.volume;
        
        totalDuration = Mathf.Max(0.01f, totalDuration);
        fin = Mathf.Clamp(fin, 0f, totalDuration);
        fout = Mathf.Clamp(fout, 0f, totalDuration);

        StartOrSwapRoutine(ob, PlayLoopForDurationFadedRoutine(src, clip, totalDuration, fin, fout, tgt));
    }

    private IEnumerator PlayLoopForDurationFadedRoutine(AudioSource src, AudioClip clip, float totalDuration, float fadeIn, float fadeOut, float targetVol)
    {
        if (src == null || clip == null) yield break;

        const double safetyLead = 0.02; // 20 ms
        double t0 = AudioSettings.dspTime + safetyLead;
        double tEnd = t0 + totalDuration;

        // Programación DSP exacta
        src.clip = clip;
        src.loop = true;
        float prevVol = src.volume;
        src.volume = 0f;

        src.PlayScheduled(t0);
        src.SetScheduledEndTime(tEnd);

        // Esperamos inicio real
        yield return new WaitUntil(() => AudioSettings.dspTime >= t0);

        // Fade-in dentro del intervalo
        if (fadeIn > 0f)
        {
            double inEnd = t0 + fadeIn;
            while (AudioSettings.dspTime < inEnd)
            {
                double k = (AudioSettings.dspTime - t0) / fadeIn;
                src.volume = Mathf.Lerp(0f, targetVol, (float)k);
                yield return null;
            }
        }
        src.volume = targetVol;

        // Calculamos inicio del fade-out dentro del total
        double outStart = tEnd - fadeOut;
 
        while (AudioSettings.dspTime < outStart)
            yield return null;

        // Fade-out hasta tEnd 
        if (fadeOut > 0f)
        {
            while (AudioSettings.dspTime < tEnd)
            {
                double k = (AudioSettings.dspTime - outStart) / fadeOut;
                src.volume = Mathf.Lerp(targetVol, 0f, (float)k);
                yield return null;
            }
        }
   
        src.loop = false;
        src.clip = null;
        src.volume = prevVol;
    }

    private void StartOrSwapRoutine(ObjetosSound key, IEnumerator routine)
    {
        if (runningRoutines.TryGetValue(key, out var r) && r != null)
            StopCoroutine(r);
        runningRoutines[key] = StartCoroutine(routine);
    }
}
