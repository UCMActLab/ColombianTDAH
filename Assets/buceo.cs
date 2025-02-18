using System;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.XR;
using Unity.Mathematics;

public class Buceo : MonoBehaviour
{
    SplineAnimate splineAnimate;
    [SerializeField]
    PathGenerator pathGen;
    bool lastSpline;

    void Start()
	{
        pathGen= GameObject.Find("PathGenerator").GetComponent<PathGenerator>();
        //if(pathGen != null )
        //{
        //    this.enabled = false;
        //}
        splineAnimate = GetComponent<SplineAnimate>();
        lastSpline = false;
        this.enabled = false;
    }

    void Update() {
        if (splineAnimate.Container != null){
            if (splineAnimate != null && splineAnimate.ElapsedTime >= splineAnimate.Duration && !lastSpline) {
                SetPath(float3.zero);
            }
            //else
                //this.enabled = false;
        }
    }

    void OnDestroy() {
        if (splineAnimate.Container != null) {
            Destroy(splineAnimate.Container.gameObject);
        }
    }

    public void SetPath( float3 destination)
    {
        if (!destination.Equals(new(float3.zero)))
        {
            Debug.Log("lastSpline");
            //lastSpline = true;
        }

        SplineContainer sp = pathGen.GeneratePath(transform.position, destination);
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
