using UnityEngine;

public class MovingObject : MonoBehaviour
{
    protected Rigidbody m_Rigidbody;
    protected MatrixCubeInfo m_CubeInfo;

    protected Box type; // Tipos actuales: Floatie, Obstacle

    public float m_Vel = 10f;
    public bool collisionReaction;

    protected void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CubeInfo = GetComponent<MatrixCubeInfo>();
        type = Box.Empty;
        collisionReaction = false;
        m_Rigidbody.linearVelocity = Vector3.left * m_Vel;
    }

    public virtual void SetVel(float obsVel)
    {
        m_Vel = obsVel;
    }

    protected void FixedUpdate()
    {
        if (!m_Rigidbody.isKinematic)
            m_Rigidbody.linearVelocity = Vector3.left * m_Vel;

    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (collisionReaction)
        {
            // Si colisiona con un Delfin
            if (collision.gameObject.GetComponent<DolphinController>() != null)
            {
                Vector3 aux = Vector3.zero;
                aux.x = m_Vel / 10;
                aux.y = -m_Vel;
                m_Rigidbody.linearVelocity = aux;
            }
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<DolphinController>() == null && other.gameObject.GetComponent<MatrixCubeInfo>() != null)
        {
            MatrixCubeInfo cubeInfo = other.gameObject.GetComponent<MatrixCubeInfo>();
            Vector2 newCubePos = cubeInfo.GetXY();
            Vector2 oldCubePos = m_CubeInfo.GetXY();

            // Solo se borra si tiene posición en el mapa (caso particular: compartir momentáneamente casilla con delfines)
            //if (oldCubePos.x != -1) DolphinLevelManager.Instance.SetOccupation((int)oldCubePos.x, (int)oldCubePos.y, Box.Empty); 
            m_CubeInfo.SetXY((int)newCubePos.x, (int)newCubePos.y);

            // Objeto ocupa nueva casilla en matriz
            //if (DolphinLevelManager.Instance.GetOccupationFromMatrix((int)newCubePos.x, (int)newCubePos.y) == Box.Empty) // Para que no quite la posición al delfín sin querer
            //{
            //    DolphinLevelManager.Instance.SetOccupation((int)newCubePos.x, (int)newCubePos.y, type);
            //}
        }
    }

    // Autodestruccion de objeto
    public void DestroyObstacle()
    {
        DolphinLevelManager.Instance.DeregisterObject(gameObject);
        Destroy(gameObject);
    }
}