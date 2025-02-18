using UnityEngine;

public class Obstaculo : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    public float m_Vel = 10f;

    void Start()
    {
        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        m_Rigidbody.linearVelocity = Vector3.left * m_Vel;
    }

    void OnCollisionEnter(Collision collision)
    {
        Vector3 aux = Vector3.zero;
        aux.x = m_Vel / 10;
        aux.y = -m_Vel;
        m_Rigidbody.linearVelocity = aux;
    }
}