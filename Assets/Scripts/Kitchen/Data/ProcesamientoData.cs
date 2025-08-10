using UnityEngine;

[CreateAssetMenu(fileName = "ProcesamientoData", menuName = "Scriptable Objects/ProcesamientoData")]
public class ProcesamientoData : ScriptableObject
{
    public Ingredientes ingredient;
    public PuestosDeTrabajo workstation;
    public GameObject processedIngredient;
    public float processTime;
}
