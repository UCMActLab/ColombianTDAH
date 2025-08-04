using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum TurnoEstado
{
    Bloqueado,
    Activo,
    Completado
}

public enum Turno
{
    Manana,
    Tarde,
    Noche
}
public enum PuestosDeTrabajo
{
    Tabla_De_Picar,
    Licuadora,
    Olla,
    Sarten,
    Mezcladora,
    Olla_A_Presion,
    Horno
}

public enum Ingredientes
{
    Agua, Aguacate, Aji, Arepa, Arroz, Azucar, Cafe, Canela, Carne, CarneMolida, Cebolla, Cilantro, Coco, CremaDeLeche, Frijoles, Fresa, Frutas, Guayaba, Harina,
    Hojas, Huevo, Hueso, Leche, Limon, Maiz, Mariscos, Miel, MixVegetales, Panela, Papa, Pez, Platano, Pollo, Queso, Tomate, Viche, Yuca
}

public class LevelKitchenManager : MonoBehaviour
{
    private static LevelKitchenManager _instance = null;

    static public LevelKitchenManager Instance { get { return _instance; } }


    [Header("Configuración")]
    public RecetasDatabase recetasDatabase;
    public NivelacionData nivelacionData;

    private int jornadaActual = 1;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        CalcularRecetasPorJornada();
    }

    private void Update()
    {
        
    }

    public void CalcularRecetasPorJornada()
    {
        if (recetasDatabase == null || nivelacionData == null)
        {
            Debug.LogError("Faltan referencias a recetasDatabase o nivelacionData.");
            return;
        }

        foreach (JornadaData jornada in nivelacionData.jornadas)
        {
            Debug.Log($"Jornada '{jornada.nombreJornada}': {jornada.recetasAsignadas.Count} recetas asignadas.");
            foreach (RecetaData receta in jornada.recetasAsignadas)
            {
                Debug.Log(receta.nombre);
            }
        }

        Debug.Log("Recetas calculadas correctamente para todas las jornadas.");
    }

    public void SetJornada(int newValue)
    {
        jornadaActual = newValue;
    }
}