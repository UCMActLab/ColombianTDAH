using UnityEngine;

public class MixerRotation : MonoBehaviour
{
    [Header("Refs")]
    public Transform whisk;          // El transform de la varilla (eje local sY)
    public ParticleSystem splash;

    [Header("Ajustes")]
    public float targetRPM = 300f;   // RPM durante la mezcla
    public float accel = 8f;         // Quiebre de aceleración/frenado (lerp)

    float _degPerSec;                // velocidad actual en grados/seg
    bool _spinning;

    void Awake()
    {
        if (!whisk) whisk = transform;
    }

    public void Play(float rpm = -1f)
    {
        Debug.Log("A");
        if (rpm > 0f) targetRPM = rpm;
        _spinning = true;
        splash.Play();
    }

    public void Stop()
    {
        _spinning = false;
        splash.Stop();
    }

    void Update()
    {
        // rpm → grados/seg (rpm * 360 / 60 = rpm * 6)
        float targetDps = (_spinning ? targetRPM * 6f : 0f);
        _degPerSec = Mathf.Lerp(_degPerSec, targetDps, Time.deltaTime * accel);
        if (whisk) whisk.Rotate(Vector3.forward, _degPerSec * Time.deltaTime, Space.Self);
    }

    void OnDisable()
    {
        // Seguridad: detener si el objeto se desactiva a mitad de proceso
        _spinning = false;
        _degPerSec = 0f;
    }
}
