//using System.Collections;
//using System.Drawing;
//using UnityEngine;

//public class MouseInputDolphinLevel : MonoBehaviour
//{
//    [SerializeField, Tooltip("Capa con la que querremos que colisione (Drag)")]
//    LayerMask _layerMaskDrag;
//    [SerializeField, Tooltip("Capa con la que querremos que colisione (Click)")]
//    LayerMask _layerMaskClick;


//    [SerializeField]
//    float _raycastDistance = 10.0f;
//    [SerializeField]
//    float delayDragTime = 0.01f;
//    float clickTime;
//    enum MouseInteraction { HOVER, CLICK };

   
//    private void Start()
//    {
//        //
//        clickTime = 0;
//    }

//    // Lanzamos un Raycast para ver si colisionamos con algo :p
//    private void Update()
//    {
//        if (Input.GetMouseButtonUp(0))
//        {
//            TryHitDolphin(MouseInteraction.CLICK);
//        }
//        else if (Input.GetMouseButtonDown(0))
//        {

//        }
//    }

//    private bool TryHitDolphin(MouseInteraction interaction)
//    {
//        Camera cam = Camera.main;
//        Vector2 mousePos = new Vector2();

//        mousePos.x = Input.mousePosition.x;
//        mousePos.y = Input.mousePosition.y;

//        Vector3 point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _raycastDistance));
//        Vector3 dir = point - cam.transform.position;
//        bool hasHit = Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, Mathf.Infinity, _layerMask);

//        Debug.DrawRay(cam.transform.position, dir, UnityEngine.Color.yellow);

//        if (hasHit)
//        {
//            OnMouseInputRecieved _input = hit.rigidbody.gameObject.GetComponent<OnMouseInputRecieved>();

//            if (interaction == MouseInteraction.CLICK)
//            {
//                _input.onClickRecieved.Invoke();
//            }
//        }

//        return hasHit;
//    }

//}
