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
        return recetas
            .Where(r => r.puestos.Contains(puesto) && r.ingredientes.Contains(entrada))
            .Where(r => r.esIntermedia || seleccion.Contains(r)) // Intermedias siempre; finales solo si están seleccionadas
            .FirstOrDefault();
    }
}
