using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RecetasDatabase", menuName = "Scriptable Objects/RecetasDatabase")]
public class RecetasDatabase : ScriptableObject
{
    public List<RecetaData> recetas = new();

    public RecetaData GetRecetaValida(
        Ingredientes entrada,
        PuestosDeTrabajo puesto
        // Probablemente hay que añadir algo para cuando se eligen las recetas en la UI
    )
    {
        var candidatas = recetas
            .Where(r => r.puestos.Contains(puesto) && r.ingredientes.Contains(entrada))
            .Where(r => r.esIntermedia); // Probablemente hay que añadir algo para cuando se eligen las recetas en la UI

        return candidatas.FirstOrDefault();
    }

    public IEnumerable<RecetaData> GetRecetasMostrables()
    {
        return recetas.Where(r => !r.esIntermedia);
    }
}
