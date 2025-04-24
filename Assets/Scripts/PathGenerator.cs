using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class PathGenerator : MonoBehaviour
{

    [SerializeField]
    public float3 limitMin;
    [SerializeField]
    public float3 limitMax;
    [SerializeField]
    public int pathPointMin;
    [SerializeField]
    public int pathPointMax;

    private void Start()
    {
        Vector3 riverSize = DolphinLevelManager.Instance.GetRiverSize();
        //limit min = centro -+ size/2
    }
    public SplineContainer GeneratePath(float3 initPos, float3 lastPos) {
        int length = UnityEngine.Random.Range(pathPointMin, pathPointMax);            
        float3[] pathPoints = new float3[length];

        pathPoints[0] = initPos;
        for (int i = 1; i < length-1; i++) {
            pathPoints[i] = new float3(UnityEngine.Random.Range(limitMin.x, limitMax.x), UnityEngine.Random.Range(limitMin.y, limitMax.y), UnityEngine.Random.Range(limitMin.z, limitMax.z));
        }
        if (!lastPos.Equals(new (float3.zero)))
            pathPoints[length-1] = lastPos;
        else
            pathPoints[length-1] = new float3(UnityEngine.Random.Range(limitMin.x, limitMax.x), UnityEngine.Random.Range(limitMin.y, limitMax.y), UnityEngine.Random.Range(limitMin.z, limitMax.z));

        return CreatePath(pathPoints);
    }

    SplineContainer CreatePath( float3[] pathPoints) {
        GameObject BuceoPath = new GameObject("BuceoPath");
        
        var container = BuceoPath.AddComponent<SplineContainer>();
        var spline = container.AddSpline();
        var knots = new BezierKnot[pathPoints.Length];

        knots[0] = new BezierKnot(
                pathPoints[0],
                -10 * Vector3.right,
                10 * Vector3.right);

        for (int i = 1; i < pathPoints.Length; i++) {
            knots[i] = new BezierKnot(
                pathPoints[i], 
                -30 * Vector3.right, 
                30 * Vector3.right);
        }
        
        spline.Knots = knots;
        
        return container;
    }
}