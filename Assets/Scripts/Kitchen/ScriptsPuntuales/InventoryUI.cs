using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Referencias UI (usa Layout Groups)")]
    [SerializeField] private RectTransform rowTop;    // HorizontalLayoutGroup (Middle Center)
    [SerializeField] private RectTransform rowBottom; // HorizontalLayoutGroup (Middle Center)
    [SerializeField] private TMPro.TextMeshProUGUI text;

    [Header("Opciones")]
    [Tooltip("Tamaño máximo por icono (se usa como límite superior).")]
    [SerializeField] private Vector2 iconSize = new(72, 72);
    [SerializeField] private int maxPerRow = 5;

    [Tooltip("Espaciado horizontal si no detecta HorizontalLayoutGroup.")]
    [SerializeField] private float fallbackHorizontalSpacing = 10f;
    [Tooltip("Espaciado vertical entre filas si no detecta VerticalLayoutGroup en el padre.")]
    [SerializeField] private float fallbackVerticalSpacing = 10f;

    // Pool interno por fila
    private readonly List<Image> poolTop = new();
    private readonly List<Image> poolBottom = new();

    /// Llama a este método cuando se actualice el inventario del puesto.
    public void Refresh(Dictionary<Ingredientes, int> counts)
    {
        if (!rowTop || !rowBottom || counts == null)
        {
            Debug.LogWarning($"[{nameof(InventoryUI)}] Faltan referencias.");
            return;
        }

        // Fuerza actualización de layout para tener tamaños reales de rect
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        // 1) Construir lista de sprites (uno por unidad), cap a 10
        List<Sprite> sprites = new();
        foreach (var kv in counts)
        {
            if (!LevelKitchenManager.Instance.GetIngredientesSprites().TryGetValue(kv.Key, out var sp) || sp == null) continue;
            int c = Mathf.Max(0, kv.Value);
            for (int i = 0; i < c; i++) sprites.Add(sp);
        }
        int total = Mathf.Min(sprites.Count, maxPerRow * 2); // máximo 10
        if (total < sprites.Count) sprites.RemoveRange(total, sprites.Count - total);
        if (total == 0)
        {
            text.gameObject.SetActive(true);
        }
        else
        {
            text.gameObject.SetActive(false);
        }

        // 2) Reparto en filas
        int topCount, bottomCount;
        if (total <= maxPerRow)
        {
            topCount = total;
            bottomCount = 0;
        }
        else
        {
            topCount = Mathf.Min(maxPerRow, Mathf.CeilToInt(total / 2f));
            bottomCount = total - topCount;
        }

        // 3) Spacings desde LayoutGroups (o fallback)
        float hSpacingTop = GetHorizontalSpacing(rowTop);
        float hSpacingBottom = GetHorizontalSpacing(rowBottom);
        float vSpacing = GetVerticalSpacing((RectTransform)transform);

        // 4) Altura disponible para cada fila (si hay 2 filas, se reparte)
        var container = (RectTransform)transform;
        float containerHeight = container.rect.height;
        float perRowAvailableHeight = bottomCount > 0
            ? Mathf.Max(0f, (containerHeight - vSpacing) * 0.5f)
            : containerHeight;

        // 5) Asegurar pools
        EnsurePool(poolTop, rowTop, topCount);
        EnsurePool(poolBottom, rowBottom, bottomCount);

        // 6) Calcular tamaño óptimo por fila (máximo posible sin salirse, limitado por iconSize)
        float sizeTop = ComputeFittingSize(rowTop.rect.width, perRowAvailableHeight, topCount, hSpacingTop, iconSize);
        float sizeBottom = bottomCount > 0
            ? ComputeFittingSize(rowBottom.rect.width, perRowAvailableHeight, bottomCount, hSpacingBottom, iconSize)
            : 0f;

        // 7) Rellenar fila superior
        int idx = 0;
        for (int i = 0; i < poolTop.Count; i++)
        {
            bool active = i < topCount;
            var img = poolTop[i];
            img.gameObject.SetActive(active);
            if (active)
            {
                img.sprite = sprites[idx++];
                ((RectTransform)img.transform).sizeDelta = new Vector2(sizeTop, sizeTop);
            }
        }

        // 8) Rellenar fila inferior
        for (int i = 0; i < poolBottom.Count; i++)
        {
            bool active = i < bottomCount;
            var img = poolBottom[i];
            img.gameObject.SetActive(active);
            if (active)
            {
                img.sprite = sprites[idx++];
                ((RectTransform)img.transform).sizeDelta = new Vector2(sizeBottom, sizeBottom);
            }
        }

        // 9) Mostrar/ocultar fila inferior y forzar layout
        rowBottom.gameObject.SetActive(bottomCount > 0);
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }

    private float GetHorizontalSpacing(RectTransform row)
    {
        var h = row.GetComponent<HorizontalLayoutGroup>();
        return h ? h.spacing : fallbackHorizontalSpacing;
    }

    private float GetVerticalSpacing(RectTransform parent)
    {
        var v = parent.GetComponent<VerticalLayoutGroup>();
        return v ? v.spacing : fallbackVerticalSpacing;
    }

    /// Calcula el tamaño cuadrado máximo por icono que cabe en la fila,
    /// respetando: ancho disponible, alto disponible, espaciado y tamaño máximo (iconSize).
    private float ComputeFittingSize(float availableWidth, float availableHeight, int countInRow, float hSpacing, Vector2 maxSize)
    {
        if (countInRow <= 0) return 0f;

        // Ancho máximo por icono que cabe con spacings
        float widthForIcons = Mathf.Max(0f, availableWidth - hSpacing * (countInRow - 1));
        float maxByWidth = countInRow > 0 ? widthForIcons / countInRow : 0f;

        // Alto máximo por icono (una fila ocupa sólo su alto)
        float maxByHeight = availableHeight;

        // Respetar iconSize como límite superior y mantener cuadrado
        float cap = Mathf.Min(maxSize.x, maxSize.y);
        float side = Mathf.Min(maxByWidth, maxByHeight, cap);

        // Evitar valores negativos o NaN
        if (float.IsNaN(side) || side < 0f) side = 0f;

        // Pequeño margen para evitar “salirse” por redondeos de layout
        side = Mathf.Max(0f, Mathf.Floor(side));

        return side;
    }

    private void EnsurePool(List<Image> pool, RectTransform parent, int neededActive)
    {
        while (pool.Count < neededActive)
        {
            var go = new GameObject("IngredientIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            pool.Add(img);
            go.SetActive(false);
        }
    }
}
