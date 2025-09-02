using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class Reloj : MonoBehaviour
{
    private TMP_Text textoReloj;
    private int tiempoInicialSegundos;
    public bool autoStart = false;

    private int tiempoRestante;
    private Coroutine rutina;

    // Evento público que se ejecuta al finalizar la animación
    public event Action OnTimeEnd;

    void Start()
    {
        textoReloj = GetComponent<TMP_Text>();
        tiempoRestante = tiempoInicialSegundos;
        if (autoStart) rutina = StartCoroutine(Contador());
        ActualizarTexto();
    }

    IEnumerator Contador()
    {
        while (tiempoRestante > 0)
        {
            yield return new WaitForSeconds(1f);
            tiempoRestante--;
            ActualizarTexto();
        }

        OnTimeEnd?.Invoke();
    }

    void ActualizarTexto()
    {
        int m = tiempoRestante / 60;
        int s = tiempoRestante % 60;
        textoReloj.text = $"{m:00}:{s:00}";
    }

    public void Pausar()
    {
        if (rutina != null) StopCoroutine(rutina);
        rutina = null;
    }

    public void Reanudar()
    {
        if (rutina == null && tiempoRestante > 0)
            rutina = StartCoroutine(Contador());
    }

    public void Reiniciar(int nuevoTiempoSeg = -1)
    {
        if (rutina != null) StopCoroutine(rutina);
        tiempoRestante = (nuevoTiempoSeg >= 0) ? nuevoTiempoSeg : tiempoInicialSegundos;
        ActualizarTexto();
        rutina = StartCoroutine(Contador());
    }

    public void SetTiempoInicial(int newValue)
    {
        tiempoInicialSegundos = newValue;
    }

    public int GetTiempo()
    {
        return tiempoRestante;
    }
}
