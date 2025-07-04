using System;
using UnityEngine;
using static DolphinController;

public class ObstaculoPelota: MonoBehaviour
{

    public Vector3 m_Vel;
    private Rigidbody m_Rigidbody;

    bool interacted = false;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (!interacted)
        {
            DolphinLevelManager.Instance.BallMiss(transform.position);
            interacted = true;
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.BHundido, ""));
        }
    }

    //ON CLICK "REBOTE" vel en y * -1
    public void TryClickBall()
    {
        if (!interacted) //RIGHT GUESS SPECIAL JUMP
        {
            //In world points text
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.BTocado, ""));
            DolphinLevelManager.Instance.BallHit(transform.position);
            //Rebota
            m_Rigidbody.linearVelocity = new Vector3(m_Rigidbody.linearVelocity.x, -m_Rigidbody.linearVelocity.y * 2, m_Rigidbody.linearVelocity.z);
            interacted = true;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log("Ball clicked");
                    TryClickBall();
                }
            }
        }
    }

    public void SetVelocity(Vector3 obsVel)
    {
        m_Vel = obsVel;
        m_Rigidbody.linearVelocity = m_Vel;
    }


    // Autodestruccion de objeto
    public void DestroyObstacle()
    {
        DolphinLevelManager.Instance.DeregisterObject(gameObject);
        Destroy(gameObject);
    }
}

/*
     * Pelota 
    Queremos un nuevo tipo de objeto al que el usuario deba reaccionar. Este objeto será una 
    pelota, que caerá desde el cielo con una parábola hacia la zona de los delfines. El usuario 
    debe tocarla con el dedo antes de que toque el agua para ganar puntos. La cantidad de puntos 
    no es relevante de momento. 
    Este objeto tendrá una malla de pelota y aparecerá aleatoriamente según unos parámetros que 
    debéis definir. Estos parámetros deben ser configurables desde la pestaña del Inspector de 
    Unity para facilitar las pruebas.
*/