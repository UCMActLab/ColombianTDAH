using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlanificationList : MonoBehaviour
{
    [SerializeField]
    GameObject _departureText;
    [SerializeField]
    GameObject _stopsText;
    [SerializeField]
    GameObject _sleepText;
    [SerializeField]
    GameObject _locationText;

    [SerializeField]
    GameObject _textPrefab;

    [SerializeField]
    Sprite _tick;
    [SerializeField]
    Sprite _cross;

    Vector3 _offset = 12 * Vector3.down;
    float _distance = 10f;

    public void SetDepartureTime(string newTime)
    {
        _departureText.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = newTime;
    }

    public void SetStops(List<string> stops)
    {
        SetSelected(stops, _stopsText);
    }

    public void SetSleepHours(List<string> sleepHours)
    {
        SetSelected(sleepHours, _sleepText);
    }

    public void SetLocationHours(List<string> locationHours)
    {
        SetSelected(locationHours, _locationText);
    }

    private void SetSelected(List<string> selectedList, GameObject gObject)
    {
        RemoveChildren(gObject);
        AddText(selectedList, gObject);
    }

    private void AddText(List<string> namesToAdd, GameObject gObject)
    {

        Transform parentTransform = gObject.transform;
        for (int i = 0; i < namesToAdd.Count; i++)
        {
            Vector3 pos = (parentTransform.position + _offset + _distance * i * Vector3.down);
            GameObject childGameObject = Instantiate(_textPrefab, pos, Quaternion.identity, parentTransform);
            childGameObject.GetComponentInChildren<TextMeshProUGUI>().text = namesToAdd[i];
        }
    }

    private void RemoveChildren(GameObject gObject)
    {
        while (gObject.transform.childCount != 0)
        {
            DestroyImmediate(gObject.transform.GetChild(0).gameObject);
        }
    }
}
