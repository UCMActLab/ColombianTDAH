using System.Collections;
using System.Drawing;
using UnityEngine;

public class MouseInputMenu : MonoBehaviour
{
    [SerializeField, Tooltip("Capa con la que querremos que colisione (Edificios)")]
    LayerMask _layerMask;

    [SerializeField]
    float _hoveringTickRate = 0.05f;

    [SerializeField]
    float _raycastDistance = 10.0f;

    enum MouseInteraction { HOVER, CLICK };

    IEnumerator _HoveringCorroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(_hoveringTickRate);
            TryHitBuilding(MouseInteraction.HOVER);
        }
    }
    private void Start()
    {
        StartCoroutine(_HoveringCorroutine());
    }

    // Lanzamos un Raycast para ver si colisionamos con algo :p
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            TryHitBuilding(MouseInteraction.CLICK);
        } 
    }

    private bool TryHitBuilding(MouseInteraction interaction)
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
            OnMouseInputRecieved _input = hit.rigidbody.gameObject.GetComponent<OnMouseInputRecieved>();
            
            if (interaction == MouseInteraction.HOVER)
            {
                _input.onHoverStart.Invoke();
                _input.SetHover(true, _hoveringTickRate);
            }
            else if (interaction == MouseInteraction.CLICK) 
            {
                _input.onClickRecieved.Invoke();
            }
        }

        return hasHit;
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
