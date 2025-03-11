using UnityEngine;

public class MovingObject : MonoBehaviour
{
    protected Rigidbody m_Rigidbody;
    protected MatrixCubeInfo m_CubeInfo;
    protected Box type;
    public float m_Vel = 10f;
    public bool collisionReaction;

    protected void Start()
    {
        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CubeInfo = GetComponent<MatrixCubeInfo>();
        type = Box.Empty;
        collisionReaction = false;
        m_Rigidbody.linearVelocity = Vector3.left * m_Vel;
    }

    public void SetVel(float obsVel)
    {
        m_Vel = obsVel;
    }

    protected void FixedUpdate()
    {
        m_Rigidbody.linearVelocity = Vector3.left * m_Vel;
        
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if(collisionReaction)
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
            //Obstáculo ocupa nueva casilla en matriz
            MatrixCubeInfo cubeInfo = other.gameObject.GetComponent<MatrixCubeInfo>();
            Vector2 newCubePos = cubeInfo.GetXY();
            Vector2 oldCubePos = m_CubeInfo.GetXY();
            DolphinLevelManager.Instance.SetOccupation((int)oldCubePos.x, (int)oldCubePos.y, Box.Empty);
            m_CubeInfo.SetXY((int)newCubePos.x, (int)newCubePos.y);
            DolphinLevelManager.Instance.SetOccupation((int)newCubePos.x, (int)newCubePos.y, type);
        }
    }

    public void DestroyObstacle()
    {
        DolphinLevelManager.Instance.DeregisterObject(gameObject);
        Destroy(gameObject);
    }


}