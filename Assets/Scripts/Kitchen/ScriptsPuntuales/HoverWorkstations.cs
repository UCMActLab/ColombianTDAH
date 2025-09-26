using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))] // Necesita un collider para recibir OnMouse*
public class HoverWorkstations : MonoBehaviour
{
    #region parameters
    [Header("UI a mostrar mientras se mantiene pulsado")]
    [SerializeField] private GameObject uiRoot;

    [Tooltip("Segundos que hay que mantener pulsado antes de mostrar la UI (0 = inmediato)")]
    [Min(0f)][SerializeField] private float showDelay = 0f;

    [Tooltip("Si está activo, si el puntero sale del puesto mientras se mantiene, ocultará la UI hasta volver a presionar.")]
    [SerializeField] private bool hideIfPointerLeavesWhileHeld = false;

    [Header("Config animación")]
    [SerializeField] private float duration = 0.15f; // duración en segundos
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.95f, 0.95f, 0.95f);
    [SerializeField] private Vector3 shownScale = Vector3.one;

    [SerializeField] private FadeCanvas fadeCanvas;
    #endregion

    #region state
    private Coroutine currentCoroutine;

    private bool isPressed;
    private bool uiShownForThisPress;
    private float pressTimer;
    #endregion

    #region lifecycle
    private void Awake()
    {
        if (uiRoot != null && !uiRoot.activeSelf) // Asegura que el objeto empieza oculto si está desactivado
        {
            uiRoot.transform.localScale = hiddenScale;
        }
    }

    private void OnDisable()
    {
        isPressed = false;
        uiShownForThisPress = false;
        HideUI();
    }
    #endregion

    #region mouse handlers
    private void OnMouseDown()
    {
        isPressed = true;
        uiShownForThisPress = false;
        pressTimer = 0f;

        if (showDelay <= 0f)
        {
            ShowUI();
            uiShownForThisPress = true;
        }
    }

    private void OnMouseUp()
    {
        isPressed = false;
        uiShownForThisPress = false;
        HideUI();
    }

    private void OnMouseExit()
    {
        if (hideIfPointerLeavesWhileHeld && isPressed)
        {
            HideUI();
        }
    }
    #endregion

    #region update
    private void Update()
    {
        if (isPressed && !uiShownForThisPress && showDelay > 0f)
        {
            pressTimer += Time.deltaTime;
            if (pressTimer >= showDelay)
            {
                ShowUI();
                uiShownForThisPress = true;
            }
        }
    }
    #endregion

    #region helpers
    public void ShowUI()
    {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;
        PlayAnimation(shownScale);
        fadeCanvas.FadeIn();
    }

    public void HideUI()
    {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;
        PlayAnimation(hiddenScale);
        fadeCanvas.FadeOut();
    }

    private void PlayAnimation(Vector3 targetScale)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ScaleCoroutine(targetScale));
    }

    private IEnumerator ScaleCoroutine(Vector3 target)
    {
        Vector3 start = uiRoot.transform.localScale;
        float tiempo = 0f;

        while (tiempo < duration)
        {
            float t = tiempo / duration;
            uiRoot.transform.localScale = Vector3.Lerp(start, target, t);
            tiempo += Time.unscaledDeltaTime; // usar unscaled por si el juego pausa
            yield return null;
        }

        uiRoot.transform.localScale = target;
        currentCoroutine = null;
    }
    #endregion
}
