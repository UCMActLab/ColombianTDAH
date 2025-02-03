using System.Drawing;
using UnityEngine;

public class MouseInputMenu : MonoBehaviour
{
    [SerializeField, Tooltip("Capa con la que querremos que colisione (Edificios)")]
    LayerMask _layerMask;

    [SerializeField]
    float _raycastDistance = 10.0f;

    // Lanzamos un Raycast para ver si colisionamos con algo :p
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            TryHitBuilding();
        } 
        
        
    }

    private void TryHitBuilding()
    {
        Camera cam = Camera.main;
        Vector2 mousePos = new Vector2();

        mousePos.x = Input.mousePosition.x;
        mousePos.y = Input.mousePosition.y;
        
        Vector3 point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _raycastDistance));
        Vector3 dir = point - cam.transform.position;
        bool hasHit = Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, Mathf.Infinity, _layerMask);
        
        Debug.DrawRay(cam.transform.position, dir, UnityEngine.Color.yellow);
        
        if (hasHit)
        {
            hit.rigidbody.gameObject.GetComponent<OnMouseInputRecieved>().onInputRecieved.Invoke();
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Camera cam = Camera.main;
    //    Vector2 mousePos = new Vector2();

    //    mousePos.x = Input.mousePosition.x;
    //    mousePos.y = Input.mousePosition.y;

    //    Vector3 point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _raycastDistance));
    //    Gizmos.DrawSphere(point, 1.0f);
    //}
}
