using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI txt;
    [SerializeField] private Button clickCatcher; // Botón para cambiar texto

    private Queue<string> lines = new();

    public bool IsOpen => panel != null && panel.activeSelf;

    void Awake()
    {
        if (clickCatcher != null)
            clickCatcher.onClick.AddListener(Next);
        Hide();
    }

    public void ShowLines(IEnumerable<string> msgs)
    {
        lines.Clear();
        foreach (var m in msgs) lines.Enqueue(m);
        panel.SetActive(true);
        Next();
    }

    public void Next()
    {
        if (lines.Count == 0) { Hide(); return; }
        txt.text = lines.Dequeue();
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);   
    }
}
