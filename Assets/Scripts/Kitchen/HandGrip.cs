using UnityEngine;

public class HandGrip : MonoBehaviour
{
    private Draggable draggable;

    [Header("Animación Mano")]
    [SerializeField] private string gripParam = "Grip";
    [SerializeField] private float gripSpeed = 8f;

    [SerializeField] private GameObject handModel;

    void Start()
    {
        draggable = GetComponent<Draggable>();
        if (draggable == null)
            Debug.LogError("[HandGripBridge] No se encontró Draggable en el mismo objeto.");
    }

    void Update()
    {
        if (draggable == null) return;

        //if (draggable.isDragging)
        //{
        //    handModel.SetActive(true); // mostrar mano
        //    AnimatorManager.Instance.LerpFloat(ObjetosAnim.Mano, gripParam, 1f, gripSpeed);
        //}
        //else
        //{
        //    AnimatorManager.Instance.LerpFloat(ObjetosAnim.Mano, gripParam, 0f, gripSpeed);

        //    handModel.SetActive(false); // ocultar mano
        //}
    }
}