using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Transform entryPoint;   // Dónde “se monta” el plato
    [SerializeField] private Transform exitPoint;    // Dónde desaparece 

    [Header("Movement")]
    [SerializeField] private float speed = 0.05f;   
    [SerializeField] private bool destroyAtExit = true;

    // Pasajeros activos
    private readonly List<Passenger> passengers = new List<Passenger>();

    public static event System.Action<RecetaData> OnDeliveredGlobal;


    private struct Passenger
    {
        public Transform t;
        public Vector3 start;
        public Vector3 end;
        public float totalDist;
        public float traveled;
    }

    #region methods
    void Update()
    {
        if (passengers.Count == 0) return;

        float dt = Time.deltaTime;
        for (int i = passengers.Count - 1; i >= 0; i--)
        {
            var p = passengers[i];
            if (p.t == null) { passengers.RemoveAt(i); continue; }

            p.traveled += speed * dt;
            float t = Mathf.Clamp01(p.traveled / p.totalDist);
            p.t.position = Vector3.Lerp(p.start, p.end, t);

            Vector3 dir = (p.end - p.start).normalized;
            if (dir.sqrMagnitude > 0.0001f)
                p.t.rotation = Quaternion.LookRotation(dir, Vector3.up);

            if (t >= 1f)
            {
                if (destroyAtExit && p.t != null)
                {
                    var cd = p.t != null ? p.t.GetComponent<CompletedRecipe>() : null;
                    if (cd != null && cd.receta != null && !cd.receta.esIntermedia)
                    {
                        LevelKitchenManager.Instance?.RegisterDelivery(cd.receta);
                        OnDeliveredGlobal?.Invoke(cd.receta);
                    }
                    Destroy(p.t.gameObject);
                }
                passengers.RemoveAt(i);
            }
            else
            {
                passengers[i] = p;
            }
        }
    }

    public bool CanBoard(GameObject go)
    {
        // Solo aceptamos platos completados
        return go.GetComponent<CompletedRecipe>() != null;
    }

    /// <summary> Monta un objeto en la cinta (lo “agarra” y lo mueve hacia la salida). </summary>
    public void Board(GameObject go)
    {
        if (go == null || entryPoint == null || exitPoint == null) return;

        var rts = go.GetComponent<ReturnToSpawn>();
        if (rts)
        {
            rts.MarkDropHandledThisFrame(); 
            rts.BeginTransit(); // Bloquea retornos mientras viaja
        }

        // Desactiva su draggable mientras viaja
        var drag = go.GetComponent<Draggable>();
        if (drag) drag.enabled = false;

        var rb = go.GetComponent<Rigidbody>();
        if (rb) { rb.isKinematic = true; rb.useGravity = false; }

        // Colocamos en la entrada
        go.transform.position = entryPoint.position;

        var p = new Passenger
        {
            t = go.transform,
            start = entryPoint.position,
            end = exitPoint.position,
            totalDist = Vector3.Distance(entryPoint.position, exitPoint.position),
            traveled = 0f
        };

        passengers.Add(p);
    }

    public Transform GetEntryPoint() => entryPoint;
    #endregion
}
