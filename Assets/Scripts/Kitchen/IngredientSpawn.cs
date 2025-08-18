using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Draggable))]
public class IngredientSpawn : MonoBehaviour
{
    [Header("Asignación")]
    [SerializeField] private IngredientSpawnPoint spawn; // Lo asigna el spawn al instanciar
    [SerializeField] private Transform spawnTransform;     

    [Header("Movimiento de retorno")]
    [SerializeField] private float returnDuration = 0.35f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Draggable drag;
    private bool isReturning;

    #region methods
    void Awake()
    {
        drag = GetComponent<Draggable>();
    }

    public void Init(IngredientSpawnPoint sp, Transform t)
    {
        spawn = sp;
        spawnTransform = t != null ? t : (sp ? sp.transform : null);
    }

    public void ReturnToSpawn(bool snap = false)
    {
        if (spawnTransform == null || isReturning) return;
        StopAllCoroutines();
        StartCoroutine(ReturnRoutine(snap));
    }

    IEnumerator ReturnRoutine(bool snap)
    {
        isReturning = true;
        if (drag) drag.enabled = false;

        Vector3 p0 = transform.position;
        Quaternion r0 = transform.rotation;
        Vector3 p1 = spawnTransform.position;
        Quaternion r1 = spawnTransform.rotation;

        if (snap || returnDuration <= 0.01f)
        {
            transform.SetPositionAndRotation(p1, r1);
        }
        else
        {
            float t = 0f;
            while (t < 1f)
            {
                float k = ease.Evaluate(t);
                transform.position = Vector3.LerpUnclamped(p0, p1, k);
                transform.rotation = Quaternion.SlerpUnclamped(r0, r1, k);
                t += Time.deltaTime / returnDuration;
                yield return null;
            }
            transform.SetPositionAndRotation(p1, r1);
        }

        if (drag) drag.enabled = true;
        isReturning = false;
    }

    /// <summary>
    /// La estación “consume” el ingrediente: lo ocultamos y pedimos al spawn re-aparecer tras delay.
    /// </summary>
    public void ConsumeAndScheduleRespawn()
    {
        if (spawn == null) { gameObject.SetActive(false); return; } // Fallback
        // Deshabilitamos y delegamos la reaparición en el spawn
        gameObject.SetActive(false);
        spawn.ScheduleReactivate(this);
    }
    #endregion
}
