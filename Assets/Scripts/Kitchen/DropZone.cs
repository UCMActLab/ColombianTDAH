using UnityEngine;

public class DropZone : MonoBehaviour
{
    #region methods
    void OnTriggerStay(Collider other)
    {
        var draggable = other.GetComponent<Draggable>();
        if (draggable != null && !draggable.isDragging)
        {
            Debug.Log("Ingrediente soltado y procesándose");  
            draggable.gameObject.SetActive(false);
        }
    }
    #endregion
}