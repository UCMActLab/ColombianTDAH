using System.Collections;
using UnityEngine;

public class FadeCanvas : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f; // Tiempo de transición
    private CanvasGroup canvasGroup;
    private Coroutine currentRoutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("El objeto no tiene un CanvasGroup. Añádelo para que funcione el fade.");
        }
    }

    public void FadeIn()
    {
        StartFade(1f);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public void FadeOut()
    {
        StartFade(0f);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void StartFade(float targetAlpha)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
