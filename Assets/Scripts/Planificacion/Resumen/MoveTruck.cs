using System.Collections.Generic;
using UnityEngine;

public class MoveTruck : MonoBehaviour
{
    [SerializeField] private GameObject truck;
    [SerializeField] private GameObject pathPointsParent;
    [SerializeField] private float speed = 30f;
    [SerializeField] private bool loop = false;  // repetir el recorrido
    [SerializeField] private float oscillationAngle = 10f; 
    [SerializeField] private float oscillationSpeed = 10f;  

    private List<Transform> pointsList = new List<Transform>();
    private int currentIndexPoint = 0;
    private bool isMoving = true;

    private bool subiendo;
    private float currentAngle = 0f;
    void Start()
    {

        foreach (Transform child in pathPointsParent.transform)
        {
            if (child.gameObject.activeSelf) //por si no esta ctivado
            {
                pointsList.Add(child);
            }
        }

        if (pointsList.Count > 0)
        {
            truck.transform.position = pointsList[0].position;
        }
    }

    void Update()
    {
        Vector3 temp = new Vector3(7.0f, 0, 0);
       // truck.transform.position += temp;
        if (isMoving)
        {
            Moving();
            Oscila();
        }
    }

    private void Moving()
    {
        if (pointsList.Count < 2) return;
      
        Vector3 targetPos = pointsList[currentIndexPoint + 1].position;

        truck.transform.position = Vector3.MoveTowards(
            truck.transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(truck.transform.position, targetPos) < 1f)
        {
            currentIndexPoint++;

            if (currentIndexPoint >= pointsList.Count - 1)
            {
                if (loop)
                {
                    currentIndexPoint = 0;
                    truck.transform.position = pointsList[0].position;
                }
                else
                {
                    isMoving = false; 
                }
            }
        }
    }


    private void Oscila()
    {
        if (subiendo)
        {
            currentAngle += oscillationSpeed * Time.deltaTime;
            if (currentAngle >= oscillationAngle) subiendo = false;
        }
        else
        {
            currentAngle -= oscillationSpeed * Time.deltaTime;
            if (currentAngle <= -oscillationAngle) subiendo = true;
        }

        truck.transform.rotation = Quaternion.Euler(0, 0, currentAngle);

    }

}
