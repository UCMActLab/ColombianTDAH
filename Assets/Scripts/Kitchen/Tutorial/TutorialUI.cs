using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class TutorialUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI txt;
    [SerializeField] private Button clickCatcher; // Botón para completar/pasar al siguiente texto
    [SerializeField] private GameObject uiBlocker;

    [Header("TextSpeed[chars/s]")]
    [SerializeField] private float charsPerSecond = 20f; // Velocidad del texto

    private Queue<string> lines = new();

    private bool isTyping;
    private bool skipRequested;
    private int visibleTarget;
    private int typedChars;
    private float typeTimer;
    public bool IsOpen => panel != null && panel.activeSelf;

    public bool ClosedByUser { get; private set; }

    private bool hiddenByPause;

    void Awake()
    {
        if (clickCatcher != null)
            clickCatcher.onClick.AddListener(OnClick);
        Hide(false);
    }

    public void ShowLines(IEnumerable<string> msgs)
    {
        ClosedByUser = false;
        DraggableBlocker.Block(DraggableBlocker.Source.Tutorial);
        lines.Clear();
        foreach (var m in msgs) lines.Enqueue(m);
        if (txt != null)
        {
            txt.text = string.Empty;
            txt.maxVisibleCharacters = 0;
        }
        if (panel) panel.SetActive(true);
        if (uiBlocker) uiBlocker.SetActive(true);
        StartTypingNext();
    }

    public void Hide(bool byUser)
    {
        ClosedByUser = byUser;
        if (panel) panel.SetActive(false);
        if (txt != null)
        {
            txt.text = string.Empty;
            txt.maxVisibleCharacters = 0;
        }
        DraggableBlocker.Unblock(DraggableBlocker.Source.Tutorial);
        if (!LevelKitchenManager.Instance.IsPaused())
        {
            if (uiBlocker) uiBlocker.SetActive(false);
        }
        isTyping = false;
        skipRequested = false;
        typedChars = 0;
        visibleTarget = 0;
        typeTimer = 0f;
        hiddenByPause = false;   
    }

    private void OnClick()
    {
        if (!IsOpen) return;

        if (isTyping)
        {
            // Si está escribiendo, se completa la línea al instante
            skipRequested = true;
            return;
        }

        // Si ya se terminó de escribir la línea, avanza a la siguiente o cierra panel
        if (lines.Count > 0) StartTypingNext();
        else Hide(true);
    }

    private void StartTypingNext()
    {
        if (txt == null) return;

        var next = lines.Dequeue();
        txt.text = next;
        txt.maxVisibleCharacters = 0;
        txt.ForceMeshUpdate();
        visibleTarget = txt.textInfo.characterCount;

        typedChars = 0;
        typeTimer = 0f;
        skipRequested = false;
        isTyping = true;
    }

    void Update()
    {
        bool paused = LevelKitchenManager.Instance.IsPaused();
        if (paused && IsOpen)
        {
            panel.SetActive(false);
            hiddenByPause = true;
            if (uiBlocker) uiBlocker.SetActive(false);
            return;
        }
        else if (!paused && hiddenByPause)
        {
            panel.SetActive(true);
            hiddenByPause = false;
        }

        if (!IsOpen || !isTyping || txt == null) return;

        if (skipRequested)
        {
            txt.maxVisibleCharacters = visibleTarget;
            isTyping = false;
            skipRequested = false;
            return;
        }

        // Avance en función del tiempo
        typeTimer += Time.deltaTime * charsPerSecond;

        while (typeTimer >= 1f && typedChars < visibleTarget)
        {
            typedChars++;
            txt.maxVisibleCharacters = typedChars;
            typeTimer -= 1f;
        }

        // Fin de línea
        if (typedChars >= visibleTarget)
        {
            isTyping = false;
        }
    }
}
