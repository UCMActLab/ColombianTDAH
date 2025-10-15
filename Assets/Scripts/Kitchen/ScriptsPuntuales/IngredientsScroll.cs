using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientsScroll : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Transform content;           // El Content del Scroll View
    [SerializeField] private GameObject ingredientPrefab; // El prefab de IngredientCell

    private Dictionary<Ingredientes, Sprite> database; // Tu ScriptableObject con la lista de ingredientes

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentScroll;

    private void Start()
    {
        database = LevelKitchenManager.Instance.GetIngredientesSprites();
        Populate();
    }

    private void Populate()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        foreach (var ingredient in database)
        {
            var cell = Instantiate(ingredientPrefab, content);
            var icon = cell.transform.Find("Icon").GetComponent<Image>();
            var name = cell.transform.Find("Nombre").GetComponent<TMP_Text>();

            icon.sprite = ingredient.Value;
            name.text = ingredient.Key.ToString().Replace("_", " "); ;
        }

        // Fuerza el cálculo de tamaños del layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentScroll);

        // Coloca el scroll arriba del todo
        scrollRect.normalizedPosition = new Vector2(0, 1);
    }
}
