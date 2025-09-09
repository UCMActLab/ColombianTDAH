using UnityEngine;
using TMPro;

public class FloatAway : MonoBehaviour
{
    //script hecho inicialmente para los textos de send location de MC

    [SerializeField] float duration = 1f;

    //porque queria que se fueran a la izquierda y hacia arriba en principio
    [SerializeField] Vector2 rangoX = new Vector2(-100f, -50f); 
    [SerializeField] Vector2 rangoY = new Vector2(50f, 100f);

    private TextMeshProUGUI textMesh;
    private Vector3 initPos;
    private Vector3 endPos;
    private Color initColor;
    private float currentDuration;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh == null) Debug.Log("FlatAway.cs, no hay text");
    }

    private void Start()
    {
        initPos = transform.position;
        endPos = initPos + new Vector3(Random.Range(rangoX.x, rangoX.y), Random.Range(rangoY.x, rangoY.y), 0); 
        initColor = textMesh.color;
        currentDuration = 0f;
    }

    private void Update()
    {
        currentDuration += Time.deltaTime;

        // lerppppp
        float t = currentDuration / duration;
        transform.position = Vector3.Lerp(initPos, endPos, t);

        // va desapareciendo
        textMesh.color = new Color(initColor.r, initColor.g, initColor.b, 1 - t);//solo cambiamos el alpha en principio

        if (currentDuration >= duration)
            Destroy(gameObject);
    }
}