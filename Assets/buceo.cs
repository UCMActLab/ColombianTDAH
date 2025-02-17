using System;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.XR;

public class Buceo : MonoBehaviour
{
    SplineAnimate splineAnimate;
    [SerializeField]
    PathGenerator pathGen;
    void Start()
	{
        pathGen= GameObject.Find("PathGenerator").GetComponent<PathGenerator>();
        //if(pathGen != null )
        //{
        //    this.enabled = false;
        //}
        splineAnimate = GetComponent<SplineAnimate>();
        this.enabled = false;
    }

    void Update() {
        if (splineAnimate.Container != null){
            if (splineAnimate != null && splineAnimate.ElapsedTime >= splineAnimate.Duration) {
                SetPath();
            }
        }
    }

    void OnDestroy() {
        if (splineAnimate.Container != null) {
            Destroy(splineAnimate.Container.gameObject);
        }
    }

    public void SetPath(){
        SplineContainer sp = pathGen.GeneratePath(transform.position);
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
