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

    private void Awake()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        pathGen = GameObject.Find("PathGenerator").GetComponent<PathGenerator>();
    }
    void Start()
	{
        //if(pathGen != null )
        //{
        //    this.enabled = false;
        //}
        lastSpline = false;
        this.enabled = false;
    }

    void Update() {
        if (splineAnimate.Container != null){
            if (splineAnimate != null && splineAnimate.ElapsedTime >= splineAnimate.Duration && !lastSpline) {
                SetPath(float3.zero);
            }
            else if (lastSpline)
            {
                this.enabled = false;
            }
        }
    }

    private void OnEnable()
    {
        lastSpline = false;
    }
    void OnDestroy() {
        if (splineAnimate.Container != null) {
            Destroy(splineAnimate.Container.gameObject);
        }
    }

    public void SetPath(float3 destination)
    {
        if (!destination.Equals(new(float3.zero)))
        {
            lastSpline = true;
        }
        pathGen = GameObject.Find("PathGenerator").GetComponent<PathGenerator>();
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
