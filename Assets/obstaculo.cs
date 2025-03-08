using UnityEngine;

public class Obstaculo : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    MatrixCubeInfo m_CubeInfo;
    public float m_Vel = 10f;

    void Start()
    {
        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CubeInfo = GetComponent<MatrixCubeInfo>();
    }

    public void SetVel(float obsVel)
    {
        m_Vel = obsVel;
    }

    void FixedUpdate()
    {
        m_Rigidbody.linearVelocity = Vector3.left * m_Vel;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<DolphinController>() != null)
        {
            Vector3 aux = Vector3.zero;
            aux.x = m_Vel / 10;
            aux.y = -m_Vel;
            m_Rigidbody.linearVelocity = aux;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<DolphinController>() == null && other.gameObject.GetComponent<MatrixCubeInfo>() != null)
        {
            //Obstáculo ocupa nueva casilla en matriz
            MatrixCubeInfo cubeInfo = other.gameObject.GetComponent<MatrixCubeInfo>();
            Vector2 newCubePos = cubeInfo.GetXY();
            Vector2 oldCubePos = m_CubeInfo.GetXY();
            DolphinLevelManager.Instance.SetOccupation((int)oldCubePos.x, (int)oldCubePos.y, Box.Empty);
            m_CubeInfo.SetXY((int)newCubePos.x, (int)newCubePos.y);
            DolphinLevelManager.Instance.SetOccupation((int)newCubePos.x, (int)newCubePos.y, Box.Obstacle);
        }
    }

    public void DestroyObstacle()
    {
        Destroy(gameObject);
    }
}