using System;
using UnityEngine;
using UnityEngine.Splines;

public class buceo : MonoBehaviour
{
    SplineAnimate splineAnimate;
    [SerializeField]
    PathGenerator pathGen;

    void Start( )
	{
		splineAnimate = GetComponent<SplineAnimate>();
        //por ahora empiezan buceando
        SetPath(pathGen.GeneratePath(transform.position));
    }

    void Update() {
        if (splineAnimate.Container != null){
            if (splineAnimate != null && splineAnimate.ElapsedTime >= splineAnimate.Duration) {
                //AQUÍ PREGUNTARÍA AL MANAGER POR SIGUIENTE PASO
                //segun respuesta blabla pero ahora sigue buceando
                SetPath(pathGen.GeneratePath(transform.position));
            }
        }
    }

    void OnDestroy() {
        if (splineAnimate.Container != null) {
            Destroy(splineAnimate.Container.gameObject);
        }
    }

    public void SetPath(SplineContainer sp){
        SplineContainer aux = splineAnimate.Container;
        splineAnimate.Container = sp;
        if (aux != null)
        {
            Destroy(aux.gameObject);
        }
        splineAnimate.ElapsedTime = 0;
        splineAnimate.Play();
    }
}
