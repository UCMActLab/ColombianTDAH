using UnityEngine;

public class ToggleGameObjectPatientInfo : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    private void Start()
    {
        if (EventRegister.Instance.PacientInfoIsRegistered)
        {
            gameObject.SetActive(true);

        }
        else
            gameObject.SetActive(false);

    }
    public void ToggleActive()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }
}
