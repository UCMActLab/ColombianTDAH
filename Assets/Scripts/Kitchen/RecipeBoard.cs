using UnityEngine;
using System.Collections.Generic;

public class RecipeBoard : MonoBehaviour
{
    public GameObject recipePrefab;          // Prefab receta (frente mirando +Z)
    public Transform boardArea;              // Empty centrado y rotado como el tablón
    public Vector2 boardSize = new Vector2(5f, 3f); // Tamaño del área en METROS (espacio mundo)
    public int maxPerRow = 5;
    [Header("Ajustes de colocación")]
    public float surfaceOffset = 0.01f;      // Sepárate del plano para evitar z-fighting
    public float extraYaw = 0f;              // Pon 180 si el prefab sale “de espaldas”

    private readonly List<GameObject> spawnedRecipes = new();

    public void ShowRecipes(List<RecetaData> recipes)
    {
        ClearBoard();
        if (boardArea == null || recipePrefab == null || recipes == null || recipes.Count == 0)
            return;

        // 1) Convertimos tamaño de mundo -> tamaño local (corrige escalas del tablón)
        Vector2 localSize = new Vector2(
            boardSize.x / boardArea.lossyScale.x,
            boardSize.y / boardArea.lossyScale.y
        );

        int total = recipes.Count;
        int rows = (total > maxPerRow) ? 2 : 1;
        int perRow = Mathf.CeilToInt((float)total / rows);

        float xSpacing = localSize.x / (perRow + 1);
        float ySpacing = localSize.y / (rows + 1);

        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < perRow; col++)
            {
                if (index >= total) break;

                // 2) Posición en espacio LOCAL del tablón (origen al centro)
                Vector3 localPos = new Vector3(
                    -localSize.x * 0.5f + (col + 1) * xSpacing,
                    +localSize.y * 0.5f - (row + 1) * ySpacing,
                    surfaceOffset              // nos separamos un poco del plano
                );

                // 3) Instanciar como hijo del tablón y alinear con su superficie
                var recipeGO = Instantiate(recipePrefab, boardArea, false);
                recipeGO.transform.localPosition = localPos;

                recipeGO.transform.localRotation = Quaternion.Euler(0f, extraYaw, 0f);

                // 4) Rellenar texto si existe
                var text = recipeGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null) text.text = recipes[index].nombre;

                spawnedRecipes.Add(recipeGO);
                index++;
            }
        }
    }

    public void ClearBoard()
    {
        foreach (var r in spawnedRecipes) if (r) Destroy(r);
        spawnedRecipes.Clear();
    }

    // Gizmo útil para ver el área en editor
    private void OnDrawGizmosSelected()
    {
        if (!boardArea) return;
        Gizmos.color = Color.white;
        Matrix4x4 m = Matrix4x4.TRS(boardArea.position, boardArea.rotation, Vector3.Scale(Vector3.one, boardArea.lossyScale));
        Gizmos.matrix = m;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(boardSize.x, boardSize.y, 0.001f));
    }
}
