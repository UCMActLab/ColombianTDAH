using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class OnMouseInputRecieved : MonoBehaviour
{
    public UnityEvent onClickRecieved;
    public UnityEvent onHoverStart;
    public UnityEvent onHoverStop;

    private bool isHovering = false;

    public bool IsHovering() { return isHovering; }

    public void SetHover(bool value, float _hoveringTickRate) 
    { 
        isHovering = value; 
        StopAllCoroutines();
        StartCoroutine(QuitHover(_hoveringTickRate)); 
    }

    IEnumerator QuitHover(float timer)
    {
        yield return new WaitForSeconds(2*timer);
        isHovering = false;
        onHoverStop.Invoke();
    }
}
