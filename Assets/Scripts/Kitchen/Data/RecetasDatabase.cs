using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RecetasDatabase", menuName = "Scriptable Objects/RecetasDatabase")]
public class RecetasDatabase : ScriptableObject
{
    public List<RecetaData> recetas = new();

    public RecetaData GetRecetaValida(
        Ingredientes entrada,
        PuestosDeTrabajo puesto,
        IEnumerable<RecetaData> seleccion
    )
    {
        if (recetas == null) return null;

        // La selección se usa SOLO como filtro adicional
        var seleccionSet = new HashSet<RecetaData>(seleccion.Where(s => s != null));

        // Todas las recetas
        var candidatas = recetas.Where(r =>
            r != null &&
            r.puestos != null && r.puestos.Contains(puesto) &&
            r.ingredientes != null && r.ingredientes.Contains(entrada))
            .ToList(); ;
        foreach (var receta in candidatas)
        {
            Debug.Log($"[DEBUG] Receta candidata: {receta.nombre}, esIntermedia: {receta.esIntermedia}");
        }

        return candidatas.FirstOrDefault(r => r.esIntermedia || seleccionSet.Contains(r));
    }
}
