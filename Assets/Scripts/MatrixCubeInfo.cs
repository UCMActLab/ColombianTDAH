using UnityEngine;

public class MatrixCubeInfo : MonoBehaviour
{
    // Position in the matrix
    public int _x, _y;
   

    public void SetXY(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public Vector2 GetXY()
    {
        return new Vector2( _x, _y );
    }
}
