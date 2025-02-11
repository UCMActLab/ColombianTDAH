using UnityEngine;

public class enviroMov : MonoBehaviour
{
    Transform m_Tr;
    public float m_Vel = 2f;

    void Start()
    {
        m_Tr = GetComponent<Transform>();
    }

    void FixedUpdate()
    {
        m_Tr.Rotate(0.0f, m_Vel*1.0f, 0.0f, Space.Self);
    }
}