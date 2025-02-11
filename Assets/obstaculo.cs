using UnityEngine;

public class Obstaculo : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    public float m_Vel = 20f;

    void Start()
    {
        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        m_Rigidbody.linearVelocity = Vector3.left * m_Vel;
    }
}