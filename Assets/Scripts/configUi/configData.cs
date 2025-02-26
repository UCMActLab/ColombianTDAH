using UnityEngine;

[CreateAssetMenu(fileName = "configData", menuName = "Scriptable Objects/configData")]

public class configData : ScriptableObject
{
    public string prefabName;

    public int numberOfPrefabsToCreate;
    public Vector3[] spawnPoints;
}
