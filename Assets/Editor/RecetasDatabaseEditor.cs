using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(RecetasDatabase))]
public class RecetasDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generar JSON de Recetas"))
        {
            RecetasDatabase database = (RecetasDatabase)target;

            string assetPath = AssetDatabase.GetAssetPath(database);
            string folderPath = Path.GetDirectoryName(assetPath);
            string outputPath = Path.Combine(folderPath, "recetas.json");

            // Convertir recetas a clases exportables
            List<RecetaExport> recetasExport = new();

            foreach (var receta in database.recetas)
            {
                recetasExport.Add(new RecetaExport
                {
                    nombre = receta.nombre,
                    categoria = receta.categoria,
                    ingredientes = receta.ingredientes,
                    puestos = receta.puestos
                });
            }

            // Envolver y serializar
            RecetasWrapper wrapper = new RecetasWrapper { recetas = recetasExport };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(outputPath, json);
            AssetDatabase.Refresh();

            Debug.Log($"✅ JSON generado en:\n{outputPath}");
        }
    }

    [System.Serializable]
    public class RecetaExport
    {
        public string nombre;
        public string categoria;
        public List<string> ingredientes;
        public List<string> puestos;
    }

    [System.Serializable]
    public class RecetasWrapper
    {
        public List<RecetaExport> recetas;
    }
}
