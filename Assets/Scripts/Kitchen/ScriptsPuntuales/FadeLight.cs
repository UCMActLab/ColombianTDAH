using System.Collections;
using UnityEngine;

public class FadeLight : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;      // Tiempo de transición
    [SerializeField] private float targetIntensity = 1f;   // Intensidad máxima de la luz
    private Light lightSource;
    private Coroutine currentRoutine;

    void Awake()
    {
        lightSource = GetComponent<Light>();
        if (lightSource == null)
        {
            Debug.LogError("El objeto no tiene un componente Light.");
        }
    }

    public void FadeIn()
    {
        StartFade(targetIntensity);
    }

    public void FadeOut()
    {
        StartFade(0f);
    }

    private void StartFade(float newTarget)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeRoutine(newTarget));
    }

    private IEnumerator FadeRoutine(float newTarget)
    {
        float startIntensity = lightSource.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            lightSource.intensity = Mathf.Lerp(startIntensity, newTarget, elapsedTime / fadeDuration);
            yield return null;
        }

        lightSource.intensity = newTarget;
    }
}
