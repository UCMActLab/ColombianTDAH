using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveTruck : MonoBehaviour
{
    [SerializeField] private GameObject truck;
    [SerializeField] private GameObject pathPointsParent;
    [SerializeField] private GameObject mapBackgrounds;
    [SerializeField] private float speed = 30f;
    [SerializeField] private bool loop = false;  // repetir el recorrido
    [SerializeField] private float oscillationAngle = 10f; 
    [SerializeField] private float oscillationSpeed = 10f;  

    private List<Transform> pointsList = new List<Transform>();
    private int currentIndexPoint = 0;
    private bool isMoving = true;

    private bool oscilatingUpwards;
    private float currentAngle = 0f;

    private int level; //para saber que mapa poner y seguir, es (levelID - 1)
    void Start()
    {
        if (SceneLoader.Instance != null)
            level = SceneLoader.Instance.getCurrentLevelId(EventRegister.TipoJuego.MisionColombia) - 1;
        else
            level = 1;

        // activamos solo el background que sea del nivel, el resto los desactivamos
        for (int i = 0; i < mapBackgrounds.transform.childCount; i++)
        {
            bool isCurrent = (i == level);
            mapBackgrounds.transform.GetChild(i).gameObject.SetActive(isCurrent);
        }

        // path correspondiente. todo esto tiene que estar bien puesto en la escena sus posiciones
        //PathPointsParent
        //------Level1Points
        //----------Lista de puntos hijos
        for (int i = 0; i < pathPointsParent.transform.childCount; i++)
        {
            pathPointsParent.transform.GetChild(i).gameObject.SetActive(i == level);
        }


        pointsList = new List<Transform>();
        Transform currentPath = pathPointsParent.transform.GetChild(level);

        foreach (Transform child in currentPath)
        {
            if (child.gameObject.activeSelf) // por si algún punto está desactivado, pero vamos que entonces para qué lo pondrias
            {
                //tienen imagenes para poder colocarlos mas rapido en el inspector, ahora las quitamos
                if(child.gameObject.GetComponent<Image>() != null)
                    child.gameObject.GetComponent<Image>().enabled = false; //para que no se vean los puntos

                pointsList.Add(child);
            }
        }

        if (pointsList.Count > 0)
        {
            truck.transform.position = pointsList[0].position; //pos inicial truck
        }
    }

    void Update()
    {
        //Vector3 temp = new Vector3(7.0f, 0, 0);
       // truck.transform.position += temp;
        if (isMoving)
        {
            Moving();
        }
        Oscila(); //siempre deberia oscilar, en mi opinion

    }

    private void Moving()
    {
        if (pointsList.Count < 2) return;
      
        Vector3 targetPos = pointsList[currentIndexPoint + 1].position;

        truck.transform.position = Vector3.MoveTowards(truck.transform.position, targetPos, speed * Time.deltaTime);


        if (Vector3.Distance(truck.transform.position, targetPos) < 1f) //si esta en el target, cambia de target al siguiente
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
        if (oscilatingUpwards)
        {
            currentAngle += oscillationSpeed * Time.deltaTime;
            if (currentAngle >= oscillationAngle) oscilatingUpwards = false;
        }
        else
        {
            currentAngle -= oscillationSpeed * Time.deltaTime;
            if (currentAngle <= -oscillationAngle) oscilatingUpwards = true;
        }

        truck.transform.rotation = Quaternion.Euler(0, 0, currentAngle);

    }

}
