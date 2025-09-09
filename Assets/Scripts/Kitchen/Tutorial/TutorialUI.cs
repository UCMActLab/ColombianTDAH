using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI txt;
    [SerializeField] private Button clickCatcher; // Botón para completar/pasar al siguiente texto

    [Header("TextSpeed[chars/s]")]
    [SerializeField] private float charsPerSecond = 20f; // Velocidad del texto

    private Queue<string> lines = new();

    private bool isTyping;
    private bool skipRequested;
    private int visibleTarget;
    private int typedChars;
    private float typeTimer;
    public bool IsOpen => panel != null && panel.activeSelf;

    void Awake()
    {
        if (clickCatcher != null)
            clickCatcher.onClick.AddListener(OnClick);
        Hide();
    }

    public void ShowLines(IEnumerable<string> msgs)
    {
        DraggableBlocker.Block();
        lines.Clear();
        foreach (var m in msgs) lines.Enqueue(m);
        if (panel) panel.SetActive(true);
        StartTypingNext();
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false); 
        DraggableBlocker.Unblock();

        isTyping = false;
        skipRequested = false;
        typedChars = 0;
        visibleTarget = 0;
        typeTimer = 0f;
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
        else Hide();
    }

    private void StartTypingNext()
    {
        if (txt == null) return;

        var next = lines.Dequeue();
        txt.text = next;
 
        txt.ForceMeshUpdate();
        visibleTarget = txt.textInfo.characterCount;

        typedChars = 0;
        typeTimer = 0f;
        skipRequested = false;
        isTyping = true;
    }

    void Update()
    {
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
