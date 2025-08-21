using UnityEngine;

public class CintaScroller : MonoBehaviour
{
    [Tooltip("Velocidad del desplazamiento en unidades por segundo")]
    public float scrollSpeed = 0.1f;

    [Tooltip("Dirección del desplazamiento de la textura (ej: (0,1) vertical, (1,0) horizontal)")]
    public Vector2 scrollDirection = Vector2.up;

    private Renderer rend;
    private Vector2 offset;

    void Start()
    {
        rend = GetComponent<Renderer>();
        offset = rend.material.mainTextureOffset;
    }

    void Update()
    {
        offset += scrollDirection.normalized * scrollSpeed * Time.deltaTime;
        rend.material.mainTextureOffset = offset;
    }
}
