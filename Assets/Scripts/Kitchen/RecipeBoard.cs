using UnityEngine;
using System.Collections.Generic;
using System;

public class RecipeBoard : MonoBehaviour
{
    [Header("Refs")]
    public GameObject recipePrefab;
    public Transform boardArea;
    [SerializeField] private Texture tickTexture; 

    [Header("Área del tablón (mundo)")]
    public Vector2 boardSize = new Vector2(5f, 3f);
    public int maxPerRow = 5;

    [Header("Layout / padding (unidades mundo)")]
    public float paddingLeft = 0.15f;
    public float paddingRight = 0.15f;
    public float paddingTop = 0.20f;
    public float paddingBottom = 0.20f;

    [Header("Ajustes de colocación")]
    public float surfaceOffset = 0.01f; // Separación del plano para evitar z-fighting
    public float extraYaw = 0f;         
    public float yNudge = 0.06f;

    [Header("Escalado del prefab")]
    public Vector2 prefabSizeXY = new Vector2(0.5f, 0.9f); // tamaño original del prefab en local
    public float pivotToFrontZ = 0.02f;

    private readonly List<GameObject> spawnedRecipes = new();

    public void ShowRecipes(List<RecetaData> recipes)
    {
        ClearBoard();
        if (!boardArea || !recipePrefab || recipes == null || recipes.Count == 0) return;

        // Tamaño en espacio local corrigiendo escala del tablón
        Vector2 localSize = new Vector2(
            boardSize.x / boardArea.lossyScale.x,
            boardSize.y / boardArea.lossyScale.y
        );

        // Área útil con padding
        float innerW = Mathf.Max(0.01f, localSize.x - (paddingLeft + paddingRight));
        float innerH = Mathf.Max(0.01f, localSize.y - (paddingTop + paddingBottom));

        int total = recipes.Count;
        int rows = (total > maxPerRow) ? 2 : 1;
        int perRow = Mathf.CeilToInt((float)total / rows);

        // Tamaño de celda
        float cellW = innerW / perRow;
        float cellH = innerH / rows;

        // Origen (esquina sup izq en local)
        float leftX = -localSize.x * 0.5f + paddingLeft;
        float topY = localSize.y * 0.5f - paddingTop;

        // Escalado uniforme para que el prefab llene la celda
        float scaleFactor = Mathf.Min(
            cellW / Mathf.Max(0.001f, prefabSizeXY.x),
            cellH / Mathf.Max(0.001f, prefabSizeXY.y)
        );

        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < perRow; col++)
            {
                if (index >= total) break;

                // Centro de la celda
                float x = leftX + (col + 0.5f) * cellW;
                float y = topY - (row + 0.5f) * cellH;

                Vector3 localPos = new Vector3(x, y + yNudge, surfaceOffset);

                // Slot vacío como soporte
                var slot = new GameObject($"RecipeSlot_{index}");
                slot.transform.SetParent(boardArea, false);
                slot.transform.localPosition = localPos;
                slot.transform.localRotation = Quaternion.identity;

                // Instanciamos el prefab dentro
                var recipeGO = Instantiate(recipePrefab, slot.transform, false);
                recipeGO.transform.localRotation = Quaternion.Euler(0f, extraYaw, 0f);
                recipeGO.transform.localScale = Vector3.one * scaleFactor;

                // Empujamos para que la cara frontal quede enrasada
                recipeGO.transform.localPosition = new Vector3(0f, 0f, -pivotToFrontZ * scaleFactor);

                // Texto si lo tiene
                var text = recipeGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text) text.text = recipes[index].nombre;

                Transform myChild = FindChildByName(recipeGO.transform, "Object_2");
                if (myChild != null)
                {
                    myChild.GetComponent<Renderer>().material.SetTexture("_BaseMap", LevelKitchenManager.Instance.GetRecetasSprites()[recipes[index].nombre].spriteTablon);
                }

                spawnedRecipes.Add(slot);
                index++;
            }
        }
    }

    public void ClearBoard()
    {
        foreach (var r in spawnedRecipes) if (r) Destroy(r);
        spawnedRecipes.Clear();
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindChildByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    public void ChangeTexture(string n)
    {
        bool encontrado = false;
        int i = 0;
        while (!encontrado && i < spawnedRecipes.Count) { 
            if (spawnedRecipes[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text == n && FindChildByName(spawnedRecipes[i].transform, "Object_2").GetComponent<Renderer>().material.GetTexture("_BaseMap").name != tickTexture.name)
            {
                encontrado = true;
                FindChildByName(spawnedRecipes[i].transform, "Object_2").GetComponent<Renderer>().material.SetTexture("_BaseMap", tickTexture);

                Debug.Log("Receta encontrada para cambiar textura en el tablon");
            }
            i++;
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (!boardArea) return;
        Gizmos.color = Color.white;
        Gizmos.matrix = Matrix4x4.TRS(
            boardArea.position, boardArea.rotation,
            Vector3.Scale(Vector3.one, boardArea.lossyScale)
        );
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(boardSize.x, boardSize.y, 0.001f));
    }
}
